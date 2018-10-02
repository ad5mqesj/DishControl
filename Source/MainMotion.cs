using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

using System.Windows.Forms;

#if _TEST
using InterfaceFake;
#else
using WinfordEthIO;
#endif

using PIDLibrary;
using System.Threading;

namespace DishControl
{
    public partial class MainForm : Form
    {
        Eth32 dev = null;
        Thread updateThread = null;
        Encoder azEncoder = null, elEncoder = null;
        double azCommand = -1.0, elCommand, RAcommand, decCommand;
        bool azForward = true;
        public double azPos = 0.0, elPos = 0.0;
        PID azPid = null;
        PID elPid = null;
        int outputPortNum = 2;
        int WatchdoLoopcount = 0;
        int currentIncrement = 0;

        //called to set up motion control sepcific objects: encoder reader class, and PID loops for each axis
        public void MotionSetup()
        {
            string resultString = Regex.Match(settings.outputPort, @"\d+").Value;
            Int32.TryParse(resultString, out outputPortNum);

            azEncoder = new Encoder(this.dev, this.settings, true);
            elEncoder = new Encoder(this.dev, this.settings, false);
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Tick += new EventHandler(TimerEventProcessor);
            mainTimer.Interval = 200;

            azPid = new PID(settings.azKp, settings.azKi, settings.azKd, 360.0, 0.0, settings.azOutMax, settings.azOutMin, this.azReadPosition, this.azSetpoint, this.setAz);
            azPid.resolution = (settings.azMax - settings.azMin) / (double)((1 << settings.AzimuthEncoderBits) - 1);

            elPid = new PID(settings.elKp, settings.elKi, settings.elKd, 360.0, 0.0, settings.elOutMax, settings.elOutMin, this.elReadPosition, this.elSetpoint, this.setEl);
            elPid.resolution = (settings.elMax - settings.elMin) / (double)((1 << settings.ElevationEncoderBits) - 1);

            azPos = settings.azPark;
            elPos = settings.elPark;
        }

        //called to establish network connection to Eth32 box
        private void Connect(bool bSameAddress = true)
        {
            if (settings.eth32Address.Length == 0)
            {
                MessageBox.Show("Please configure the eht32 in Options->Settings");
                return;
            }
            if (mainTimer != null)
                mainTimer.Stop();
            bRunTick = false;

            // If we're already connected, disconnect and reconnect
            if (dev.Connected && !bSameAddress)
            {
                enableDrive(false);
                Thread.Sleep(1000);
                dev.Disconnect();
                Thread.Sleep(1000);
                dev = null;
            }

            try
            {
                // Connect, use a 1 second timeout
                if (dev == null)
                    dev = new Eth32();
                if (!dev.Connected)
                    dev.Connect(settings.eth32Address, Eth32.DefaultPort, 1000);

                if (dev.Connected)
                {
                    state = DishState.Stopped;
                    dev.ResetDevice();

                    azEncoder.SetupEncoderPorts();
                    elEncoder.SetupEncoderPorts();
                    //setup output port
                    this.dev.SetDirection(this.outputPortNum, 0xff);
                    //watchdog output
                    this.dev.SetDirection(4, 1);
                    Thread.Sleep(100);
                    //set up initial state of output control bits
                    this.setControlBits(true, true, false);
                    this.setControlBits(false, true, false);

                    //setup PWM
                    this.dev.PwmClockState = Eth32PwmClock.Enabled;
                    this.dev.PwmBasePeriod = 199; //10 khz
                    this.setAz(0.0);
                    this.setEl(0.0);
                    this.enableDrive(true);
                    mainTimer.Start();
                    bRunTick = true;
                }
            }
            catch (Eth32Exception etherr)
            {
#if !_TEST
                MessageBox.Show("Error connecting to the ETH32: " + Eth32.ErrorString(etherr.ErrorCode));
#endif
            }
            updateThread.Start();
        }

        //called to set/unset main Drive enable bit
        private void enableDrive(bool enable)
        {
            int val = 0;
            //use same polarity as Az
            bool polarity = settings.azActiveHi;
            if (settings.driveEnablebit >= 0)
            {
                val = 1;
                val = polarity ? val : (val > 0 ? 0 : 1);
                if (dev.Connected)
                    this.dev.OutputBit(this.outputPortNum, settings.driveEnablebit, val);
                Thread.Sleep(50);
            }
        }

        //sinple 1 pole low pass filter
        private double LowPass(double oldVal, double curVal)
        {
            double output = settings.alpha * oldVal + (1.0 - settings.alpha) * curVal;
            return output;
        }

        //read and filter az position, filtered to reduce jitter near bit transitions
        public double azReadPosition()
        {
            double curval = 0.0;
            double retval;
            if (dev.Connected)
            {
                bUpdating = true;
                lock (this.dev)
                {
                    curval = azEncoder.countsToDegrees(azEncoder.readNormalizedEncoderBits());
                }
                this.azPos = LowPass(this.azPos, curval);
                if (bUpdating)
                    bUpdating = false;
            }
            retval = (this.azForward ? this.azPos : this.azPos - 360.0);
            return retval;
        }

        //read and filter el position, filtered to reduce jitter near bit transitions
        public double elReadPosition()
        {
            double curval = 0.0;
            double retval;
            if (dev.Connected)
            {
                bUpdating = true;
                lock (this.dev)
                {
                    curval = elEncoder.countsToDegrees(elEncoder.readNormalizedEncoderBits());
                }
                this.elPos = LowPass(this.elPos, curval);
                if (bUpdating)
                    bUpdating = false;
            }
            retval = this.elPos;
            return retval;
        }

        public double azSetpoint()
        {
            double dir = 1.0;
            double command = 0.0;
            double distanceF = Math.Abs(this.azPos - this.azCommand);
            double distanceR = Math.Abs(this.azPos + 360.0 - this.azCommand);
            double FLimitDist = Math.Abs(settings.azMax - distanceF);
            double RLimitDist = Math.Abs(settings.azMin + distanceR);

            //case when its 180 either way we should move away from nearest limit
            if (Math.Abs(distanceF - distanceR) < 1)
            {
                if (FLimitDist > RLimitDist || (RLimitDist > 0.0 && settings.azMin < 0.0))
                {
                    dir = 1.0;
                    azForward = true;
                }
                else
                {
                    azForward = false;
                    dir = -1.0;
                }
                command = distanceF * dir;
            }
            else
            {
                if (distanceF < distanceR && (this.azPos + distanceF) < settings.azMax)
                {
                    command = this.azCommand;
                    azForward = true;
                }
                else if (distanceR < distanceF && (this.azPos - distanceR) > settings.azMin)
                {
                    command = this.azCommand - 360.0;
                    azForward = false;
                }
            }
            return command;
        }
        private double elSetpoint()
        {
            return this.elCommand;
        }

        public void setAz(double azVel)
        {
            bool azDir = (azVel > 0.05);
            if (dev.Connected)
            {
                lock (this.dev)
                {
                    setControlBits(true, Math.Abs(azVel) < 0.05, azDir);
                    this.dev.SetPwmParameters(settings.azPWMchan, Eth32PwmChannel.Normal, 10000.0, Math.Abs(azVel));
                }
            }
        }
        public void setEl(double elVel)
        {
            if (dev.Connected)
            {
                lock (this.dev)
                {
                    setControlBits(false, Math.Abs(elVel) < 0.05, (elVel < -0.05));
                    this.dev.SetPwmParameters(settings.elPWMchan, Eth32PwmChannel.Normal, 10000.0, Math.Abs(elVel));
                }
            }
        }


    }
}
