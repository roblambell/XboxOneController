using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XboxOnePadReader;

namespace PadReaderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ControllerReader myController = ControllerReader.Instance;
            int x = 0;
            while (x < 5)
            {
                Thread.Sleep(1000);
                ++x;
            }
            myController.CloseController();

            XboxOneController.State lol = new XboxOneController.State();
            lol.PacketNumber = 1;
        }
    }
}
