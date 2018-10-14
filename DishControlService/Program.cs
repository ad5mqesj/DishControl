using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DishControl.Service
{

	public static class Program
	{

        static public MotionControl mControl;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
		{
			WebApiService webapiservice = null;

#if DEBUG

            if (Debugger.IsAttached)
			{
                BasicLog.writeLog("Debug Console mode");
				webapiservice = new WebApiService();
                BasicLog.writeLog("OnStart: Call");
				webapiservice.OnStart();
                BasicLog.writeLog("OnStart: Complete");
				System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
			}
#endif
			if (webapiservice == null)
			{
                BasicLog.writeLog("Service mode");
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new WebApiService() 
				};
                BasicLog.writeLog("Run: Call");				
				ServiceBase.Run(ServicesToRun);
			}


		}
	}
}
