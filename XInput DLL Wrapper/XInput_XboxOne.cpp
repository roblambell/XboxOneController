// XInputWrapper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "lusb0_usb.h"
#include "XInput_XboxOne.h"

#include <conio.h>
#include <iostream>
#include <stdint.h>
#include <stdio.h>

struct usb_bus *busses;
struct usb_bus *bus;
struct usb_device *dev;
struct usb_dev_handle *handle;

static unsigned short idVendor  = 0x045E;
static unsigned short idProduct = 0x02D1;

int configuration  = 1;
int interface      = 0;
int endpointIn     = 0x81;
int endpointOut    = 0x01;
int timeout        = 5000; // milliseconds

bool isConnected = false;

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

XboxOneControllerState *reportedState;
XBOXONE_STATE controllerState;

int iround(double num) {
	return (num > 0.0) ? (int)floor(num + 0.5) : (int)ceil(num - 0.5);
}

bool updateState()
{
	uint8_t raw_data[64];

	int ret = usb_interrupt_read(handle, endpointIn, (char*)raw_data, sizeof(raw_data), timeout);
	if (ret < 0)
	{
		return false;
	}

	char tag = raw_data[0];
	char code = raw_data[1];
	char data[62];

	int i = 0;
	for(int j = 2; j < 64; ++j)
	{
		data[i] = raw_data[j];
		i++;
	}

	controllerState.guideButton = 0;

	controllerState.menu = 0;
	controllerState.view = 0;
	controllerState.aButton = 0;
	controllerState.bButton = 0;
	controllerState.xButton = 0;
	controllerState.yButton = 0;

	controllerState.up = 0;
	controllerState.down = 0;
	controllerState.left = 0;
	controllerState.right = 0;
	controllerState.leftShoulder = 0;
	controllerState.rightShoulder = 0;
	controllerState.leftThumb = 0;
	controllerState.rightThumb = 0;

	controllerState.leftTrigger = 0;
	controllerState.rightTrigger = 0;

	controllerState.thumbLX = 0;
	controllerState.thumbLY = 0;
	controllerState.thumbRX = 0;
	controllerState.thumbRY = 0;

	switch(tag)
	{
		case 0x07:
			if((data[2]&0x01) != 0)
			{
				controllerState.guideButton = 1;
			}
			break;
		case 0x20:
			XboxOneControllerState *reportedState = (XboxOneControllerState*)data;

			controllerState.guideButton = 0;

			// char *buttons1_index[] = {"Sync", "Unknown", "Menu", "View", "A", "B", "X", "Y"};
			controllerState.menu = ((reportedState->buttons1&(1 << 2)) != 0) ? 1 : 0;
			controllerState.view = ((reportedState->buttons1&(1 << 3)) != 0) ? 1 : 0;
			controllerState.aButton = ((reportedState->buttons1&(1 << 4)) != 0) ? 1 : 0;
			controllerState.bButton = ((reportedState->buttons1&(1 << 5)) != 0) ? 1 : 0;
			controllerState.xButton = ((reportedState->buttons1&(1 << 6)) != 0) ? 1 : 0;
			controllerState.yButton = ((reportedState->buttons1&(1 << 7)) != 0) ? 1 : 0;

			// char *buttons2_index[] = {"Up", "Down", "Left", "Right", "Left Shoulder", "Right Shoulder", "Left Stick (Pressed)", "Right Stick (Pressed)"};
			controllerState.up = ((reportedState->buttons2&(1 << 0)) != 0) ? 1 : 0;
			controllerState.down = ((reportedState->buttons2&(1 << 1)) != 0) ? 1 : 0;
			controllerState.left = ((reportedState->buttons2&(1 << 2)) != 0) ? 1 : 0;
			controllerState.right = ((reportedState->buttons2&(1 << 3)) != 0) ? 1 : 0;
			controllerState.leftShoulder = ((reportedState->buttons2&(1 << 4)) != 0) ? 1 : 0;
			controllerState.rightShoulder = ((reportedState->buttons2&(1 << 5)) != 0) ? 1 : 0;
			controllerState.leftThumb = ((reportedState->buttons2&(1 << 6)) != 0) ? 1 : 0;
			controllerState.rightThumb = ((reportedState->buttons2&(1 << 7)) != 0) ? 1 : 0;

			// Triggers are 0 - 1023 (need to be 0 - 255)
			controllerState.leftTrigger = reportedState->leftTrigger > 0 ? iround((reportedState->leftTrigger / (float)1023) * 255) : 0;
			controllerState.rightTrigger = reportedState->rightTrigger > 0 ? iround((reportedState->rightTrigger / (float)1023) * 255) : 0;

			// Axes are -32767 - 32767 (as expected)
			controllerState.thumbLX = reportedState->thumbLX;
			controllerState.thumbLY = reportedState->thumbLY;
			controllerState.thumbRX = reportedState->thumbRX;
			controllerState.thumbRY = reportedState->thumbRY;

			break;
	}

	return true;
}

void vibrate(int leftTriggerVal, int rightTriggerVal, int leftVal, int rightVal)
{
	// Motors are 0 - 255
	unsigned char data[] = {9, 0, 0, 9, 0, 15, leftTriggerVal, rightTriggerVal, leftVal, rightVal, 255, 0, 0};
	usb_interrupt_write(handle, endpointOut, (char*)data, sizeof(data), timeout);
}

bool connectController(bool enable)
{
	if (enable && !isConnected)
	{

		usb_init();
		usb_find_busses();
		usb_find_devices();
    
		busses = usb_get_busses();
	
		for (bus = busses; bus; bus = bus->next)
		{
    		for (dev = bus->devices; dev; dev = dev->next)
			{
				if (dev->descriptor.idVendor == idVendor && dev->descriptor.idProduct == idProduct)
				{
					// Use the first controller we find (only).
					handle = usb_open(dev);
					
					if(usb_set_configuration(handle, configuration) < 0)
					{
						return false;
					}
					
					if(usb_claim_interface(handle, interface) < 0)
					{
						return false;
					}

					// Initialise
					char data[] = {0x05, 0x20};
					if(usb_interrupt_write(handle, endpointOut, (char*)data, sizeof(data), timeout) < 0)
					{
						return false;
					}

					isConnected = true;
					return true;
				}
    		}
		}
	}
	else if (!enable && isConnected)
	{
		usb_release_interface(handle, interface);
		usb_close(handle);

		isConnected = false;
	}

	return true;
}

DWORD WINAPI XInputGetState
(
    __in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
    __out XINPUT_STATE* pState								// Receives the current state
)
{
	if(!isConnected)
	{
		connectController(true);
	}
	
	if(isConnected && dwUserIndex == 0)
	{
		updateState();
		
		XINPUT_GAMEPAD gamepadState;
		gamepadState.bLeftTrigger = controllerState.leftTrigger;
		gamepadState.bRightTrigger = controllerState.rightTrigger;
		gamepadState.sThumbLX = controllerState.thumbLX;
		gamepadState.sThumbLY = controllerState.thumbLY;
		gamepadState.sThumbRX = controllerState.thumbRX;
		gamepadState.sThumbRY = controllerState.thumbRY;

		gamepadState.wButtons = 0;

		if (controllerState.view) gamepadState.wButtons |= XINPUT_GAMEPAD_BACK;
		if (controllerState.leftThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_THUMB;
		if (controllerState.rightThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_THUMB;
		if (controllerState.menu)			gamepadState.wButtons |= XINPUT_GAMEPAD_START;

		if (controllerState.up) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_UP;
		if (controllerState.right) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_RIGHT;
		if (controllerState.down) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_DOWN;
		if (controllerState.left) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_LEFT;

		if (controllerState.leftShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_SHOULDER;
		if (controllerState.rightShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_SHOULDER;

		if (controllerState.yButton) gamepadState.wButtons |= XINPUT_GAMEPAD_Y;
		if (controllerState.bButton) gamepadState.wButtons |= XINPUT_GAMEPAD_B;
		if (controllerState.aButton) gamepadState.wButtons |= XINPUT_GAMEPAD_A;
		if (controllerState.xButton) gamepadState.wButtons |= XINPUT_GAMEPAD_X;

		//if (controllerState.guideButton) gamepadState.wButtons |= XINPUT_GAMEPAD_GUIDE;

		pState->Gamepad = gamepadState;

		return ERROR_DEVICE_NOT_CONNECTED;
	}
	else
	{
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD WINAPI XInputSetState
(
    __in DWORD             dwUserIndex,						// Index of the gamer associated with the device
    __in XINPUT_VIBRATION* pVibration						// The vibration information to send to the controller
)
{
	if(!isConnected)
	{
		connectController(true);
	}

	if(isConnected && dwUserIndex == 0)
	{
		// We're receiving as XInput [0 ~ 65535], need to be [0 ~ 255] !!
		int leftVal = iround(((float)pVibration->wLeftMotorSpeed / 65535) * 255);
		int rightVal = iround(((float)pVibration->wRightMotorSpeed / 65535) * 255);
					
		vibrate(0, 0, leftVal, rightVal);

		return ERROR_SUCCESS;
	}
	else
	{
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
	if(!isConnected)
	{
		connectController(true);
	}

	if(isConnected && dwUserIndex == 0)
	{
		pCapabilities->Flags   = XINPUT_CAPS_VOICE_SUPPORTED;
		pCapabilities->Type    = XINPUT_DEVTYPE_GAMEPAD;
		pCapabilities->SubType = XINPUT_DEVSUBTYPE_GAMEPAD;

		pCapabilities->Gamepad.wButtons    = 0xF3FF;

		pCapabilities->Gamepad.bLeftTrigger  =
		pCapabilities->Gamepad.bRightTrigger = 0xFF;

		pCapabilities->Gamepad.sThumbLX =
		pCapabilities->Gamepad.sThumbLY =
		pCapabilities->Gamepad.sThumbRX =
		pCapabilities->Gamepad.sThumbRY = (SHORT) 0xFFC0;

		pCapabilities->Vibration.wLeftMotorSpeed  =
		pCapabilities->Vibration.wRightMotorSpeed = 0xFF;

		return ERROR_SUCCESS;
	}
	else
	{
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

void WINAPI XInputEnable
(
    __in bool enable										// [in] Indicates whether xinput is enabled or disabled. 
)
{
	if(!isConnected)
	{
		connectController(true);
	}
	
	if (isConnected && !enable)
	{
		XINPUT_VIBRATION Vibration = { 0, 0 };

		XInputSetState(0, &Vibration);
	}
}

DWORD WINAPI XInputGetDSoundAudioDeviceGuids
(
    __in  DWORD dwUserIndex,								// Index of the gamer associated with the device
    __out GUID* pDSoundRenderGuid,							// DSound device ID for render
    __out GUID* pDSoundCaptureGuid							// DSound device ID for capture
)
{
	if(!isConnected)
	{
		connectController(true);
	}

	if(isConnected && dwUserIndex == 0)
	{
		pDSoundRenderGuid = NULL;
		pDSoundCaptureGuid = NULL;

		return ERROR_SUCCESS;
	}
	else
	{
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
	if(!isConnected)
	{
		connectController(true);
	}

	if(isConnected && dwUserIndex == 0)
	{
		pBatteryInformation->BatteryType  = BATTERY_TYPE_WIRED;
		pBatteryInformation->BatteryLevel = BATTERY_LEVEL_FULL;

		return ERROR_SUCCESS;
	}
	else
	{
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
	if(!isConnected)
	{
		connectController(true);
	}

	if(isConnected && dwUserIndex == 0)
	{
		// TODO: pKeystroke
		return ERROR_EMPTY; // or ERROR_SUCCESS
	}
	else
	{
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

// Undocumented

DWORD WINAPI XInputGetStateEx(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	if(!isConnected)
	{
		connectController(true);
	}
	
	if(isConnected && dwUserIndex == 0)
	{
		updateState();
		
		XINPUT_GAMEPAD gamepadState;
		gamepadState.bLeftTrigger = controllerState.leftTrigger;
		gamepadState.bRightTrigger = controllerState.rightTrigger;
		gamepadState.sThumbLX = controllerState.thumbLX;
		gamepadState.sThumbLY = controllerState.thumbLY;
		gamepadState.sThumbRX = controllerState.thumbRX;
		gamepadState.sThumbRY = controllerState.thumbRY;

		gamepadState.wButtons = 0;

		if (controllerState.view) gamepadState.wButtons |= XINPUT_GAMEPAD_BACK;
		if (controllerState.leftThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_THUMB;
		if (controllerState.rightThumb) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_THUMB;
		if (controllerState.menu)			gamepadState.wButtons |= XINPUT_GAMEPAD_START;

		if (controllerState.up) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_UP;
		if (controllerState.right) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_RIGHT;
		if (controllerState.down) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_DOWN;
		if (controllerState.left) gamepadState.wButtons |= XINPUT_GAMEPAD_DPAD_LEFT;

		if (controllerState.leftShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_LEFT_SHOULDER;
		if (controllerState.rightShoulder) gamepadState.wButtons |= XINPUT_GAMEPAD_RIGHT_SHOULDER;

		if (controllerState.yButton) gamepadState.wButtons |= XINPUT_GAMEPAD_Y;
		if (controllerState.bButton) gamepadState.wButtons |= XINPUT_GAMEPAD_B;
		if (controllerState.aButton) gamepadState.wButtons |= XINPUT_GAMEPAD_A;
		if (controllerState.xButton) gamepadState.wButtons |= XINPUT_GAMEPAD_X;

		if (controllerState.guideButton) gamepadState.wButtons |= XINPUT_GAMEPAD_GUIDE;

		pState->Gamepad = gamepadState;

		return ERROR_SUCCESS;
	}
	else
	{
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}

DWORD WINAPI XInputSetStateEx
(
    __in DWORD             dwUserIndex,						// Index of the gamer associated with the device
    __in XINPUT_VIBRATION_EX* pVibration					// The vibration information to send to the controller
)
{
	if(!isConnected)
	{
		connectController(true);
	}

	if(isConnected && dwUserIndex == 0)
	{
		// We're receiving as XInput [0 ~ 65535], need to be [0 ~ 255] !!
		int leftTriggerVal = iround(((float)pVibration->wLeftTriggerMotorSpeed / 65535) * 255);
		int rightTriggerVal = iround(((float)pVibration->wRightTriggerMotorSpeed / 65535) * 255);
		int leftVal = iround(((float)pVibration->wLeftMotorSpeed / 65535) * 255);
		int rightVal = iround(((float)pVibration->wRightMotorSpeed / 65535) * 255);
			
		vibrate(leftTriggerVal, rightTriggerVal, leftVal, rightVal);

		return ERROR_SUCCESS;
	}
	else
	{
		return ERROR_DEVICE_NOT_CONNECTED;
	}
}