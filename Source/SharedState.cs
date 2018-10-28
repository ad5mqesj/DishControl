using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DishControl
{
    public class SharedState
    {
        public SharedState()
        {
            Azimuth = 0.0;
            Elevation = 0.0;
            state = DishState.Unknown;
        }

        public double Azimuth { get; set; }
        public double Elevation { get; set; }
        public DishState state { get; set; }  

    }
}
