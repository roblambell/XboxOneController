using LibUsbDotNet;
using LibUsbDotNet.Main;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XboxOnePadReader
{
    public class ThreadController
    {
        private const byte configurations = 0x01;
        private const int interfaces = 0x00;
        private const int timeout = 2000;

        private volatile bool _shouldStop;

        private UsbEndpointReader reader;
        private UsbEndpointWriter writer;
        private byte[] lastState = new byte[64];
        private UsbDevice _myDevice;

        public volatile int tickCount = 0;
        public volatile XboxOneState state = new XboxOneState();

        public ThreadController(UsbDevice myDevice)
        {
            _myDevice = myDevice;
            IUsbDevice wholeUsbDevice = _myDevice as IUsbDevice;

            wholeUsbDevice.SetConfiguration(configurations);
            wholeUsbDevice.ClaimInterface(interfaces);
            reader = _myDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            writer = _myDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

            byte[] data = { 0x05, 0x20 };
            int dataWritten = 0;
            ErrorCode ec = writer.Write(data, timeout, out dataWritten);
            if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);
        }

        public static byte iround(double num)
        {
            return (num > 0.0) ? (byte)Math.Floor(num + 0.5) : (byte)Math.Ceiling(num - 0.5);
        }

        public void UpdateState()
        {
            while (!_shouldStop && _myDevice.IsOpen)
            {
                byte[] rawData = new byte[64];
                int transferLength;
                reader.Read(rawData, 1000, out transferLength);

                if (!Enumerable.SequenceEqual(lastState, rawData))
                    ++tickCount;

                byte tag = rawData[0];
                byte code = rawData[1];
                byte[] data = new byte[62];

                Array.Copy(rawData, 2, data, 0, 62);

                switch (tag)
                {
                    case 0x07:
                        if ((data[2] & 0x01) != 0)
                        {
                            state.guideButton = 1;
                        }
                        break;
                    case 0x20:
                        state.guideButton = 0;

                        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        XboxOneControllerState reportedState = (XboxOneControllerState)Marshal.PtrToStructure(
                            handle.AddrOfPinnedObject(), typeof(XboxOneControllerState));
                        handle.Free();

                        // char *buttons1_index[] = {"Sync", "Unknown", "Menu", "View", "A", "B", "X", "Y"};
                        state.menu = ((reportedState.buttons1 & (1 << 2)) != 0) ? (byte)1 : (byte)0;
                        state.view = ((reportedState.buttons1 & (1 << 3)) != 0) ? (byte)1 : (byte)0;
                        state.aButton = ((reportedState.buttons1 & (1 << 4)) != 0) ? (byte)1 : (byte)0;
                        state.bButton = ((reportedState.buttons1 & (1 << 5)) != 0) ? (byte)1 : (byte)0;
                        state.xButton = ((reportedState.buttons1 & (1 << 6)) != 0) ? (byte)1 : (byte)0;
                        state.yButton = ((reportedState.buttons1 & (1 << 7)) != 0) ? (byte)1 : (byte)0;

                        // char *buttons2_index[] = {"Up", "Down", "Left", "Right", "Left Shoulder", "Right Shoulder", "Left Stick (Pressed)", "Right Stick (Pressed)"};
                        state.up = ((reportedState.buttons2 & (1 << 0)) != 0) ? (byte)1 : (byte)0;
                        state.down = ((reportedState.buttons2 & (1 << 1)) != 0) ? (byte)1 : (byte)0;
                        state.left = ((reportedState.buttons2 & (1 << 2)) != 0) ? (byte)1 : (byte)0;
                        state.right = ((reportedState.buttons2 & (1 << 3)) != 0) ? (byte)1 : (byte)0;
                        state.leftShoulder = ((reportedState.buttons2 & (1 << 4)) != 0) ? (byte)1 : (byte)0;
                        state.rightShoulder = ((reportedState.buttons2 & (1 << 5)) != 0) ? (byte)1 : (byte)0;
                        state.leftThumb = ((reportedState.buttons2 & (1 << 6)) != 0) ? (byte)1 : (byte)0;
                        state.rightThumb = ((reportedState.buttons2 & (1 << 7)) != 0) ? (byte)1 : (byte)0;

                        // Triggers are 0 - 1023 (need to be 0 - 255)
                        state.leftTrigger = reportedState.leftTrigger > 0 ? iround((reportedState.leftTrigger / (float)1023) * 255) : (byte)0;
                        state.rightTrigger = reportedState.rightTrigger > 0 ? iround((reportedState.rightTrigger / (float)1023) * 255) : (byte)0;

                        // Axes are -32767 - 32767 (as expected)
                        state.thumbLX = reportedState.thumbLX;
                        state.thumbLY = reportedState.thumbLY;
                        state.thumbRX = reportedState.thumbRX;
                        state.thumbRY = reportedState.thumbRY;
                        break;
                }

            }

            if (_myDevice.IsOpen)
            {
                IUsbDevice wholeUsbDevice = _myDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    wholeUsbDevice.ReleaseInterface(0);
                }

                _myDevice.Close();
            }
            _myDevice = null;
            writer = null;
            reader = null;
        }

        public void Vibrate(int leftTriggerVal, int rightTriggerVal, int leftVal, int rightVal, int dwUserIndex)
        {
            byte[] data = { 9, 0, 0, 9, 0, 15, (byte)leftTriggerVal, (byte)rightTriggerVal, (byte)leftVal, (byte)rightVal, 255, 0, 0 };

            if (writer != null)
            {
                lock (writer)
                {
                    int dataWritten = 0;
                    writer.Write(data, 500, out dataWritten);
                }
            }
        }

        public void StopThread()
        {
            _shouldStop = true;
        }

        public class XboxOneState
        {
            public XboxOneState() {}
            public byte guideButton;
            public byte view;
            public byte menu;
            public byte rightShoulder;
            public byte rightTrigger;
            public byte rightThumb;
            public byte leftShoulder;
            public byte leftTrigger;
            public byte leftThumb;
            public short thumbRX;
            public short thumbRY;
            public short thumbLX;
            public short thumbLY;
            public byte up;
            public byte down;
            public byte left;
            public byte right;
            public byte yButton;
            public byte bButton;
            public byte aButton;
            public byte xButton;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct XboxOneControllerState
        {
            public byte eventCount;
            public byte unknown;
            public byte buttons1;
            public byte buttons2;
            public short leftTrigger;  // Triggers are 0 - 1023
            public short rightTrigger;
            public short thumbLX;      // Axes are -32767 - 32767
            public short thumbLY;
            public short thumbRX;
            public short thumbRY;
        };
    }
}
