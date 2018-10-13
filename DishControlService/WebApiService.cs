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
			//Program.Log.Write("WebApi: Start");
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
			//Program.Log.Write("WebApi: Start");
			WebApp.Start<WebApi>(url: baseAddress);
			//Program.Log.Write("WebApi: Complete");
		}

		protected override void OnStop()
		{
			//Program.Log.Write("WebApi: OnStart");
			// noone seems to worry about the shutdown...	
		}
	}
}
