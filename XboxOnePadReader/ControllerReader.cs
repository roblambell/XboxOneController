using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XboxOnePadReader
{
    public class ControllerReader
    {
        private static ControllerReader instance = null;
        public List<ThreadController> controllers = new List<ThreadController>();
        private List<Thread> controllersThread = new List<Thread>();
        private const int idVendor = 0x045E;
        private const int idProduct = 0x02D1;

        public ControllerReader()
        {
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;

            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Pid == idProduct && usbRegistry.Vid == idVendor)
                {
                    if (controllers.Count == 4)
                        return;

                    try
                    {
                        UsbDevice myDevice = usbRegistry.Device;
                        ThreadController controller = new ThreadController(usbRegistry.Device);

                        Thread myThread = new Thread(new ThreadStart(controller.UpdateState));

                        controllersThread.Add(myThread);
                        controllers.Add(controller);
                        myThread.Start();
                    }
                    catch (Exception) { }
                }
            }
        }

        public void CloseController()
        {
            foreach (ThreadController controller in controllers)
                controller.StopThread();
            foreach (Thread controllerThread in controllersThread)
            {
                while (controllerThread.IsAlive)
                    Thread.Sleep(1);
            }
        }

        public static ControllerReader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ControllerReader();
                }
                return instance;
            }
        }
    }
}
