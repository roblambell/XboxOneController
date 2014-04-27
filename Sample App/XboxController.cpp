// Copyright 2014 Rob Lambell.  All rights reserved.
// Copyright 2013 Kyle Lemons.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#include "lusb0_usb.h"

#include <conio.h>
#include <iostream>
#include <stdint.h>
#include <stdio.h>

// Deadzones for our sanity during testing - remember to remove them!
#define XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE  7849
#define XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE 8689
#define XINPUT_GAMEPAD_TRIGGER_THRESHOLD    30

struct XboxOneControllerHandler
{
	struct usb_dev_handle *handle;
	XboxOneControllerHandler() : isConnected(false) {} bool isConnected;
};

struct usb_bus *busses;
struct usb_bus *bus;
struct usb_device *dev;
XboxOneControllerHandler *controllerHandler[4] = { NULL, NULL, NULL, NULL };

static unsigned short idVendor = 0x045E;
static unsigned short idProduct = 0x02D1;

int configuration = 1;
int interface = 0;
int endpointIn = 0x81;
int endpointOut = 0x01;
int timeout = 5000; // milliseconds

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

void vibrate(int leftTriggerVal, int rightTriggerVal, int leftVal, int rightVal, int dwUserIndex)
{
	// Motors are 0 - 255
	unsigned char data[] = { 9, 0, 0, 9, 0, 15, leftTriggerVal, rightTriggerVal, leftVal, rightVal, 255, 0, 0 };
	if (usb_interrupt_write(controllerHandler[dwUserIndex]->handle, endpointOut, (char*)data, sizeof(data), timeout) < 0)
	{
		std::cout << "usb_interrupt_write (" << usb_strerror() << ")." << std::endl;
	}
}

void decode(char *data)
{
	XboxOneControllerState *controllerState = (XboxOneControllerState*)data;

	char *buttons1_index[] = { "Sync", "Unknown", "Menu", "View", "A", "B", "X", "Y" };
	for (int i = 0; i < 8; ++i)
	{
		if ((controllerState->buttons1&(1 << i)) != 0)
		{
			std::cout << buttons1_index[i] << std::endl;
		}
	}

	char *buttons2_index[] = { "Up", "Down", "Left", "Right", "Left Shoulder", "Right Shoulder", "Left Stick (Pressed)", "Right Stick (Pressed)" };
	for (int i = 0; i < 8; ++i)
	{
		if ((controllerState->buttons2&(1 << i)) != 0)
		{
			std::cout << buttons2_index[i] << std::endl;
		}
	}

	// Triggers are 0 - 1023
	if (controllerState->leftTrigger > XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
	{
		std::cout << "Left Trigger: " << controllerState->leftTrigger << std::endl;
	}

	if (controllerState->rightTrigger > XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
	{
		std::cout << "Right Trigger: " << controllerState->rightTrigger << std::endl;
	}

	// Axes are -32767 - 32767
	if (abs(controllerState->thumbLX) > XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE || abs(controllerState->thumbLY) > XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE)
	{
		std::cout << "Left Stick: " << controllerState->thumbLX << ", " << controllerState->thumbLY << std::endl;
	}

	if (abs(controllerState->thumbRX) > XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE || abs(controllerState->thumbRY) > XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE)
	{
		std::cout << "Right Stick: " << controllerState->thumbRX << ", " << controllerState->thumbRY << std::endl;
	}
}

int main(int argc, char **argv)
{
	int xboxControllerCounter = 0;
	usb_init();
	usb_find_busses();
	usb_find_devices();

	busses = usb_get_busses();

	for (bus = busses; bus; bus = bus->next)
	{
		for (dev = bus->devices; dev; dev = dev->next)
		{
			if (dev->descriptor.idVendor == idVendor && dev->descriptor.idProduct == idProduct && xboxControllerCounter < 4)
			{
				std::cout << "Found Xbox One Controller\n";
				controllerHandler[xboxControllerCounter] = (XboxOneControllerHandler*)malloc(sizeof(XboxOneControllerHandler));
				controllerHandler[xboxControllerCounter]->handle = usb_open(dev);

				if (usb_set_configuration(controllerHandler[xboxControllerCounter]->handle, configuration) < 0)
				{
					std::cout << "usb_set_configuration (" << usb_strerror() << ")." << std::endl;
					exit(EXIT_FAILURE);
				}

				if (usb_claim_interface(controllerHandler[xboxControllerCounter]->handle, interface) < 0)
				{
					std::cout << "usb_claim_interface (" << usb_strerror() << ")." << std::endl;
					exit(EXIT_FAILURE);
				}

				// Initialise
				char data[] = { 0x05, 0x20 };
				if (usb_interrupt_write(controllerHandler[xboxControllerCounter]->handle, endpointOut, (char*)data, sizeof(data), timeout) < 0)
				{
					std::cout << "usb_interrupt_write (" << usb_strerror() << ").\n";
				}

				std::cout << "Initialised Xbox One Controller number: " << xboxControllerCounter << std::endl << std::endl;

				std::cout << "Shake it ";
				// Motors are 0 - 255
				vibrate(50, 50, 50, 50, xboxControllerCounter);
				Sleep(500);
				vibrate(0, 0, 0, 0, xboxControllerCounter);

				std::cout << "and Listen - Press any key to exit." << std::endl << std::endl;

				bool receive_data = true;
				while (receive_data)
				{
					uint8_t raw_data[64];

					int ret = usb_interrupt_read(controllerHandler[xboxControllerCounter]->handle, endpointIn, (char*)raw_data, sizeof(raw_data), timeout);
					if (ret < 0)
					{
						std::cout << "usb_interrupt_read (" << usb_strerror() << ")." << std::endl;
						break;
					}

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
							std::cout << "Guide" << std::endl;
						}
						break;
					case 0x20:
						decode(data);
						break;
					}

					if (_kbhit())
					{
						while (_kbhit()) _getch();
						receive_data = false;
					}
				}

				usb_release_interface(controllerHandler[xboxControllerCounter]->handle, interface);
				usb_close(controllerHandler[xboxControllerCounter]->handle);
			}
		}
	}

	return 0;
}

