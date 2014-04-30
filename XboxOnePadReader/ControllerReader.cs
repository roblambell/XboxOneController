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
        private static ControllerReader instance;
        public List<ThreadController> controllers;
        private List<Thread> controllersThread;
        private const int idVendor = 0x045E;
        private const int idProduct = 0x02D1;

        public ControllerReader()
        {
            //Find devices matching the vendor and product ID
            UsbDeviceFinder USBFinder = new UsbDeviceFinder(idVendor, idProduct);
            //Using the registry create a list of those devices
            UsbRegDeviceList USBRegistryDevices = new UsbRegDeviceList();
            USBRegistryDevices = USBRegistryDevices.FindAll(USBFinder);
            
            foreach (UsbRegistry winUsbRegistry in USBRegistryDevices)
            {
                if (controllers.Count == 4)
                    return;

                try
                {
                    UsbDevice myDevice = winUsbRegistry.Device;
                    ThreadController controller = new ThreadController(winUsbRegistry.Device);

                    Thread myThread = new Thread(new ThreadStart(controller.UpdateState));

                    controllersThread.Add(myThread);
                    controllers.Add(controller);
                }
                catch (Exception) { }
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
