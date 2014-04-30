// XInputWrapper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "lusb0_usb.h"
#include "XInput_XboxOne.h"

#include <conio.h>
#include <iostream>
#include <stdint.h>
#include <stdio.h>
#include <time.h>

struct XboxOneControllerHandler
{
	struct usb_dev_handle *handle;
	bool isConnected;
	XBOXONE_STATE controllerState;

	uint8_t lastState[64];
	unsigned int tickCount;

	XINPUT_GAMEPAD lastGamepadState;
};

XboxOneControllerHandler *controllerHandler[4] = { NULL, NULL, NULL, NULL };
HANDLE XboxOneControllerThread[4] = { 0 };
HANDLE XboxOneControllerMutex[4] = { 0 };

static unsigned short idVendor = 0x045E;
static unsigned short idProduct = 0x02D1;

int configuration = 1;
int interface = 0;
int endpointIn = 0x81;
int endpointOut = 0x01;
int timeout = 2000; // milliseconds 
bool controllerInit = false;
bool runThread = true;

// Structure we receive from the controller
struct XboxOneControllerState
{
	char eventCount;
	char unknown;
	char buttons1;
	char buttons2;
	short leftTrigger;  // Triggers are 0 - 1023
	short rightTrigger;
	short thumbLX;      // Axes are -32767 - 32767
	short thumbLY;
	short thumbRX;
	short thumbRY;
};

FILE *stream = NULL;
HANDLE mutexLogger = NULL;
void writeLog(char *tag, char *format, ...)
{
#ifdef _DEBUG
	time_t now;
	va_list args;
	char timeArray[128];

	int dwWaitResult = WaitForSingleObject(mutexLogger, INFINITE);
	if (dwWaitResult == WAIT_OBJECT_0)
	{

		if (stream)
		{
			time(&now);
			ctime_s(timeArray, 128, &now);
			timeArray[strlen(timeArray) - 1] = 0x00;
			fprintf(stream, "%s [%s]: ", timeArray, tag);

			va_start(args, format);
			vfprintf(stream, format, args);
			va_end(args);

			fflush(stream);
		}
		ReleaseMutex(mutexLogger);
	}
#endif
}

int iround(double num) {
	return (num > 0.0) ? (int)floor(num + 0.5) : (int)ceil(num - 0.5);
}

DWORD WINAPI updateState(void* dwUserIndexPointer)
{
	int dwUserIndex = *static_cast<int*>(dwUserIndexPointer);
	struct usb_dev_handle *handle = controllerHandler[dwUserIndex]->handle;
	uint8_t raw_data[64];

	writeLog("updateState", "start dwUserIndex = %d\n", dwUserIndex);
	while (runThread)
	{
		writeLog("updateState", "usb_interrupt_read call = %d dwUserIndex = %d\n", dwUserIndex);
		memset(raw_data, 0, 64);
		int ret = usb_interrupt_read(handle, endpointIn, (char*)raw_data, sizeof(raw_data), 500);
		if (ret < 0)
		{
			writeLog("updateState", "usb_interrupt_read fail error = %d dwUserIndex = %d\n", ret, dwUserIndex);
		}

		int dwWaitResult = WaitForSingleObject(XboxOneControllerMutex[dwUserIndex], INFINITE);

		if (dwWaitResult == WAIT_OBJECT_0)
		{
			if (memcmp(controllerHandler[dwUserIndex]->lastState, raw_data, 64) != 0)
			{
				controllerHandler[dwUserIndex]->tickCount = (controllerHandler[dwUserIndex]->tickCount != UINT_MAX) ? controllerHandler[dwUserIndex]->tickCount + 1 : 0;
				writeLog("updateState", "State changed dwUserIndex = %d tickCount = %d\n", dwUserIndex, controllerHandler[dwUserIndex]->tickCount);
			}
			memcpy(controllerHandler[dwUserIndex]->lastState, raw_data, 64);

			char tag = raw_data[0];
			char code = raw_data[1];
			char data[62];

			int i = 0;
			for (int j = 2; j < 64; ++j)
			{
				data[i] = raw_data[j];
				i++;
			}

			switch (tag)
			{
			case 0x07:
				if ((data[2] & 0x01) != 0)
				{
					controllerHandler[dwUserIndex]->controllerState.guideButton = 1;
				}
				break;
			case 0x20:
				XboxOneControllerState *reportedState = (XboxOneControllerState*)data;

				controllerHandler[dwUserIndex]->controllerState.guideButton = 0;

				// char *buttons1_index[] = {"Sync", "Unknown", "Menu", "View", "A", "B", "X", "Y"};
				controllerHandler[dwUserIndex]->controllerState.menu = ((reportedState->buttons1&(1 << 2)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.view = ((reportedState->buttons1&(1 << 3)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.aButton = ((reportedState->buttons1&(1 << 4)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.bButton = ((reportedState->buttons1&(1 << 5)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.xButton = ((reportedState->buttons1&(1 << 6)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.yButton = ((reportedState->buttons1&(1 << 7)) != 0) ? 1 : 0;

				// char *buttons2_index[] = {"Up", "Down", "Left", "Right", "Left Shoulder", "Right Shoulder", "Left Stick (Pressed)", "Right Stick (Pressed)"};
				controllerHandler[dwUserIndex]->controllerState.up = ((reportedState->buttons2&(1 << 0)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.down = ((reportedState->buttons2&(1 << 1)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.left = ((reportedState->buttons2&(1 << 2)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.right = ((reportedState->buttons2&(1 << 3)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.leftShoulder = ((reportedState->buttons2&(1 << 4)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.rightShoulder = ((reportedState->buttons2&(1 << 5)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.leftThumb = ((reportedState->buttons2&(1 << 6)) != 0) ? 1 : 0;
				controllerHandler[dwUserIndex]->controllerState.rightThumb = ((reportedState->buttons2&(1 << 7)) != 0) ? 1 : 0;

				// Triggers are 0 - 1023 (need to be 0 - 255)
				controllerHandler[dwUserIndex]->controllerState.leftTrigger = reportedState->leftTrigger > 0 ? iround((reportedState->leftTrigger / (float)1023) * 255) : 0;
				controllerHandler[dwUserIndex]->controllerState.rightTrigger = reportedState->rightTrigger > 0 ? iround((reportedState->rightTrigger / (float)1023) * 255) : 0;

				// Axes are -32767 - 32767 (as expected)
				controllerHandler[dwUserIndex]->controllerState.thumbLX = reportedState->thumbLX;
				controllerHandler[dwUserIndex]->controllerState.thumbLY = reportedState->thumbLY;
				controllerHandler[dwUserIndex]->controllerState.thumbRX = reportedState->thumbRX;
				controllerHandler[dwUserIndex]->controllerState.thumbRY = reportedState->thumbRY;
				break;
			}
			ReleaseMutex(XboxOneControllerMutex[dwUserIndex]);
		}
	}
	writeLog("updateState", "stop\n");
	return true;
}

void vibrate(int leftTriggerVal, int rightTriggerVal, int leftVal, int rightVal, int dwUserIndex)
{
	writeLog("vibrate", "start leftTriggerVal = %d rightTriggerVal = %d leftVal = %d rightVal = %d dwUserIndex = %d \n", leftTriggerVal, rightTriggerVal, leftVal, rightVal, dwUserIndex);
	// Motors are 0 - 255
	unsigned char data[] = { 9, 0, 0, 9, 0, 15, leftTriggerVal, rightTriggerVal, leftVal, rightVal, 255, 0, 0 };
	int dwWaitResult = WaitForSingleObject(XboxOneControllerMutex[dwUserIndex], INFINITE);
	if (dwWaitResult == WAIT_OBJECT_0)
	{
		usb_interrupt_write(controllerHandler[dwUserIndex]->handle, endpointOut, (char*)data, sizeof(data), 500);
		ReleaseMutex(XboxOneControllerMutex[dwUserIndex]);
	}
	writeLog("vibrate", "stop\n");
}

bool connectController(bool enable)
{
	int xboxControllerCounter = 0;

	writeLog("connectController", "enable = %d && controllerInit = %d\n", enable, controllerInit);
	if (enable && !controllerInit)
	{
#ifdef _DEBUG
		fopen_s(&stream, "XboxOneController.log", "w");
		mutexLogger = CreateMutex(NULL, FALSE, NULL);
#endif
		usb_init();
		usb_find_busses();
		usb_find_devices();
		writeLog("connectController", "usb_init - usb_find_busses - usb_find_devices\n");
		struct usb_bus *busses = usb_get_busses();

		for (struct usb_bus *bus = busses; bus; bus = bus->next)
		{
			for (struct usb_device *dev = bus->devices; dev; dev = dev->next)
			{
				if (dev->descriptor.idVendor == idVendor && dev->descriptor.idProduct == idProduct && xboxControllerCounter < 4)
				{
					controllerHandler[xboxControllerCounter] = (XboxOneControllerHandler*)malloc(sizeof(XboxOneControllerHandler));
					memset(controllerHandler[xboxControllerCounter], 0, sizeof(XboxOneControllerHandler));

					controllerHandler[xboxControllerCounter]->handle = usb_open(dev);
					writeLog("connectController", "usb_open - xboxControllerCounter = %d\n", xboxControllerCounter);
					if (usb_set_configuration(controllerHandler[xboxControllerCounter]->handle, configuration) < 0)
					{
						writeLog("connectController", "usb_set_configuration failed\n");
						return false;
					}

					if (usb_claim_interface(controllerHandler[xboxControllerCounter]->handle, interface) < 0)
					{
						writeLog("connectController", "usb_claim_interface failed\n");
						return false;
					}

					// Initialise
					char data[] = { 0x05, 0x20 };
					if (usb_interrupt_write(controllerHandler[xboxControllerCounter]->handle, endpointOut, (char*)data, sizeof(data), timeout) < 0)
					{
						writeLog("connectController", "usb_interrupt_write failed\n");
						return false;
					}

					controllerHandler[xboxControllerCounter]->isConnected = true;
					int* id = new int(xboxControllerCounter);
					XboxOneControllerThread[xboxControllerCounter] = CreateThread(NULL, 0, updateState, id, 0, NULL);

					XboxOneControllerMutex[xboxControllerCounter] = CreateMutex(NULL, FALSE, NULL);
					writeLog("connectController", "xboxControllerCounter = %d => Connected\n", xboxControllerCounter);
					xboxControllerCounter++;
				}
			}
		}
		controllerInit = true;
	}
	else if (!enable && controllerInit)
	{
		writeLog("connectController", "free all handler...\n");

		runThread = false;
		WaitForMultipleObjects(3,
			XboxOneControllerThread, TRUE, INFINITE);

		while (xboxControllerCounter < 4)
		{
			if (XboxOneControllerThread[xboxControllerCounter])
				CloseHandle(XboxOneControllerThread[xboxControllerCounter]);

			if (controllerHandler[xboxControllerCounter])
			{
				CloseHandle(XboxOneControllerThread[xboxControllerCounter]);
				usb_release_interface(controllerHandler[xboxControllerCounter]->handle, interface);
				usb_close(controllerHandler[xboxControllerCounter]->handle);
				free(controllerHandler[xboxControllerCounter]);
				writeLog("connectController", "free handler xboxControllerCounter = %d done\n", xboxControllerCounter);
			}
			xboxControllerCounter++;
		}
		writeLog("connectController", "free handler completed\n");
#ifdef _DEBUG
		fclose(stream);
#endif
		controllerInit = false;
	}
	writeLog("connectController", "return true\n");
	return true;
}

DWORD WINAPI XInputGetState
(
__in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
__out XINPUT_STATE* pState								// Receives the current state
)
{
	writeLog("XInputGetState", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}
	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		int dwWaitResult = WaitForSingleObject(XboxOneControllerMutex[dwUserIndex], 100);
		XINPUT_GAMEPAD gamepadState = { 0 };

		if (dwWaitResult == WAIT_OBJECT_0)
		{
			writeLog("XInputGetState", "Get the Mutex - dwUserIndex = %d \n", controllerInit, dwUserIndex);
			gamepadState.bLeftTrigger = controllerHandler[dwUserIndex]->controllerState.leftTrigger;
			gamepadState.bRightTrigger = controllerHandler[dwUserIndex]->controllerState.rightTrigger;
			gamepadState.sThumbLX = controllerHandler[dwUserIndex]->controllerState.thumbLX;
			gamepadState.sThumbLY = controllerHandler[dwUserIndex]->controllerState.thumbLY;
			gamepadState.sThumbRX = controllerHandler[dwUserIndex]->controllerState.thumbRX;
			gamepadState.sThumbRY = controllerHandler[dwUserIndex]->controllerState.thumbRY;

			gamepadState.wButtons = 0;

			if (controllerHandler[dwUserIndex]->controllerState.view) gamepadState.wButtons |= XINPUT_GAMEPAD_BACK;
			if (controllerHandler[dwUserIndex]->controllerState.leftThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_THUMB;
			if (controllerHandler[dwUserIndex]->controllerState.rightThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_THUMB;
			if (controllerHandler[dwUserIndex]->controllerState.menu)			gamepadState.wButtons |= XINPUT_GAMEPAD_START;

			if (controllerHandler[dwUserIndex]->controllerState.up) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_UP;
			if (controllerHandler[dwUserIndex]->controllerState.right) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_RIGHT;
			if (controllerHandler[dwUserIndex]->controllerState.down) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_DOWN;
			if (controllerHandler[dwUserIndex]->controllerState.left) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_LEFT;

			if (controllerHandler[dwUserIndex]->controllerState.leftShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_SHOULDER;
			if (controllerHandler[dwUserIndex]->controllerState.rightShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_SHOULDER;

			if (controllerHandler[dwUserIndex]->controllerState.yButton) gamepadState.wButtons |= XINPUT_GAMEPAD_Y;
			if (controllerHandler[dwUserIndex]->controllerState.bButton) gamepadState.wButtons |= XINPUT_GAMEPAD_B;
			if (controllerHandler[dwUserIndex]->controllerState.aButton) gamepadState.wButtons |= XINPUT_GAMEPAD_A;
			if (controllerHandler[dwUserIndex]->controllerState.xButton) gamepadState.wButtons |= XINPUT_GAMEPAD_X;
			//if (controllerHandler[dwUserIndex]->controllerState.guideButton) gamepadState.wButtons |= XINPUT_GAMEPAD_GUIDE;

			pState->dwPacketNumber = controllerHandler[dwUserIndex]->tickCount;
			ReleaseMutex(XboxOneControllerMutex[dwUserIndex]);
		}
		pState->Gamepad = gamepadState;
		writeLog("XInputGetState", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputGetState", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD WINAPI XInputSetState
(
__in DWORD             dwUserIndex,						// Index of the gamer associated with the device
__in XINPUT_VIBRATION* pVibration						// The vibration information to send to the controller
)
{
	writeLog("XInputSetState", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		// We're receiving as XInput [0 ~ 65535], need to be [0 ~ 255] !!
		int leftVal = iround(((float)pVibration->wLeftMotorSpeed / 65535) * 255);
		int rightVal = iround(((float)pVibration->wRightMotorSpeed / 65535) * 255);
		vibrate(0, 0, leftVal, rightVal, dwUserIndex);
		writeLog("XInputSetState", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputSetState", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD WINAPI XInputGetCapabilities
(
__in  DWORD                dwUserIndex,					// Index of the gamer associated with the device
__in  DWORD                dwFlags,						// Input flags that identify the device type
__out XINPUT_CAPABILITIES* pCapabilities				// Receives the capabilities
)
{
	writeLog("XInputGetCapabilities", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (dwFlags > XINPUT_FLAG_GAMEPAD)
	{
		writeLog("XInputGetCapabilities", "return ERROR_BAD_ARGUMENTS\n");
		return ERROR_BAD_ARGUMENTS;
	}

	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		pCapabilities->Flags = XINPUT_CAPS_VOICE_SUPPORTED;
		pCapabilities->Type = XINPUT_DEVTYPE_GAMEPAD;
		pCapabilities->SubType = XINPUT_DEVSUBTYPE_GAMEPAD;

		pCapabilities->Gamepad.wButtons = 0xF3FF;

		pCapabilities->Gamepad.bLeftTrigger = 0xFF;
		pCapabilities->Gamepad.bRightTrigger = 0xFF;

		pCapabilities->Gamepad.sThumbLX = (SHORT)0xFFC0;
		pCapabilities->Gamepad.sThumbLY = (SHORT)0xFFC0;
		pCapabilities->Gamepad.sThumbRX = (SHORT)0xFFC0;
		pCapabilities->Gamepad.sThumbRY = (SHORT)0xFFC0;

		pCapabilities->Vibration.wLeftMotorSpeed = 0xFF;
		pCapabilities->Vibration.wRightMotorSpeed = 0xFF;

		writeLog("XInputGetCapabilities", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputGetCapabilities", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

void WINAPI XInputEnable
(
__in bool enable										// [in] Indicates whether xinput is enabled or disabled. 
)
{
	writeLog("XInputEnable", "controllerInit = %d - enable = %d \n", controllerInit, enable);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (controllerInit && !enable)
	{
		XINPUT_VIBRATION Vibration = { 0, 0 };
		int xboxControllerCounter = 0;

		while (xboxControllerCounter < 4)
		{
			if (controllerHandler[xboxControllerCounter])
			{
				XInputSetState(xboxControllerCounter, &Vibration);
			}
			xboxControllerCounter++;
		}
	}
}

DWORD WINAPI XInputGetDSoundAudioDeviceGuids
(
__in  DWORD dwUserIndex,								// Index of the gamer associated with the device
__out GUID* pDSoundRenderGuid,							// DSound device ID for render
__out GUID* pDSoundCaptureGuid							// DSound device ID for capture
)
{
	writeLog("XInputGetDSoundAudioDeviceGuids", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		pDSoundRenderGuid = NULL;
		pDSoundCaptureGuid = NULL;

		writeLog("XInputGetDSoundAudioDeviceGuids", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputGetDSoundAudioDeviceGuids", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD XInputGetBatteryInformation
(
__in  DWORD                       dwUserIndex,			// Index of the gamer associated with the device
__in  BYTE                        devType,				// Which device on this user index
__out XINPUT_BATTERY_INFORMATION* pBatteryInformation	// Contains the level and types of batteries
)
{
	writeLog("XInputGetBatteryInformation", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		pBatteryInformation->BatteryType = BATTERY_TYPE_WIRED;
		pBatteryInformation->BatteryLevel = BATTERY_LEVEL_FULL;

		writeLog("XInputGetBatteryInformation", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputGetBatteryInformation", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD WINAPI XInputGetKeystroke
(
__in       DWORD dwUserIndex,							// Index of the gamer associated with the device
__reserved DWORD dwReserved,							// Reserved for future use
__out      PXINPUT_KEYSTROKE pKeystroke					// Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
)
{
	writeLog("XInputGetKeystroke", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		// TODO: pKeystroke
		writeLog("XInputGetKeystroke", "return ERROR_EMPTY\n");
		return ERROR_EMPTY; // or ERROR_SUCCESS
	}
	else
	{
		writeLog("XInputGetKeystroke", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

// Undocumented

DWORD WINAPI XInputGetStateEx
(
__in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
__out XINPUT_STATE* pState								// Receives the current state
)
{
	writeLog("XInputGetStateEx", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}
	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		int dwWaitResult = WaitForSingleObject(XboxOneControllerMutex[dwUserIndex], 100);
		XINPUT_GAMEPAD gamepadState = { 0 };

		if (dwWaitResult == WAIT_OBJECT_0)
		{
			writeLog("XInputGetStateEx", "Get the Mutex - dwUserIndex = %d \n", controllerInit, dwUserIndex);
			gamepadState.bLeftTrigger = controllerHandler[dwUserIndex]->controllerState.leftTrigger;
			gamepadState.bRightTrigger = controllerHandler[dwUserIndex]->controllerState.rightTrigger;
			gamepadState.sThumbLX = controllerHandler[dwUserIndex]->controllerState.thumbLX;
			gamepadState.sThumbLY = controllerHandler[dwUserIndex]->controllerState.thumbLY;
			gamepadState.sThumbRX = controllerHandler[dwUserIndex]->controllerState.thumbRX;
			gamepadState.sThumbRY = controllerHandler[dwUserIndex]->controllerState.thumbRY;

			gamepadState.wButtons = 0;

			if (controllerHandler[dwUserIndex]->controllerState.view) gamepadState.wButtons |= XINPUT_GAMEPAD_BACK;
			if (controllerHandler[dwUserIndex]->controllerState.leftThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_THUMB;
			if (controllerHandler[dwUserIndex]->controllerState.rightThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_THUMB;
			if (controllerHandler[dwUserIndex]->controllerState.menu)			gamepadState.wButtons |= XINPUT_GAMEPAD_START;

			if (controllerHandler[dwUserIndex]->controllerState.up) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_UP;
			if (controllerHandler[dwUserIndex]->controllerState.right) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_RIGHT;
			if (controllerHandler[dwUserIndex]->controllerState.down) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_DOWN;
			if (controllerHandler[dwUserIndex]->controllerState.left) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_LEFT;

			if (controllerHandler[dwUserIndex]->controllerState.leftShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_SHOULDER;
			if (controllerHandler[dwUserIndex]->controllerState.rightShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_SHOULDER;

			if (controllerHandler[dwUserIndex]->controllerState.yButton) gamepadState.wButtons |= XINPUT_GAMEPAD_Y;
			if (controllerHandler[dwUserIndex]->controllerState.bButton) gamepadState.wButtons |= XINPUT_GAMEPAD_B;
			if (controllerHandler[dwUserIndex]->controllerState.aButton) gamepadState.wButtons |= XINPUT_GAMEPAD_A;
			if (controllerHandler[dwUserIndex]->controllerState.xButton) gamepadState.wButtons |= XINPUT_GAMEPAD_X;

			if (controllerHandler[dwUserIndex]->controllerState.guideButton) gamepadState.wButtons |= XINPUT_GAMEPAD_GUIDE;

			pState->dwPacketNumber = controllerHandler[dwUserIndex]->tickCount;
			ReleaseMutex(XboxOneControllerMutex[dwUserIndex]);
		}
		pState->Gamepad = gamepadState;
		writeLog("XInputGetStateEx", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputGetStateEx", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD WINAPI XInputSetStateEx
(
__in DWORD             dwUserIndex,						// Index of the gamer associated with the device
__in XINPUT_VIBRATION_EX* pVibration					// The vibration information to send to the controller
)
{
	writeLog("XInputSetStateEx", "controllerInit = %d - dwUserIndex = %d \n", controllerInit, dwUserIndex);
	if (!controllerInit)
	{
		connectController(true);
	}

	if (controllerInit && dwUserIndex >= 0 && dwUserIndex < 4 && controllerHandler[dwUserIndex])
	{
		// We're receiving as XInput [0 ~ 65535], need to be [0 ~ 255] !!
		int leftTriggerVal = iround(((float)pVibration->wLeftTriggerMotorSpeed / 65535) * 255);
		int rightTriggerVal = iround(((float)pVibration->wRightTriggerMotorSpeed / 65535) * 255);
		int leftVal = iround(((float)pVibration->wLeftMotorSpeed / 65535) * 255);
		int rightVal = iround(((float)pVibration->wRightMotorSpeed / 65535) * 255);

		vibrate(leftTriggerVal, rightTriggerVal, leftVal, rightVal, dwUserIndex);

		writeLog("XInputSetStateEx", "return ERROR_SUCCESS\n");
		return ERROR_SUCCESS;
	}
	else
	{
		writeLog("XInputSetStateEx", "return ERROR_DEVICE_NOT_CONNECTED\n");
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}