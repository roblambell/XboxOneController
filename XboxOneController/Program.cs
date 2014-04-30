using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using EasyHook;

namespace XboxOneController
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool noGAC = false;
            try
            {
                Config.Register(
                    "XboxOneController Hook",
                    "XinputInject.dll",
                    "RawInputInject.dll",
                    "SharpDX.dll",
                    "SharpDX.XInput.dll",
                    "XboxOneController.exe");
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show("This is an administrative task! Attempting without GAC...", "Permission denied...", MessageBoxButtons.OK);

                noGAC = true;
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(noGAC));
        }
    }
}
