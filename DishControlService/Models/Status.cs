using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DishControl.Service.Models
{
    [Serializable]
    public class Status : ISerializable
    {
        public bool Connected { get; set; }
        public DishState State { get; set; }
        public bool Tracking { get; set; }

        public Status()
        {
            Connected = false;
            Tracking = false;
            State = DishState.Unknown;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Connected", Connected.ToString());
            info.AddValue("State", State.ToString("F"));
            info.AddValue("Tracking", Tracking.ToString());
        }

        protected Status(SerializationInfo info, StreamingContext context)
        {
            this.Connected = info.GetBoolean("Connected");
            DishState state;
            Enum.TryParse(info.GetString("State"), out state);
            this.State = state;
            this.Tracking = info.GetBoolean("Tracking");
        }
    }
}
