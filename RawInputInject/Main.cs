using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using EasyHook;
using System.Reflection;

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
            try
            {
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "GetRawInputData"),
                    new DGetRawInputData(GetRawInputData_hook),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "GetRawInputDeviceInfoW"),
                    new DGetRawInputDeviceInfo(GetRawInputDeviceInfo_hook),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "GetRawInputDeviceList"),
                    new DGetRawInputDeviceList(GetRawInputDeviceList_hook),
                    this));
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "RegisterRawInputDevices"),
                    new DRegisterRawInputDevices(RegisterRawInputDevices_hook),
                    this));
                /*
                 * Don't forget that all hooks will start deaktivated...
                 * The following ensures that all threads are intercepted:
                 */
                foreach (LocalHook hook in Hooks)
                    hook.ThreadACL.SetExclusiveACL(new Int32[1]);
            }
            catch (Exception e)
            {
                /*
                    Now we should notice our host process about this error...
                 */
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), Assembly.GetExecutingAssembly().GetName().Name, e);

                return;
            }


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
        delegate uint DGetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        delegate uint DGetRawInputDataAsync(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);
        [DllImport("user32.dll", EntryPoint = "GetRawInputData", CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        static uint GetRawInputData_hook(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;
                //TODO
                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("GetRawInputData");
                }
            }
            catch
            {
            }
            return GetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DGetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);
        delegate uint DGetRawInputDeviceInfoAsync(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);
        [DllImport("user32.dll", EntryPoint = "GetRawInputDeviceInfo", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        static uint GetRawInputDeviceInfo_hook(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;
                //TODO
                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("GetRawInputDeviceInfo");
                }
            }
            catch
            {
            }
            return GetRawInputDeviceInfo(hDevice, uiCommand, pData, ref pcbSize);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public Int32 dwType;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DGetRawInputDeviceList([Out]RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);
        delegate uint DGetRawInputDeviceListAsync([Out]RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);
        [DllImport("user32.dll")]
        public static extern uint GetRawInputDeviceList([Out]RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);
        static uint GetRawInputDeviceList_hook([Out]RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;
                //TODO
                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("GetRawInputDeviceInfo");
                }
            }
            catch
            {
            }
            return GetRawInputDeviceList(pRawInputDeviceList, ref puiNumDevices, cbSize);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsagePage;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsage;
            [MarshalAs(UnmanagedType.U4)]
            public int dwFlags;
            public IntPtr hwndTarget; // The window that will receive WM_INPUT messages
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool DRegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, int cbSize);
        delegate bool DRegisterRawInputDevicesAsync(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, int cbSize);
        [DllImport("user32.dll", EntryPoint = "RegisterRawInputDevices")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, int cbSize);
        static bool RegisterRawInputDevices_hook(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, int cbSize)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;
                //TODO
                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push("RegisterRawInputDevices");
                }
            }
            catch
            {
            }
            return RegisterRawInputDevices(pRawInputDevices, uiNumDevices, cbSize);
        }
    }
}
