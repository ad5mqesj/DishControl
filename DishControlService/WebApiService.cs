using System.Configuration;
using System.ServiceProcess;
using DishControl.App_Start;
using Microsoft.Owin.Hosting;

namespace DishControl.Service
{
	public partial class WebApiService : ServiceBase
	{


        public WebApiService()
		{
			InitializeComponent();
			BasicLog.writeLog("WebApi: Start");
            Program.mControl = new MotionControl();
            BasicLog.writeLog(string.Format("Motion Control {0} configured", Program.mControl.appConfigured ? "is" : "is not"));
        }

#if DEBUG

		public void OnStart()
		{			
			this.OnStart(null);
		}

#endif

		protected override void OnStart(string[] args)
		{
            if (Program.mControl.appConfigured)
            {
                BasicLog.writeLog("Initialize Motion Control");
                Program.mControl.Connect();
                BasicLog.writeLog(string.Format("Motion Control Connection {0}", Program.mControl.isConnected() ? "Succeeded" : "Failed"));
            }
            string baseAddress = ConfigurationManager.AppSettings["WebAPIBaseAddress"];
            BasicLog.writeLog("WebApi: Start");
			WebApp.Start<WebApi>(url: baseAddress);
            BasicLog.writeLog("WebApi: Complete");
        }

        protected override void OnStop()
		{
            BasicLog.writeLog("WebApi: OnStart");
            // noone seems to worry about the shutdown...	
        }
    }
}
