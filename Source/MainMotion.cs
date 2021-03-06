﻿using System;
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
        public double azPos = -1.0, elPos = 0.0;
        PID azPid = null;
        PID elPid = null;
        int outputPortNum = 2;
        int WatchdoLoopcount = 0;
        int currentIncrement = 0;
        private delegate void TimerEventDel();

        //called to set up motion control sepcific objects: encoder reader class, and PID loops for each axis
        public void MotionSetup()
        {
            string resultString = Regex.Match(settings.outputPort, @"\d+").Value;
            Int32.TryParse(resultString, out outputPortNum);

            azEncoder = new Encoder(this.dev, this.settings, true);
            elEncoder = new Encoder(this.dev, this.settings, false);
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Tick += new EventHandler(TimerEventProcessor);
            mainTimer.Interval = 250;

            azPid = new PID(settings.azKp, settings.azKi, settings.azKd, settings.azG, 360.0, 0.0, settings.azOutMax, settings.azOutMin, this.azReadPosition, this.azSetpoint, this.setAz);
            azPid.resolution = (settings.azMax - settings.azMin) / (double)((1 << settings.AzimuthEncoderBits) - 1);

            elPid = new PID(settings.elKp, settings.elKi, settings.elKd, settings.elG, 360.0, 0.0, settings.elOutMax, settings.elOutMin, this.elReadPosition, this.elSetpoint, this.setEl);
            elPid.resolution = (settings.elMax - settings.elMin) / (double)((1 << settings.ElevationEncoderBits) - 1);

            azPos = settings.azPark;
            elPos = settings.elPark;

            //set up thread for watchdog
            this.updateThread = new Thread(updateThreadCallback);
            this.updateThread.Priority = ThreadPriority.Highest;
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
                    MotionSetup();

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
            if (!updateThread.IsAlive)
            {
                this.updateThread = new Thread(updateThreadCallback);
                this.updateThread.Priority = ThreadPriority.Highest;
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

        //compute shortest path to commanded az position
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

        //for el we just go
        private double elSetpoint()
        {
            return this.elCommand;
        }
        //send az command
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
        //send el command
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

        //set up relays for az/el motion
        private void setControlBits(bool isAz, bool stopped, bool dirCW)
        {
            int val = 0;
            int bit = 0;

            bool polarity = isAz ? settings.azActiveHi : settings.elActiveHi;
            val = polarity ? val : (val > 0 ? 0 : 1);


            if (stopped) //turn all bits "off"
            {
                val = 0;
                polarity = isAz ? settings.azActiveHi : settings.elActiveHi;
                val = polarity ? val : (val > 0 ? 0 : 1);

                bit = isAz ? settings.azEnable : settings.elEnable;
                if (bit >= 0 && bit <= 7)
                {
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                }

                bit = isAz ? settings.azCCWbit : settings.elCCWbit;
                this.dev.OutputBit(this.outputPortNum, bit, val);

                bit = isAz ? settings.azCWbit : settings.elCWbit;
                this.dev.OutputBit(this.outputPortNum, bit, val);
                //don't subsequently turn things on when not driving 
                return;
            }

            driveType mode = isAz ? settings.azDriveType : settings.elDriveType;
            switch (mode)
            {
                case driveType.Both:
                    //CW
                    bit = isAz ? settings.azCWbit : settings.elCWbit;
                    val = dirCW ? 1 : 0;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);

                    //CCW
                    bit = isAz ? settings.azCCWbit : settings.elCCWbit;
                    //polarity ALWAYS opposite of CW bit
                    val = val > 0 ? 0 : 1;
                    this.dev.OutputBit(this.outputPortNum, bit, val);

                    //enable
                    bit = isAz ? settings.azEnable : settings.elEnable;
                    val = stopped ? 0 : 1;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    if (bit >= 0 && bit <= 7)
                    {
                        this.dev.OutputBit(this.outputPortNum, bit, val);
                    }
                    break;

                case driveType.CCW:
                    //CW
                    bit = isAz ? settings.azCWbit : settings.elCWbit;
                    val = dirCW ? 1 : 0;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    //CCW
                    bit = isAz ? settings.azCCWbit : settings.elCCWbit;
                    //polarity ALWAYS opposite of CW bit
                    val = val > 0 ? 0 : 1;
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    break;

                case driveType.DirEnable:
                    //direction
                    bit = isAz ? settings.azCWbit : settings.elCWbit;
                    val = dirCW ? 1 : 0;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    //enable
                    bit = isAz ? settings.azEnable : settings.elEnable;
                    val = stopped ? 0 : 1;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    break;
            }

        }

        //this is Thread callback actual thread handler for the watchdog timer
        //similar to button thread
        private void updateThreadCallback()
        {
            while (btEnabled)
            {
                Thread.Sleep(TimerTickms);
                if (!btEnabled)
                    return;
                if (bRunTick && this.IsHandleCreated)
                    BeginInvoke(new TimerEventDel(timer_Tick));
            }
        }

        //actual watchdog pulse generator
        private void timer_Tick()
        {
            WatchdoLoopcount++;
            if (WatchdoLoopcount > 19)
                WatchdoLoopcount = 0;
            int bitState = (WatchdoLoopcount) % 5 > 0 ? 1 : 0; //1:5 duty cycle
            lock (this.dev)
            {
                this.dev.OutputBit(4, 0, bitState);
            }

            if (WatchdoLoopcount == 0)
            {
                if (dev.Connected && state == DishState.Stopped)
                {
                    if (azCommand == -1.0)
                    {
                        //initialize old value in lowpass filter so 
                        //it doesn't take too long to settle on startup
                        azPos = azEncoder.countsToDegrees(0);
                        elPos = elEncoder.countsToDegrees(0);
                    }
                    azPos = azReadPosition();
                    elPos = elReadPosition();
                    if (azCommand == -1.0)
                    {
                        azCommand = azPos;
                        elCommand = elPos;
                        GeoAngle mAzAngle = GeoAngle.FromDouble(azCommand, true);
                        GeoAngle mElAngle = GeoAngle.FromDouble(elCommand);
                        this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
                        this.commandEl.Text = string.Format("{2}{0} : {1}", mElAngle.Degrees, mElAngle.Minutes, mElAngle.IsNegative ? "-" : "");
                    }
                }
            }

            if (state != DishState.Stopped && state != DishState.Unknown)
            {
                if (azPid.Complete && elPid.Complete)
                {
                    this.Stop();
                }
            }

        }


    }
}
