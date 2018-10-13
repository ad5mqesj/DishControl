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
		}

#if DEBUG

		public void OnStart()
		{			
			this.OnStart(null);
		}

#endif

		protected override void OnStart(string[] args)
		{
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
