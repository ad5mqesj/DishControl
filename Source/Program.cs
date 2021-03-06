﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinfordEthIO;

namespace DishControl
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if _TEST
            InterfaceFake.Eth32 dev =new InterfaceFake.Eth32();
#else
            Eth32 dev = new Eth32();
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(dev));
        }
    }
}
