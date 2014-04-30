using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using EasyHook;
using System.Reflection;
using SharpDX;
using SharpDX.XInput;
using XboxOnePadReader;


namespace XboxOneController
{
    public class XboxOneControllerInjection : EasyHook.IEntryPoint
    {
        private const uint ERROR_DEVICE_NOT_CONNECTED = 0x1167;
        private const uint ERROR_SUCCESS = 0x00;
        private const uint ERROR_BAD_ARGUMENTS = 0x160;
        private const uint ERROR_EMPTY = 0x4306;

        public RemoInterface Interface = null;
        public List<LocalHook> Hooks = null;
        Stack<String> Queue = new Stack<string>();

        public XboxOneControllerInjection(
            RemoteHooking.IContext InContext,
            String InChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<RemoInterface>(InChannelName);
            Hooks = new List<LocalHook>();
            Interface.Ping(RemoteHooking.GetCurrentProcessId());
        }

        public void Run(
            RemoteHooking.IContext InContext,
            String InArg1)
        {
            bool succeed = false;

            try
            {
                //xinput1_3.dll
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputEnable"),
                    new DXInputEnable(XInputEnable_Hooked),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetBatteryInformation"),
                    new DXInputGetBatteryInformation(XInputGetBatteryInformation_Hooked),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetCapabilities"),
                    new DXInputGetCapabilities(XInputGetCapabilities_Hooked),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetDSoundAudioDeviceGuids"),
                    new DXInputGetDSoundAudioDeviceGuids(XInputGetDSoundAudioDeviceGuids_Hooked),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetKeystroke"),
                    new DXInputGetKeystroke(XInputGetKeystroke_Hooked),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetState"),
                    new DXInputGetState(XInputGetState_Hooked),
                    this));
               /* Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetStateEx"),
                    new DXInputGetStateEx(XInputGetStateEx_Hooked),
                    this));*/
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputSetState"),
                    new DXInputSetState(XInputSetState_Hooked),
                    this));
               /* Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputSetStateEx"),
                    new DXInputSetStateEx(XInputSetStateEx_Hooked),
                    this));*/
                /*
                 * Don't forget that all hooks will start deaktivated...
                 * The following ensures that all threads are intercepted:
                 */
                foreach(LocalHook hook in Hooks)
                    hook.ThreadACL.SetExclusiveACL(new Int32[1]);
                succeed = true;
            }
            catch (Exception e)
            {
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), "xinput1_3.dll", e);
                Hooks.Clear();
            }

            try
            {
                if (!succeed)
                {
                    //xinput1_1.dll
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_1.dll", "XInputEnable"),
                        new DXInputEnable(XInputEnable_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_1.dll", "XInputGetCapabilities"),
                        new DXInputGetCapabilities(XInputGetCapabilities_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_1.dll", "XInputGetDSoundAudioDeviceGuids"),
                        new DXInputGetDSoundAudioDeviceGuids(XInputGetDSoundAudioDeviceGuids_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_1.dll", "XInputGetState"),
                        new DXInputGetState(XInputGetState_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_1.dll", "XInputSetState"),
                        new DXInputSetState(XInputSetState_Hooked),
                        this));

                    foreach (LocalHook hook in Hooks)
                        hook.ThreadACL.SetExclusiveACL(new Int32[1]);
                    succeed = true;
                }
            }
            catch (Exception e)
            {
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), "xinput1_1.dll", e);
                Hooks.Clear();
            }

            try
            {
                if (!succeed)
                {
                    //xinput1_2.dll
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_2.dll", "XInputEnable"),
                        new DXInputEnable(XInputEnable_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_2.dll", "XInputGetCapabilities"),
                        new DXInputGetCapabilities(XInputGetCapabilities_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_2.dll", "XInputGetDSoundAudioDeviceGuids"),
                        new DXInputGetDSoundAudioDeviceGuids(XInputGetDSoundAudioDeviceGuids_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_2.dll", "XInputGetState"),
                        new DXInputGetState(XInputGetState_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_2.dll", "XInputSetState"),
                        new DXInputSetState(XInputSetState_Hooked),
                        this));

                    foreach (LocalHook hook in Hooks)
                        hook.ThreadACL.SetExclusiveACL(new Int32[1]);
                    succeed = true;
                }
            }
            catch (Exception e)
            {
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), "xinput1_2.dll", e);
                Hooks.Clear();
            }

            try
            {
                if (!succeed)
                {
                    //xinput1_4.dll
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputEnable"),
                        new DXInputEnable(XInputEnable_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputGetBatteryInformation"),
                        new DXInputGetBatteryInformation(XInputGetBatteryInformation_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputGetCapabilities"),
                        new DXInputGetCapabilities(XInputGetCapabilities_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputGetDSoundAudioDeviceGuids"),
                        new DXInputGetDSoundAudioDeviceGuids(XInputGetDSoundAudioDeviceGuids_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputGetKeystroke"),
                        new DXInputGetKeystroke(XInputGetKeystroke_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputGetState"),
                        new DXInputGetState(XInputGetState_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_4.dll", "XInputSetState"),
                        new DXInputSetState(XInputSetState_Hooked),
                        this));

                    foreach (LocalHook hook in Hooks)
                        hook.ThreadACL.SetExclusiveACL(new Int32[1]);
                    succeed = true;
                }
            }
            catch (Exception e)
            {
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), "xinput1_4.dll", e);
                Hooks.Clear();
            }


            try
            {
                if (!succeed)
                {
                    //xinput9_1_0.dll
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput9_1_0.dll", "XInputGetCapabilities"),
                        new DXInputGetCapabilities(XInputGetCapabilities_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput9_1_0.dll", "XInputGetDSoundAudioDeviceGuids"),
                        new DXInputGetDSoundAudioDeviceGuids(XInputGetDSoundAudioDeviceGuids_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput9_1_0.dll", "XInputGetState"),
                        new DXInputGetState(XInputGetState_Hooked),
                        this));
                    Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput9_1_0.dll", "XInputSetState"),
                        new DXInputSetState(XInputSetState_Hooked),
                        this));
                    foreach (LocalHook hook in Hooks)
                        hook.ThreadACL.SetExclusiveACL(new Int32[1]);
                    succeed = true;
                }
            }
            catch (Exception e)
            {
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), Assembly.GetExecutingAssembly().GetName().Name, e);
            }

            if (!succeed)
                return;

            // wait for host process termination...
            try
            {
                while (Interface.Ping(RemoteHooking.GetCurrentProcessId()))
                {
                    Thread.Sleep(500);

                    // transmit newly monitored file accesses...
                    lock (Queue)
                    {
                        if (Queue.Count > 0)
                        {
                            String[] Package = null;

                            Package = Queue.ToArray();

                            Queue.Clear();

                            Interface.OnFunctionsCalled(RemoteHooking.GetCurrentProcessId(), Package);
                        }
                    }
                }
            }
            catch
            {
                // NET Remoting will raise an exception if host is unreachable
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputSetState(int dwUserIndex, ref Vibration pVibration);
        delegate void DXInputSetStateAsync(int dwUserIndex, ref Vibration pVibration);
        static uint XInputSetState_Hooked(int dwUserIndex, ref Vibration pVibration)
        {
            try
            {
                ControllerReader myController = ControllerReader.Instance;

                if (myController.controllers.Count < dwUserIndex)
                    return ERROR_DEVICE_NOT_CONNECTED;

                int leftVal = ThreadController.iround(((float)pVibration.LeftMotorSpeed / 65535) * 255);
                int rightVal = ThreadController.iround(((float)pVibration.RightMotorSpeed / 65535) * 255);
                myController.controllers[dwUserIndex].Vibrate(0, 0, leftVal, rightVal, dwUserIndex);

                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputSetState dwUserIndex = " + dwUserIndex);
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_SUCCESS;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetState(int dwUserIndex, out State pState);
        delegate void DXInputGetStateAsync(int dwUserIndex, out State pState);
        static uint XInputGetState_Hooked(int dwUserIndex, out State pState)
        {
            pState = new State();
            try
            {
                ControllerReader myController = ControllerReader.Instance;

                if (myController.controllers.Count < dwUserIndex)
                    return ERROR_DEVICE_NOT_CONNECTED;

                pState.Gamepad.LeftTrigger = myController.controllers[dwUserIndex].state.leftTrigger;
                pState.Gamepad.RightTrigger = myController.controllers[dwUserIndex].state.rightTrigger;
                pState.Gamepad.LeftThumbX = myController.controllers[dwUserIndex].state.thumbLX;
                pState.Gamepad.LeftThumbY = myController.controllers[dwUserIndex].state.thumbLY;
                pState.Gamepad.RightThumbX = myController.controllers[dwUserIndex].state.thumbRX;
                pState.Gamepad.RightThumbY = myController.controllers[dwUserIndex].state.thumbRY;

                pState.Gamepad.Buttons = 0;

                if (myController.controllers[dwUserIndex].state.view != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.Back;
                if (myController.controllers[dwUserIndex].state.leftThumb != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.LeftThumb;
                if (myController.controllers[dwUserIndex].state.rightThumb != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.RightThumb;
                if (myController.controllers[dwUserIndex].state.menu != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.Start;

                if (myController.controllers[dwUserIndex].state.up != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.DPadUp;
                if (myController.controllers[dwUserIndex].state.right != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.DPadRight;
                if (myController.controllers[dwUserIndex].state.down != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.DPadDown;
                if (myController.controllers[dwUserIndex].state.left != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.DPadLeft;

                if (myController.controllers[dwUserIndex].state.leftShoulder != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.LeftShoulder;
                if (myController.controllers[dwUserIndex].state.rightShoulder != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.RightShoulder;

                if (myController.controllers[dwUserIndex].state.yButton != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.Y;
                if (myController.controllers[dwUserIndex].state.bButton != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.B;
                if (myController.controllers[dwUserIndex].state.aButton != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.A;
                if (myController.controllers[dwUserIndex].state.xButton != 0) pState.Gamepad.Buttons |= SharpDX.XInput.GamepadButtonFlags.X;

                pState.PacketNumber = myController.controllers[dwUserIndex].tickCount;

                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetState");
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_SUCCESS;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputSetStateEx(int playerIndex, ref Vibration pVibration);
        delegate void DXInputSetStateExAsync(int playerIndex, ref Vibration pVibration);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputSetStateEx")]
        static extern uint XInputSetStateEx(int playerIndex, ref Vibration pVibration);

        static uint XInputSetStateEx_Hooked(int playerIndex, ref Vibration pVibration)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;
                //TODO
                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputSetStateEx");
                }
            }
            catch
            {
            }
            return XInputSetStateEx(playerIndex, ref pVibration);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetStateEx(int playerIndex, out State pState);
        delegate void DXInputGetStateExAsync(int playerIndex, out State pState);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetStateEx")]
        static extern uint XInputGetStateEx(int playerIndex, out State pState);

        static uint XInputGetStateEx_Hooked(int playerIndex, out State pState)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;
                //TODO
                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetStateEx");
                }
            }
            catch
            {
            }
            return XInputGetStateEx(playerIndex, out pState);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetKeystroke(int dwUserIndex, int dwReserved, out Keystroke pKeystroke);
        delegate void DXInputGetKeystrokeAsync(int dwUserIndex, int dwReserved, out Keystroke pKeystroke);

        static uint XInputGetKeystroke_Hooked(int dwUserIndex, int dwReserved, out Keystroke pKeystroke)
        {
            pKeystroke = new Keystroke();
            try
            {
                ControllerReader myController = ControllerReader.Instance;
                if (myController.controllers.Count < dwUserIndex)
                    return ERROR_DEVICE_NOT_CONNECTED;

                //TODO

                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetKeystroke");
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_EMPTY;
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetBatteryInformation(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation);
        delegate void DXInputGetBatteryInformationAsync(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation);
        static uint XInputGetBatteryInformation_Hooked(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation)
        {
            pBatteryInformation = new BatteryInformation();
            try
            {
                ControllerReader myController = ControllerReader.Instance;

                if (myController.controllers.Count < dwUserIndex)
                    return ERROR_DEVICE_NOT_CONNECTED;

                pBatteryInformation.BatteryType = BatteryType.Wired;
                pBatteryInformation.BatteryLevel = BatteryLevel.Full;

                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetBatteryInformation");
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_SUCCESS;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetDSoundAudioDeviceGuids(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid);
        delegate void DXInputGetDSoundAudioDeviceGuidsAsync(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid);
        static uint XInputGetDSoundAudioDeviceGuids_Hooked(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid)
        {
            pDSoundRenderGuid = new Guid();
            pDSoundCaptureGuid = new Guid();

            try
            {
                ControllerReader myController = ControllerReader.Instance;

                if (myController.controllers.Count < dwUserIndex)
                    return ERROR_DEVICE_NOT_CONNECTED;
                //TODO
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetDSoundAudioDeviceGuids");
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_SUCCESS;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputEnable(bool enable);
        delegate void DXInputEnableAsync(bool enable);
        static uint XInputEnable_Hooked(bool enable)
        {
            try
            {
                Vibration resetVibration = new Vibration();

                for (int x = 0; x < 4; ++x)
                    XInputSetState_Hooked(x, ref resetVibration);

                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputEnable");
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_SUCCESS;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetCapabilities(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities);
        delegate void DXInputGetCapabilitiesAsync(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities);
        static uint XInputGetCapabilities_Hooked(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities)
        {
            pCapabilities = new Capabilities();
            try
            {
                if (dwFlags > DeviceQueryType.Gamepad)
                    return ERROR_BAD_ARGUMENTS;

                ControllerReader myController = ControllerReader.Instance;
                if (myController.controllers.Count < dwUserIndex)
                    return ERROR_DEVICE_NOT_CONNECTED;

                pCapabilities.Flags = CapabilityFlags.VoiceSupported;
                pCapabilities.Type = DeviceType.Gamepad;
                pCapabilities.SubType = DeviceSubType.Gamepad;

                pCapabilities.Gamepad.Buttons = GamepadButtonFlags.A | GamepadButtonFlags.B | GamepadButtonFlags.Back | GamepadButtonFlags.DPadDown
                    | GamepadButtonFlags.DPadLeft | GamepadButtonFlags.DPadRight | GamepadButtonFlags.DPadUp | GamepadButtonFlags.LeftShoulder | GamepadButtonFlags.LeftThumb
                    | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.RightThumb | GamepadButtonFlags.Start | GamepadButtonFlags.X | GamepadButtonFlags.Y;

                pCapabilities.Gamepad.LeftTrigger = 0xFF;
                pCapabilities.Gamepad.RightTrigger = 0xFF;

                pCapabilities.Gamepad.LeftThumbX = short.MaxValue;
                pCapabilities.Gamepad.LeftThumbY = short.MaxValue;
                pCapabilities.Gamepad.RightThumbX = short.MaxValue;
                pCapabilities.Gamepad.RightThumbY = short.MaxValue;

                pCapabilities.Vibration.LeftMotorSpeed = 0xFF;
                pCapabilities.Vibration.RightMotorSpeed = 0xFF;

                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetCapabilities");
                }
            }
            catch
            {
                return ERROR_DEVICE_NOT_CONNECTED;
            }
            return ERROR_SUCCESS;
        }
    }
}
