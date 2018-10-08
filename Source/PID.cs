﻿using DishControl;
using System;
using System.Threading;

namespace PIDLibrary
{
    public class PID
    {

        #region Fields

        //Gains
        private double kp;
        private double ki;
        private double kd;

        private double gain;

        //Running Values
        //private DateTime lastUpdate;
        private long lastUpdate = 0;
        private double lastPV;
        private double errSum;

        //Reading/Writing Values
        private GetDouble readPV;
        private GetDouble readSP;
        private SetDouble writeOV;

        //Max/Min Calculation
        private double pvMax;
        private double pvMin;
        private double outMax;
        private double outMin;

        //Threading and Timing
        private double computeHz = 20.0f;
        private Thread runThread;

        private bool motionComplete = false;
        #endregion

        #region Properties

        public double PGain
        {
            get { return kp; }
            set { kp = value; }
        }

        public double IGain
        {
            get { return ki; }
            set { ki = value; }
        }

        public double DGain
        {
            get { return kd; }
            set { kd = value; }
        }

        public double Gain
        {
            get { return gain; }
            set { gain = value; }
        }

        public double PVMin
        {
            get { return pvMin; }
            set { pvMin = value; }
        }

        public double PVMax
        {
            get { return pvMax; }
            set { pvMax = value; }
        }

        public double OutMin
        {
            get { return outMin; }
            set { outMin = value; }
        }

        public double OutMax
        {
            get { return outMax; }
            set { outMax = value; }
        }

        public bool PIDOK
        {
            get { return runThread != null; }
        }

        public bool Complete
        {
            get { return motionComplete; }
        }

        public double resolution
        {
            get; set;
        }

        public bool inMotion { get; set; }
        #endregion

        #region Construction / Deconstruction

        public PID(double pG, double iG, double dG, double g,
            double pMax, double pMin, double oMax, double oMin,
            GetDouble pvFunc, GetDouble spFunc, SetDouble outFunc)
        {
            kp = pG;
            ki = iG;
            kd = dG;
            gain = g;
            pvMax = pMax;
            pvMin = pMin;
            outMax = oMax;
            outMin = oMin;
            readPV = pvFunc;
            readSP = spFunc;
            writeOV = outFunc;
        }

        ~PID()
        {
            Disable();
            readPV = null;
            readSP = null;
            writeOV = null;
        }

        #endregion

        #region Public Methods

        public void Enable()
        {
            if (runThread != null)
                return;

            Reset();

            runThread = new Thread(new ThreadStart(Run));
            runThread.IsBackground = true;
            runThread.Priority = ThreadPriority.Highest;
            runThread.Name = "PID Processor";
            inMotion = true;
            runThread.Start();
        }

        public void Disable()
        {
            if (runThread == null)
                return;

            inMotion = true;
            runThread.Abort();
            runThread = null;
        }

        public void Reset()
        {
            errSum = 0.0f;
            lastUpdate = DateTime.Now.Ticks;
            motionComplete = false;
            inMotion = true;
        }

        #endregion

        #region Private Methods

        private double ScaleValue(double value, double valuemin, double valuemax, double scalemin, double scalemax)
        {
            double vPerc = (value - valuemin) / (valuemax - valuemin);
            double bigSpan = vPerc * (scalemax - scalemin);

            double retVal = scalemin + bigSpan;

            return retVal;
        }

        private double Clamp(double value, double min, double max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        private void Compute()
        {
            if (readPV == null || readSP == null || writeOV == null)
                return;

            double pv = readPV();
            double sp = readSP();

            //We need to scale the pv to +/- 100%, but first clamp it
            pv = Clamp(pv, pvMin, pvMax);
            pv = ScaleValue(pv, pvMin, pvMax, -10.0f, 10.0f);

            //We also need to scale the setpoint
            sp = Clamp(sp, pvMin, pvMax);
            sp = ScaleValue(sp, pvMin, pvMax, -10.0f, 10.0f);

            double res =  ScaleValue(resolution, pvMin, pvMax, 0.0f, 20.0f);
            //Now the error is in percent...
            double err = sp - pv;
            if (Math.Abs(err) <= res)
            {
                err = 0.0;
                motionComplete = true;
            }
            else
                motionComplete = false;

            double pTerm = err * kp;
            double iTerm = 0.0f;
            double dTerm = 0.0f;

            double partialSum = 0.0f;
            long nowTime = DateTime.Now.Ticks;

            if (lastUpdate != 0)
            {
                double dT = (nowTime - lastUpdate)/10000; //time in ms

                //Compute the integral if we have to...
                if (pv >= pvMin && pv <= pvMax)
                {
                    partialSum = errSum + dT * err;
                    iTerm = ki * partialSum;
                }

                if (dT != 0.0f)
                    dTerm = kd * (pv - lastPV) / dT;
            }

            lastUpdate = nowTime;
            errSum = partialSum;
            lastPV = pv;

            //Now we have to scale the output value to match the requested scale
            double outReal = (pTerm + iTerm + dTerm)*gain;

            //outReal = Clamp(outReal, -10.0f, 10.0f);
            //outReal = ScaleValue(outReal, -10.0f, 10.0f, outMin, outMax);

            //Write it out to the world
            outReal = Clamp(outReal, outMin, outMax);
            writeOV(outReal);
        }

        #endregion

        #region Threading

        private void Run()
        {

            while (true)
            {
                try
                {
                    int sleepTime = (int)(1000 / computeHz);
                    Thread.Sleep(sleepTime);
                    Compute();
                    if (motionComplete)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    BasicLog.writeLog(e.Message);
                }
            }

        }

        #endregion

    }
}
