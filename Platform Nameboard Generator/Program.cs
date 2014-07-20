using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Platform_Nameboard_Generator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit); 
            Application.Run(new Form1());
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            //Cleanup temporary textures
            string[] filePaths = Directory.GetFiles(Form1.temppath);
            foreach (string filePath in filePaths)
            {

                File.Delete(filePath);
            }
            Directory.Delete(Form1.temppath);
        }
    }
}
