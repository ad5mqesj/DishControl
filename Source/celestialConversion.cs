using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DishControl
{
    public class AltAz
    {
        public double Alt { get; set; }
        public double Az { get; set; }
        public double distance { get; set; }
        public double paralacticAngle { get; set; }
    }

    public class RaDec
    {
        public double RA { get; set; }
        public double Dec { get; set; }

        public double distance { get; set; }
        public void Normalize()
        {
            if (this.RA > 24.0)
                this.RA = 24.0;
            else if (this.RA < -0.0)
                this.RA = 0.0;

            if (this.Dec > 180.0)
                this.Dec = 180.0;
            else if (this.Dec < -180.0)
                this.Dec = 180.0;
        }
    }

    public class celestialConversion
    {
        /// <summary>
        /// DateTime will be set to current UTC time
        /// </summary>
        /// <param name="RA">The right ascension in decimal value</param>
        /// <param name="Dec">The declination in decimal value</param>
        /// <param name="Lat">The latitude in decimal value</param>
        /// <param name="Long">The longitude in decimal value</param>
        /// <returns>The altitude and azimuth in decimal value</returns>
        public static AltAz CalculateAltAz(double RA, double Dec, double Lat, double Long)
        {
            return CalculateAltAz(RA, Dec, Lat, Long, DateTime.UtcNow);
        }

        /// <summary>
        /// </summary>
        /// <param name="RA">The right ascension in decimal value</param>
        /// <param name="Dec">The declination in decimal value</param>
        /// <param name="Lat">The latitude in decimal value</param>
        /// <param name="Long">The longitude in decimal value</param>
        /// <param name="Date">The date(time) in UTC</param>
        /// <returns>The altitude and azimuth in decimal value</returns>
        public static AltAz CalculateAltAz(double RA, double Dec, double Lat, double Long, DateTime Date)
        {
            // Day offset and Local Siderial Time
            double dayOffset = (Date - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalDays;
            double LST = (100.46 + 0.985647 * dayOffset + Long + 15 * (Date.Hour + Date.Minute / 60d) + 360) % 360;

            // Hour Angle
            double HA = (LST - RA + 360) % 360;

            // HA, DEC, Lat to Alt, AZ
            double x = Math.Cos(HA * (Math.PI / 180)) * Math.Cos(Dec * (Math.PI / 180));
            double y = Math.Sin(HA * (Math.PI / 180)) * Math.Cos(Dec * (Math.PI / 180));
            double z = Math.Sin(Dec * (Math.PI / 180));

            double xhor = x * Math.Cos((90 - Lat) * (Math.PI / 180)) - z * Math.Sin((90 - Lat) * (Math.PI / 180));
            double yhor = y;
            double zhor = x * Math.Sin((90 - Lat) * (Math.PI / 180)) + z * Math.Cos((90 - Lat) * (Math.PI / 180));

            double az = Math.Atan2(yhor, xhor) * (180 / Math.PI) + 180;
            double alt = Math.Asin(zhor) * (180 / Math.PI);

            return new AltAz()
            {
                Alt = alt,
                Az = az
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="Alt">The Altitude (90-elevation) in decimal degrees</param>
        /// <param name="Az">The Azimuth in decimal degrees</param>
        /// <param name="Lat">The latitude in decimal degrees</param>
        /// <param name="Long">The longitude in decimal value</param>
        /// <returns>The Right Ascentsion and Decination decimal value</returns>
        public static RaDec CalcualteRaDec(double Alt, double Az, double Lat, double Long)
        {
            return CalcualteRaDec(Alt, Az, Lat, Long, DateTime.UtcNow);
        }

       /// <summary>
       /// </summary>
       /// <param name="Alt">The Altitude (90-elevation) in decimal degrees</param>
       /// <param name="Az">The Azimuth in decimal degrees</param>
       /// <param name="Lat">The latitude in decimal degrees</param>
       /// <param name="Long">The longitude in decimal value</param>
       /// <param name="Date">The date(time) in UTC</param>
       /// <returns>The Right Ascentsion and Decination decimal value</returns>
        public static RaDec CalcualteRaDec(double Alt, double Az, double Lat, double Long, DateTime Date)
        {
            double temp, sin_dec, lat = Lat * (Math.PI / 180);
            double cos_lat = Math.Cos(lat);
            double alt, az, dec, ha, ra;

            alt = Alt * (Math.PI / 180);
            az = Az * (Math.PI / 180);

            if (alt > Math.PI / 2.0)
            {
                alt = Math.PI - alt;
                az += Math.PI;
            }
            if (alt < -Math.PI / 2.0)
            {
                alt = -Math.PI - alt;
                az -= Math.PI;
            }
            sin_dec = Math.Sin(lat) * Math.Sin(alt) + cos_lat * Math.Cos(alt) * Math.Cos(az);
            dec = Math.Asin(sin_dec);

            if (cos_lat < .00001)         /* polar case */
            {
                ha = az + Math.PI;
            }
            else
            {
                temp = cos_lat * Math.Cos(dec);
                temp = (Math.Sin(alt) - Math.Sin(lat) * sin_dec) / temp;
                if (temp > 1.0)
                    temp = 1.0;
                temp = Math.Acos(-temp);
                ///THIS WAS BACKWARDS!
                if (Math.Sin(az) < 0.0)
                    ha = Math.PI - temp;
                else
                    ha = Math.PI + temp;
            }
            double dayOffset = (Date - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalDays;
            double LST = (100.46 + 0.985647 * dayOffset + Long + 15 * (Date.Hour + Date.Minute / 60d) + 360) % 360;
            ra = (LST - (ha * 180.0 / Math.PI) + 360) % 360;

            RaDec rd =  new RaDec()
            {
                RA = ra * 24 / 360,
                Dec = dec * 180.0 / Math.PI
            };
            rd.Normalize();
            return rd;
        }

        /// <summary>
        /// </summary>
        /// <param name="year">The yeah 4 digits</param>
        /// <param name="month">month number 1-12</param>
        /// <param name="day">The day of month</param>
        /// <returns>The Julian date decimal value</returns>
        private static double calcJD(int year, int month, int day)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            var A = Math.Floor(year / 100d);
            var B = 2 - A + Math.Floor(A / 4d);

            var JD = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1.0)) + day + B - 1524.5;

            return JD;
        }

    }
}
