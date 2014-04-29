using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using EasyHook;

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
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
                    new DCreateFile(CreateFile_Hooked),
                    this));

                /*
                 * Don't forget that all hooks will start deaktivated...
                 * The following ensures that all threads are intercepted:
                 */
                foreach(LocalHook hook in Hooks)
                    hook.ThreadACL.SetExclusiveACL(new Int32[1]);
            }
            catch (Exception e)
            {
                /*
                    Now we should notice our host process about this error...
                 */
                Interface.ReportError(RemoteHooking.GetCurrentProcessId(), e);

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

                            Interface.OnCreateFile(RemoteHooking.GetCurrentProcessId(), Package);
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
        delegate IntPtr DCreateFile(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        delegate void DCreateFileAsync(
            Int32 InClientPID,
            IntPtr InHandle,
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        // just use a P-Invoke implementation to get native API access from C# (this step is not necessary for C++.NET)
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFile(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        // this is where we are intercepting all file accesses!
        static IntPtr CreateFile_Hooked(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile)
        {
            try
            {
                XboxOneControllerInjection This = (XboxOneControllerInjection)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if (This.Queue.Count < 1000)
                        This.Queue.Push(InFileName);
                }
            }
            catch
            {
            }

            // call original API...
            return CreateFile(
                InFileName,
                InDesiredAccess,
                InShareMode,
                InSecurityAttributes,
                InCreationDisposition,
                InFlagsAndAttributes,
                InTemplateFile);
        }

        /*[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate uint DXInputGetState(PlayerIndex playerIndex, out XINPUT_STATE pState);
        delegate void DXInputGetStateAsync(PlayerIndex playerIndex, out XINPUT_STATE pState);

        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")]
        static extern uint XInputGetState(PlayerIndex playerIndex, out XINPUT_STATE pState);

        static uint XInputGetState_Hooked(PlayerIndex playerIndex, out XINPUT_STATE pState)
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
        }*/
    }
}
