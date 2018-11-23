using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DishControl
{
    public class SharedState
    {
        public SharedState()
        {
            azimuth = 0.0;
            elevation = 0.0;
            commandAzimuth = 0.0;
            commandElevation = 0.0;
            commandAzimuthRate = 0.0;
            commandElevationRate = 0.0;
            state = DishState.Unknown;
            connected = false;
            trackCelestial = false;
            trackMoon = false;
            btEnabled = true;

            appConfigured = false;
            connectEvent = new ManualResetEvent(false);

            command = CommandType.Unknown;
            go = new ManualResetEvent(false);
        }

        public double azimuth { get; set; }
        public double elevation { get; set; }

        public double commandAzimuth { get; set; }
        public double commandElevation { get; set; }
        public double commandRightAscension { get; set; }
        public double commandDeclination { get; set; }

        public double commandAzimuthRate { get; set; }
        public double commandElevationRate { get; set; }

        public DishState state { get; set; }  

        public bool connected { get; set; }

        public bool trackCelestial { get; set; }
        public bool trackMoon { get; set; }
        public bool btEnabled { get; set; }

        public bool appConfigured { get; set; }
        public ManualResetEvent connectEvent;

        public CommandType command { get; set; }
        public ManualResetEvent go;
    }
}
