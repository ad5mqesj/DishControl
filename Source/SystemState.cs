using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DishControl
{
    public enum DishState
    {
        Unknown,
        Stopped,
        Moving, 
        Tracking,
        Parking
    }

    public enum CommandType
    {
        Unknown,
        Jog,
        Move,
        Track,
        Stop
    }
}
