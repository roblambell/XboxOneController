using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace XboxOneController
{
    public enum PlayerIndex
    {
        One,
        Two,
        Three,
        Four
    }

    [Flags]
    internal enum ButtonValues : ushort
    {
        A = 0x1000,
        B = 0x2000,
        Back = 0x20,
        BigButton = 0x800,
        Down = 2,
        Left = 4,
        LeftShoulder = 0x100,
        LeftThumb = 0x40,
        Right = 8,
        RightShoulder = 0x200,
        RightThumb = 0x80,
        Start = 0x10,
        Up = 1,
        X = 0x4000,
        Y = 0x8000
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_VIBRATION
    {
        public short LeftMotorSpeed;
        public short RightMotorSpeed;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_CAPABILITIES
    {
        public byte Type;
        public byte SubType;
        public ushort Flags;
        public XINPUT_GAMEPAD GamePad;
        public XINPUT_VIBRATION Vibration;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_GAMEPAD
    {
        public ButtonValues Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX;
        public short ThumbLY;
        public short ThumbRX;
        public short ThumbRY;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_STATE
    {
        public int PacketNumber;
        public XINPUT_GAMEPAD GamePad;
    }
}
