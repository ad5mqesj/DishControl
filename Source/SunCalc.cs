using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DishControl
{
    public class Suntimes
    {
        public double angle { get; set; }
        public string morningName { get; set; }
        public string eveningName { get; set; }
    }

    public class riseSetTimes
    {
        public Dictionary<string, long> times { get; set; }
        public long nadir { get; set; }
        public long solarNoon { get; set; }
    }
    public class moonRiseSet
    {
        public long riseTime { get; set; }
        public long settime { get; set; }
        public string state { get; set; }
    }

    public class illumination
    {
        public double fraction { get; set; }
        public double phase { get; set; }
        public double angle { get; set; }
    }

    public class SunCalc
    {
        double rad = Math.PI / 180.0,
            dar = 180.0 / Math.PI;
        long dayMs = 1000 * 60 * 60 * 24,
            days2s = 86400,
            J1970 = 2440588,
            J2000 = 2451545;

        double e = Math.PI / 180.0 * 23.4397;  // obliquity of the Earth
        public long epochDate(DateTime date)
        {
            TimeSpan span = date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)span.TotalMilliseconds;
        }

        public double toJulian(long date)
        {

            return ((double)date / (double)dayMs - 0.5) + J1970;
        }

        public long fromJulian(long j)
        {
            return (long)(j + 0.5 - J1970) * dayMs;
        }

        public double toDays(long date)
        {
            return toJulian(date) - J2000;
        }
        public double rightAscension(double l, double b)
        {
            return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
        }
        public double declination(double l, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
        }

        public double azimuth(double H, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(H), Math.Cos(H) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }
        public double altitude(double H, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(H));
        }

        public double siderealTime(double d, double lw)
        {
            return rad * (280.16 + 360.9856235 * d) - lw;
        }

        public double astroRefraction(double h)
        {
            if (h < 0) // the following formula works for positive altitudes only.
                h = 0; // if h = -0.08901179 a div/0 would occur.

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / Math.Tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
        }


        public double solarMeanAnomaly(double d)
        {
            return rad * (357.5291 + 0.98560028 * d);
        }

        public double eclipticLongitude(double M)
        {

            double C = rad * (1.9148 * Math.Sin(M) + 0.02 * Math.Sin(2 * M) + 0.0003 * Math.Sin(3 * M)), // equation of center
                P = rad * 102.9372; // perihelion of the Earth

            return M + C + P + Math.PI;
        }

        public RaDec sunCoords(double d)
        {

            double M = solarMeanAnomaly(d),
                L = eclipticLongitude(M);

            return new RaDec()
            {
                Dec = declination(L, 0),
                RA = rightAscension(L, 0)
            };
        }

        // calculates sun position for a given date and latitude/longitude

        public AltAz getPosition(int date, double lat, double lng)
        {

            double lw = rad * -lng,
                phi = rad * lat,
                d = toDays(date);

            RaDec c = sunCoords(d);
            double H = siderealTime(d, lw) - c.RA;

            return new AltAz()
            {
                Az = azimuth(H, phi, c.Dec)*dar+180.0,
                Alt = altitude(H, phi, c.Dec)*dar
            };
        }


        // sun times configuration (angle, morning name, evening name)

        public Suntimes[] times = new Suntimes[]
        {
            new Suntimes() { angle = -0.833, morningName = "sunrise", eveningName = "sunset"},
            new Suntimes() { angle = -0.3,  morningName = "sunriseEnd", eveningName = "sunsetStart"},
            new Suntimes() { angle = -6,  morningName = "dawn", eveningName = "dusk" },
            new Suntimes() { angle = -12,  morningName = "nauticalDawn", eveningName = "nauticalDusk"},
            new Suntimes() { angle = -18,  morningName = "nightEnd", eveningName = "night" },
            new Suntimes() { angle = 6,  morningName = "goldenHourEnd", eveningName = "goldenHour"}
        };

        // adds a custom time to the times config

        /*    SunCalc.addTime = function(angle, riseName, setName)
            {
                times.push([angle, riseName, setName]);
            };
        */

        // calculations for sun times

        double J0 = 0.0009;

        public double julianCycle(double d, double lw)
        {
            return Math.Round(d - J0 - lw / (2 * Math.PI));
        }

        public double approxTransit(double Ht, double lw, double n)
        {
            return J0 + (Ht + lw) / (2 * Math.PI) + n;
        }
        public double solarTransitJ(double ds, double M, double L)
        {
            return J2000 + ds + 0.0053 * Math.Sin(M) - 0.0069 * Math.Sin(2 * L);
        }

        public double hourAngle(double h, double phi, double d)
        {
            return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
        }

        // returns set time for the given sun altitude
        public double getSetJ(double h, double lw, double phi, double dec, double n, double M, double L)
        {

            double w = hourAngle(h, phi, dec),
                a = approxTransit(w, lw, n);
            return solarTransitJ(a, M, L);
        }

        // calculates sun times for a given date and latitude/longitude
        public riseSetTimes getSunTimes(long date, double lat, double lng)
        {
            double lw = rad * -lng,
                   phi = rad * lat;

            double d = toDays(date);
            double n = julianCycle(d, lw),
            ds = approxTransit(0, lw, n),

            M = solarMeanAnomaly(ds),
            L = eclipticLongitude(M),
            dec = declination(L, 0), time, Jset, Jrise,

            Jnoon = solarTransitJ(ds, M, L);

            int i, len;

            riseSetTimes result = new riseSetTimes();
            result.times = new Dictionary<string, long>();
            result.nadir = fromJulian((long)(Jnoon - 0.5));
            result.solarNoon = fromJulian((long)Jnoon);

            for (i = 0, len = times.Length; i < len; i += 1)
            {
                time = times[i].angle;

                Jset = getSetJ(time * rad, lw, phi, dec, n, M, L);
                Jrise = Jnoon - (Jset - Jnoon);

                result.times.Add(times[i].morningName, fromJulian((long)Jrise));
                result.times.Add(times[i].eveningName, fromJulian((long)Jrise));
            }

            return result;
        }


        // moon calculations, based on http://aa.quae.nl/en/reken/hemelpositie.html formulas
        RaDec moonCoords(double d)
        {
            // geocentric ecliptic coordinates of the moon
            double L = rad * (218.316 + 13.176396 * d), // ecliptic longitude
                M = rad * (134.963 + 13.064993 * d), // mean anomaly
                F = rad * (93.272 + 13.229350 * d),  // mean distance

                l = L + rad * 6.289 * Math.Sin(M), // longitude
                b = rad * 5.128 * Math.Sin(F),     // latitude
                dt = 385001 - 20905 * Math.Cos(M);  // distance to the moon in km
            double T = d / 36525.0;
            double E1 = 125.045 - 0.0529921 * d;
            double E2 = 250.089 - 0.1059842 * d;
            double moonRA = rightAscension(l, b) / rad;
            double moonDec = declination(l, b) / rad;
            return new RaDec()
            {
                RA = rightAscension(l, b),
                Dec = declination(l, b),
                distance = dt
            };
        }

        public AltAz getMoonPosition(long date, double lat, double lng)
        {

            double lw = rad * -lng,
                phi = rad * lat,
                d = toDays(date);

            RaDec c = moonCoords(d);
            double H = siderealTime(d, lw) - c.RA,
                h = altitude(H, phi, c.Dec),
                // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
                pa = Math.Atan2(Math.Sin(H), Math.Tan(phi) * Math.Cos(c.Dec) - Math.Sin(c.Dec) * Math.Cos(H));

            double refraction = astroRefraction(h);
            h = h + refraction; // altitude correction for refraction
            refraction = refraction / rad;
            ///HACK HACK magic offsets .76 and .985 determined empirically
            ///
            return new AltAz()
            {
                Az = azimuth(H, phi, c.Dec)*dar+180.0 - 0.76,
                Alt = h*dar-0.985,
                distance = c.distance,
                paralacticAngle = pa
            };
        }


        // calculations for illumination parameters of the moon,
        // based on http://idlastro.gsfc.nasa.gov/ftp/pro/astro/mphase.pro formulas and
        // Chapter 48 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
        public illumination getMoonIllumination(long date)
        {

            double d = toDays(date);
            RaDec s = sunCoords(d),
            m = moonCoords(d);

            double sdist = 149598000, // distance from Earth to Sun in km

                phi = Math.Acos(Math.Sin(s.Dec) * Math.Sin(m.Dec) + Math.Cos(s.Dec) * Math.Cos(m.Dec) * Math.Cos(s.RA - m.RA)),
                inc = Math.Atan2(sdist * Math.Sin(phi), m.distance - sdist * Math.Cos(phi)),
                angle = Math.Atan2(Math.Cos(s.Dec) * Math.Sin(s.RA - m.RA), Math.Sin(s.Dec) * Math.Cos(m.Dec) -
                        Math.Cos(s.Dec) * Math.Sin(m.Dec) * Math.Cos(s.RA - m.RA));

            return new illumination()
            {
                fraction = (1 + Math.Cos(inc)) / 2,
                phase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / Math.PI,
                angle = angle
            };
        }
        public long getEpochTime()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public long hoursLater(long date, int h)
        {
            return date + h * days2s / 24;
        }

        // calculations for moon rise/set times are based on http://www.stargazing.net/kepler/moonrise.html article
        public moonRiseSet getMoonTimes(long date, double lat, double lng, bool inUTC)
        {
            long t = date;
            //            if (inUTC) t.setUTCHours(0, 0, 0, 0);
            //            else t.setHours(0, 0, 0, 0);

            double hc = 0.133 * rad;
            AltAz gmp = getMoonPosition(t, lat, lng);
            double h0 = gmp.Alt - hc,
                h1, h2, rise = 0.0, set = 0.0, a, b, xe, ye = 0.0, d, roots, x1 = 0.0, x2 = 0.0, dx;

            // go in 2-hour chunks, each time seeing if a 3-point quadratic curve crosses zero (which means rise or set)
            for (var i = 1; i <= 24; i += 2)
            {
                AltAz gmp1 = getMoonPosition(hoursLater(t, i), lat, lng);
                AltAz gmp2 = getMoonPosition(hoursLater(t, i + 1), lat, lng);
                h1 = gmp1.Alt - hc;
                h2 = gmp2.Alt - hc;

                a = (h0 + h2) / 2 - h1;
                b = (h2 - h0) / 2;
                xe = -b / (2 * a);
                ye = (a * xe + b) * xe + h1;
                d = b * b - 4 * a * h1;
                roots = 0;
                x1 = 0.0;
                x2 = 0.0;
                if (d >= 0)
                {
                    dx = Math.Sqrt(d) / (Math.Abs(a) * 2);
                    x1 = xe - dx;
                    x2 = xe + dx;
                    if (Math.Abs(x1) <= 1) roots++;
                    if (Math.Abs(x2) <= 1) roots++;
                    if (x1 < -1) x1 = x2;
                }

                if (roots == 1)
                {
                    if (h0 < 0) rise = (double)i + x1;
                    else set = i + x1;

                }
                else if (roots == 2)
                {
                    rise = i + (ye < 0 ? x2 : x1);
                    set = i + (ye < 0 ? x1 : x2);
                }

                if (rise != 0.0 && set != 0.0) break;

                h0 = h2;
            }

            var result = new moonRiseSet();

            if (rise != 0.0) result.riseTime = hoursLater(t, (int)rise);
            if (set != 0) result.settime = hoursLater(t, (int)set);
            if (rise == 0.0 && set == 0.0)
                result.state = ye > 0 ? "alwaysUp" : "alwaysDown";

            return result;

        }
    }
}
