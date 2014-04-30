using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using EasyHook;
using System.Reflection;
using SharpDX;
using SharpDX.Win32;
using SharpDX.XInput;


namespace XboxOneController
{
    public class XboxOneControllerInjection : EasyHook.IEntryPoint
    {
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
        [DllImport("xinput1_3.dll", EntryPoint = "XInputSetState")]
        static extern uint XInputSetState(int dwUserIndex, ref Vibration pVibration);
        static uint XInputSetState_Hooked(int dwUserIndex, ref Vibration pVibration)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputSetState");
                }
            }
            catch
            {
            }
            return XInputSetState(dwUserIndex, ref pVibration);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetState(int playerIndex, out State pState);
        delegate void DXInputGetStateAsync(int playerIndex, out State pState);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")]
        static extern uint XInputGetState(int playerIndex, out State pState);

        static uint XInputGetState_Hooked(int playerIndex, out State pState)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetState");
                }
            }
            catch
            {
            }
            return XInputGetState(playerIndex, out pState);
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
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetKeystroke")]
        static extern uint XInputGetKeystroke(int dwUserIndex, int dwReserved, out Keystroke pKeystroke);

        static uint XInputGetKeystroke_Hooked(int dwUserIndex, int dwReserved, out Keystroke pKeystroke)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetKeystrokeDelegate");
                }
            }
            catch
            {
            }
            return XInputGetKeystroke(dwUserIndex, dwReserved, out pKeystroke);
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetBatteryInformation(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation);
        delegate void DXInputGetBatteryInformationAsync(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetBatteryInformation")]
        static extern uint XInputGetBatteryInformation(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation);

        static uint XInputGetBatteryInformation_Hooked(int dwUserIndex, int devType, out BatteryInformation pBatteryInformation)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetBatteryInformation");
                }
            }
            catch
            {
            }
            return XInputGetBatteryInformation(dwUserIndex, devType, out pBatteryInformation);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetDSoundAudioDeviceGuids(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid);
        delegate void DXInputGetDSoundAudioDeviceGuidsAsync(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetDSoundAudioDeviceGuids")]
        static extern uint XInputGetDSoundAudioDeviceGuids(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid);
        static uint XInputGetDSoundAudioDeviceGuids_Hooked(int dwUserIndex, out Guid pDSoundRenderGuid, out Guid pDSoundCaptureGuid)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetDSoundAudioDeviceGuids");
                }
            }
            catch
            {
            }
            return XInputGetDSoundAudioDeviceGuids(dwUserIndex, out pDSoundRenderGuid, out pDSoundCaptureGuid);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputEnable(Bool enable);
        delegate void DXInputEnableAsync(Bool enable);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputEnable")]
        static extern uint XInputEnable(Bool enable);
        static uint XInputEnable_Hooked(Bool enable)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputEnable");
                }
            }
            catch
            {
            }
            return XInputEnable(enable);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetCapabilities(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities);
        delegate void DXInputGetCapabilitiesAsync(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetCapabilities")]
        static extern uint XInputGetCapabilities(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities);
        static uint XInputGetCapabilities_Hooked(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities pCapabilities)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("XInputGetCapabilities");
                }
            }
            catch
            {
            }
            return XInputGetCapabilities(dwUserIndex, dwFlags, out pCapabilities);
        }
    }
}
