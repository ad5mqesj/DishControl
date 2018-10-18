using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json;

namespace DishControl.Service.Controller
{
    public class PingController : ApiController
    {
        [HttpGet]
        [Route("Ping")]
        public string Ping()
        {
            return "Dish Control Service";
        }

        [HttpGet]
        [Route("FullPing")]
        public string FullPing()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var retval = new 
            {
                ApplicationName = "Dish Control Service",
                Version = asm.GetName().Version.ToString(),
#if DEBUG
                BuildType = "Debug"
#else
			    BuildType =  "Release"						
#endif
            };
            string output = JsonConvert.SerializeObject(retval);
            return output;
        }

    }
}
