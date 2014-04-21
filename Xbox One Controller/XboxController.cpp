/**
 * Xbox One Controller
 *
 * @author Rob Lambell <rob@lambell.info>
 * @license MIT
 */

#include "lusb0_usb.h"

#include <conio.h>
#include <iostream>
#include <stdint.h>
#include <stdio.h>

// Deadzones for our sanity during testing - remember to remove them!
#define XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE  7849
#define XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE 8689
#define XINPUT_GAMEPAD_TRIGGER_THRESHOLD    30

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

struct XboxOneControllerState
{
	char eventCount;
	char unknown;
	char buttons1;
	char buttons2;
	short leftTrigger;
	short rightTrigger;
	short thumbLX;
	short thumbLY;
	short thumbRX;
	short thumbRY;
};

void vibrate(int leftTriggerVal, int rightTriggerVal, int leftVal, int rightVal)
{
	unsigned char data[] = {9, 0, 0, 9, 0, 15, leftTriggerVal, rightTriggerVal, leftVal, rightVal, 255, 0, 0};
	if(usb_interrupt_write(handle, endpointOut, (char*)data, sizeof(data), timeout) < 0)
	{
		std::cout << "usb_interrupt_write (" << usb_strerror() << ").\n";
	}
}

void decode(char *data)
{	
	XboxOneControllerState *controllerState = (XboxOneControllerState*)data;

	char *buttons1_index[] = {"Sync", "Unknown", "Menu", "View", "A", "B", "X", "Y"};
	for(int i = 0; i < 8; ++i)
	{
		if((controllerState->buttons1&(1 << i)) != 0)
		{
			std::cout << buttons1_index[i] << "\n";
		}
	}

	char *buttons2_index[] = {"Up", "Down", "Left", "Right", "Left Shoulder", "Right Shoulder", "Left Stick (Pressed)", "Right Stick (Pressed)"};
	for(int i = 0; i < 8; ++i)
	{
		if((controllerState->buttons2&(1 << i)) != 0)
		{
			std::cout << buttons2_index[i] << "\n";
		}
	}

	// Triggers are 0 - 1023
	if(controllerState->leftTrigger > XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
	{
		std::cout << "Left Trigger: " << controllerState->leftTrigger << "\n";
	}

	if(controllerState->rightTrigger > XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
	{
		std::cout << "Right Trigger: " << controllerState->rightTrigger << "\n";
	}

	// Axes are -32767 - 32767
	if(abs(controllerState->thumbLX) > XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE || abs(controllerState->thumbLY) > XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE)
	{
		std::cout << "Left Stick: " << controllerState->thumbLX << ", " << controllerState->thumbLY << "\n";
	}

	if(abs(controllerState->thumbRX) > XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE || abs(controllerState->thumbRY) > XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE)
	{
		std::cout << "Right Stick: " << controllerState->thumbRX << ", " << controllerState->thumbRY << "\n";
	}
}

int main(int argc, char **argv)
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

				std::cout << "Found Xbox One Controller\n";

				handle = usb_open(dev);

				if(usb_set_configuration(handle, configuration) < 0)
				{
					std::cout << "usb_set_configuration (" << usb_strerror() << ").\n";
					exit(EXIT_FAILURE);
				}

				if(usb_claim_interface(handle, interface) < 0)
				{
					std::cout << "usb_claim_interface (" << usb_strerror() << ").\n";
					exit(EXIT_FAILURE);
				}

				// Initialise
				char data[] = {0x05, 0x20};
				if(usb_interrupt_write(handle, endpointOut, (char*)data, sizeof(data), timeout) < 0)
				{
					std::cout << "usb_interrupt_write (" << usb_strerror() << ").\n";
				}

				std::cout << "Initialised Xbox One Controller\n\n";

				std::cout << "Shake it ";
				// Motors are 0 - 255
				vibrate(50, 50, 50, 50);
				Sleep(500);
				vibrate(0, 0, 0, 0);

				std::cout << "and Listen - Press any key to exit.\n\n";
				
				bool receive_data = true;
				while(receive_data)
				{
					uint8_t raw_data[64];

					int ret = usb_interrupt_read(handle, endpointIn, (char*)raw_data, sizeof(raw_data), timeout);
					if (ret < 0)
					{
						std::cout << "usb_interrupt_read (" << usb_strerror() << ").\n";
						break;
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

					switch(tag)
					{
						case 0x07:
							if((data[2]&0x01) != 0){
								std::cout << "Guide\n";
							}
							break;
						case 0x20:
							decode(data);
							break;
					}
					
					if(_kbhit())
					{
						receive_data = false;
					}
				}

				usb_release_interface(handle, interface);
				usb_close(handle);
				break;
			}
    	}
    }

    return 0;
}

