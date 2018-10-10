using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DishContorl.Service
{

	public static class Program
	{
//		public static Logger Log = new Logger(null);

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			/*
			string log = ConfigurationManager.AppSettings["LogFile"];
			if (!String.IsNullOrWhiteSpace(log))
			{
				Program.Log = new Logger(log);
			}
			*/
			WebApiService webapiservice = null;

#if DEBUG

			if (Debugger.IsAttached)
			{
				//Log.Write("Debug Console mode");
				webapiservice = new WebApiService();
				//Log.Write("OnStart: Call");
				webapiservice.OnStart();
				//Log.Write("OnStart: Complete");
				System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
			}
#endif
			if (webapiservice == null)
			{
				//Log.Write("Service mode");
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new WebApiService() 
				};
				//Log.Write("Run: Call");				
				ServiceBase.Run(ServicesToRun);
			}


		}
	}
}
