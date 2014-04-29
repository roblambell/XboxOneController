using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace XboxOneController
{
    [StructLayout(LayoutKind.Sequential)]
    public struct State
    {
        public int PacketNumber;
        public SharpDX.XInput.Gamepad Gamepad;
    }

    [Flags]
    public enum GamepadKeyCode : short
    {
        A = 0x5800,
        B = 0x5801,
        Back = 0x5815,
        DPadDown = 0x5811,
        DPadLeft = 0x5812,
        DPadRight = 0x5813,
        DPadUp = 0x5810,
        LeftShoulder = 0x5805,
        LeftThumbDown = 0x5821,
        LeftThumbDownright = 0x5826,
        LeftThumbLeft = 0x5823,
        LeftThumbPress = 0x5816,
        LeftThumbRight = 0x5822,
        LeftThumbUp = 0x5820,
        LeftThumbUpright = 0x5825,
        LeftTrigger = 0x5806,
        None = 0,
        RightShoulder = 0x5804,
        RightThumbDown = 0x5831,
        RightThumbDownleft = 0x5837,
        RightThumbDownLeft = 0x5827,
        RightThumbDownRight = 0x5836,
        RightThumbLeft = 0x5833,
        RightThumbPress = 0x5817,
        RightThumbRight = 0x5832,
        RightThumbUp = 0x5830,
        RightThumbUpleft = 0x5834,
        RightThumbUpLeft = 0x5824,
        RightThumbUpRight = 0x5835,
        RightTrigger = 0x5807,
        Start = 0x5814,
        X = 0x5802,
        Y = 0x5803
    }

    [Flags]
    public enum KeyStrokeFlags : short
    {
        KeyDown = 1,
        KeyUp = 2,
        None = 0,
        Repeat = 4
    }


    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Keystroke
    {
        public GamepadKeyCode VirtualKey;
        public char Unicode;
        public KeyStrokeFlags Flags;
        public char UserIndex;
        public byte HidCode;
    }


}
