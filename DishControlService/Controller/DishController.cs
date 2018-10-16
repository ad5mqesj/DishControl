using DishControl.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace DishControl.Service.Controller
{
    public class DishController : ApiController
    {
        [HttpGet]
        [Route("ConnectionStatus")]
        public JsonResult<bool> GetConnectionStatus()
        {
            return Json(Program.mControl.isConnected());
        }

        [HttpGet]
        [Route("Position")]
        public JsonResult<PositionResult> GetPosition()
        {
            RaDec astro = celestialConversion.CalcualteRaDec(Program.mControl.elPos, Program.mControl.azPos, Program.mControl.settings.latitude, Program.mControl.settings.longitude);
            PositionResult result = new PositionResult()
            {
                Azimuth = Program.mControl.azPos,
                Elevation = Program.mControl.elPos,
                RightAscension = astro.RA,
                Declination = astro.Dec
            };
            return Json(result);
        }


    }
}
