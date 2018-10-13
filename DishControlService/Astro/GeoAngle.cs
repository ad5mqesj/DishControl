using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DishControl
{
    public class GeoAngle
    {
        public bool IsNegative { get; set; }
        public int Degrees { get; set; }
        public int Minutes { get; set; }
        public double Seconds { get; set; }

        public static double ConvertDegreeAngleToDouble(string point)
        {
            //Example: 17° 21 18 S

            var multiplier = (point.Contains("S") || point.Contains("W")) ? -1 : 1; //handle south and west

            point = Regex.Replace(point, "[^0-9.]", ""); //remove the characters

            var pointArray = point.Split(' '); //split the string.

            //Decimal degrees = 
            //   whole number of degrees, 
            //   plus minutes divided by 60, 
            //   plus seconds divided by 3600

            var degrees = Double.Parse(pointArray[0]);
            var minutes = Double.Parse(pointArray[1]) / 60;
            var seconds = Double.Parse(pointArray[2]) / 3600;

            return (degrees + minutes + seconds) * multiplier;
        }

        public static GeoAngle FromDouble(double angleInDegrees, bool range360 = false)
        {
            //ensure the value will fall within the primary range [-180.0..+180.0]
            if (!range360)
            {
                while (angleInDegrees < -180.0)
                    angleInDegrees += 360.0;

                while (angleInDegrees > 180.0)
                    angleInDegrees -= 360.0;
            }
            else
            {
                while (angleInDegrees < 0.0)
                    angleInDegrees += 360.0;

                while (angleInDegrees > 360.0)
                    angleInDegrees -= 360.0;
            }
            var result = new GeoAngle();

            //switch the value to positive
            result.IsNegative = angleInDegrees < 0;
            angleInDegrees = Math.Abs(angleInDegrees);

            //gets the degree
            result.Degrees = (int)Math.Floor(angleInDegrees);
            var delta = angleInDegrees - result.Degrees;

            //gets minutes and seconds
            result.Minutes = (int)Math.Floor(delta * 60.0);
            double seconds = 3600.0 * delta;
            result.Seconds = seconds % 60.0;

            return result;
        }

        public override string ToString()
        {
            var degrees = this.IsNegative
                ? -this.Degrees
                : this.Degrees;

            return string.Format(
                "{0}° {1:00}' {2:00}\"",
                degrees,
                this.Minutes,
                this.Seconds);
        }

        public string ToString(string format)
        {
            switch (format)
            {
                case "NS":
                    return string.Format(
                        "{0}° {1:00}' {2:00.000}\" {3}",
                        this.Degrees,
                        this.Minutes,
                        this.Seconds,
                        this.IsNegative ? 'S' : 'N');

                case "WE":
                    return string.Format(
                        "{0}° {1:00}' {2:00.000}\" {3}",
                        this.Degrees,
                        this.Minutes,
                        this.Seconds,
                        this.IsNegative ? 'W' : 'E');

                default:
                    throw new NotImplementedException();
            }
        }

        public static GeoAngle FromString (string point)
        {
            var multiplier = (point.Contains("S") || point.Contains("W")) ? -1 : 1; //handle south and west

            point = Regex.Replace(point, "[^0-9. :]", ""); //remove the characters

            var pointArray = point.Split(new char [] {':' }); //split the string.

            for (int i = 0, j = pointArray.Length; i < j; i++)
            {
                pointArray[i] = Regex.Replace(pointArray[i], "[^0-9. ]", ""); //remove the characters
            }
            //Decimal degrees = 
            //   whole number of degrees, 
            //   plus minutes divided by 60, 
            //   plus seconds divided by 3600
            var result = new GeoAngle();
            int d = 0;
            Int32.TryParse(pointArray[0], out d);
            result.Degrees = d;
            d = 0;
            if (pointArray.Length > 1)
                Int32.TryParse(pointArray[1], out d);
            result.Minutes = d;
            double dd = 0;
            if (pointArray.Length > 2)
                double.TryParse(pointArray[2], out dd);
            result.Seconds = dd;
            if (multiplier < 0)
            {
                result.IsNegative = true;
            }
            else
            {
                result.IsNegative = false;
            }
            return result;
        }

        public double ToDouble()
        {
            return ((double)this.Degrees + (double)this.Minutes / 60.0 + this.Seconds / 3600.0) * (this.IsNegative ? -1.0 : 1.0);
        }
    }
}
