namespace DishControl.Service.Models
{
    public class PositionResult
    {
        public PositionResult()
        {
            Azimuth = 0.0;
            Elevation = 0.0;
            RightAscension = 0.0;
            Declination = 0.0;
        }

        public double Azimuth { get; set; }
        public double Elevation { get; set; }
        public double RightAscension { get; set; }
        public double Declination { get; set; }

        public string formattedAzimuth
        {
            get
            {
                return GeoAngle.FromDouble(this.Azimuth).ToString();
            }
        }
        public string formattedElevation
        {
            get
            {
                return GeoAngle.FromDouble(this.Elevation).ToString();
            }
        }

        public string formattedRightAscension
        {
            get
            {
                return GeoAngle.FromDouble(this.RightAscension).ToString();
            }
        }

        public string formattedDeclination
        {
            get
            {
                return GeoAngle.FromDouble(this.Declination).ToString();
            }
        }

    }
}
