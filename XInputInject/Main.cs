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
                Hooks.Add(LocalHook.Create(LocalHook.GetProcAddress("xinput1_3.dll", "XInputGetState"),
                    new DXInputGetState(XInputGetState_Hooked),
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
        }
    }
}
