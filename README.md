Xbox One Controller
===================

Simple libusb-win32 XInput Wrapper for the Xbox One Controller.

Thanks to Kyle Lemons for his earlier work https://github.com/kylelemons/xbox

To Swizzy and Lucas Assis for vibration https://www.youtube.com/watch?v=6bCEK71UuW4

Instructions
------------
1\. [Download the latest release](https://github.com/badgio/XboxOneController/releases)  
2\. Install the driver (or get it from http://sourceforge.net/apps/trac/libusb-win32/wiki)
3\. Place xinput1_3.dll in to same directory as the game executable

May need to rename to one of the following:
- xinput1_4.dll (Windows 8 / metro apps only)
- xinput1_3.dll
- xinput1_2.dll
- xinput1_1.dll
- xinput9_1_0.dll

This may be better implemented in the future using the same method as Scarlet.Crush's XInput Wrapper http://forums.pcsx2.net/Thread-XInput-Wrapper-for-DS3-and-Play-com-USB-Dual-DS2-Controller

The xinput1_3.dll wrapper does however allow us the freedom to add functionality, additional is:-
- XInputSetStateEx()

```C
typedef struct _XINPUT_VIBRATION_EX
{
    WORD                                wLeftMotorSpeed;
    WORD                                wRightMotorSpeed;
    WORD                                wLeftTriggerMotorSpeed;
    WORD                                wRightTriggerMotorSpeed;
} XINPUT_VIBRATION_EX, *PXINPUT_VIBRATION_EX;
