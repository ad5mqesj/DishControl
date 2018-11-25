using System;
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
    public class MainMotion
    {
        Eth32 dev = null;
        Thread updateThread = null;
        Thread moveEventThread = null;
        Thread configEventThread = null;
        Encoder azEncoder = null, elEncoder = null;
        double azCommand = -1.0, elCommand;
        bool azForward = true;
        public double azPos = -1.0, elPos = 0.0;
        PID azPid = null;
        PID elPid = null;
        int outputPortNum = 2;
        int WatchdoLoopcount = 0;
        int TimerTickms = 5;

        public MainMotion(Eth32 dev)
        {
            this.dev = dev;
            this.moveEventThread = new Thread(moveEventHandler);
            this.moveEventThread.Start();
            this.configEventThread = new Thread(configEventHandler);
            this.configEventThread.Start();
        }

        //called to set up motion control sepcific objects: encoder reader class, and PID loops for each axis
        public void MotionSetup()
        {
            string resultString = Regex.Match(Program.settings.outputPort, @"\d+").Value;
            Int32.TryParse(resultString, out outputPortNum);

            azEncoder = new Encoder(this.dev, Program.settings, true);
            elEncoder = new Encoder(this.dev, Program.settings, false);
            //mainTimer = new System.Windows.Forms.Timer();
            //mainTimer.Tick += new EventHandler(TimerEventProcessor);
            //mainTimer.Interval = 250;

            azPid = new PID(Program.settings.azKp, Program.settings.azKi, Program.settings.azKd, Program.settings.azG, 360.0, 0.0, Program.settings.azOutMax, Program.settings.azOutMin, this.azReadPosition, this.azSetpoint, this.setAz);
            azPid.resolution = (Program.settings.azMax - Program.settings.azMin) / (double)((1 << Program.settings.AzimuthEncoderBits) - 1);

            elPid = new PID(Program.settings.elKp, Program.settings.elKi, Program.settings.elKd, Program.settings.elG, 360.0, 0.0, Program.settings.elOutMax, Program.settings.elOutMin, this.elReadPosition, this.elSetpoint, this.setEl);
            elPid.resolution = (Program.settings.elMax - Program.settings.elMin) / (double)((1 << Program.settings.ElevationEncoderBits) - 1);

            azPos = Program.settings.azPark;
            elPos = Program.settings.elPark;

            //set up thread for watchdog
            this.updateThread = new Thread(updateThreadCallback);
            this.updateThread.Priority = ThreadPriority.Highest;
        }

        //waits for signal to indicate we have a (new) config and should (re-)connect
        private void configEventHandler()
        {
            while (Program.state.btEnabled) //while not exiting
            {
                Program.state.connectEvent.WaitOne();
                if (!Program.state.btEnabled)           //bail if exit signal is set
                    return;
                if (Program.state.appConfigured)
                    Connect();
                else
                {
                    if (dev.Connected)
                        dev.Disconnect();
                }
                Program.state.connectEvent.Reset();
            }
        }

        //waits for signal from UI that it wants to command a motion
        private void moveEventHandler()
        {
            while (Program.state.btEnabled)
            {
                Program.state.go.WaitOne();
                if (!Program.state.btEnabled) //bail if exit signal is set
                    return;
                switch (Program.state.command)
                {
                    case CommandType.Jog:
                        Program.state.state = DishState.Moving;
                        setAz(Program.state.commandAzimuthRate);
                        setEl(Program.state.commandElevationRate);
                        break;

                    case CommandType.Move:
                        Program.state.state = DishState.Moving;
                        if (Program.state.raDecCommand)
                        {
                            computeAzElCommandFromRaDec();
                        }
                        lock (Program.state)
                        {
                            azCommand = Program.state.commandAzimuth;
                            elCommand = Program.state.commandElevation;
                        }
                        azPid.Enable();
                        elPid.Enable();
                        break;
                    case CommandType.Stop:
                        Program.state.commandAzimuthRate = 0.0;
                        Program.state.commandElevationRate = 0.0;
                        this.Stop();
                        break;
                    case CommandType.Track:
                        Program.state.state = DishState.Tracking;
                        break;
                }
                Program.state.go.Reset();
            }
        }

        private void computeAzElCommandFromRaDec()
        {
            AltAz aa = celestialConversion.CalculateAltAz(Program.state.commandRightAscension, Program.state.commandDeclination, Program.settings.latitude, Program.settings.longitude);
            if (aa.Alt < 0)
            {
                Program.state.errorMessage = "Target is below horizon!";
                Program.state.trackCelestial = false;
                Program.state.trackMoon = false;
            }
            Program.state.commandAzimuth = aa.Az;
            Program.state.commandElevation = aa.Alt;
        }

        //called to establish network connection to Eth32 box
        private void Connect(bool bSameAddress = true)
        {
            if (Program.settings.eth32Address.Length == 0)
            {
                MessageBox.Show("Please configure the eht32 in Options->Program.settings");
                return;
            }

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
                    dev.Connect(Program.settings.eth32Address, Eth32.DefaultPort, 1000);

                if (dev.Connected)
                {
                    Program.state.state = DishState.Stopped;
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
            bool polarity = Program.settings.azActiveHi;
            if (Program.settings.driveEnablebit >= 0)
            {
                val = 1;
                val = polarity ? val : (val > 0 ? 0 : 1);
                if (dev.Connected)
                    this.dev.OutputBit(this.outputPortNum, Program.settings.driveEnablebit, val);
                Thread.Sleep(50);
            }
        }

        //sinple 1 pole low pass filter
        private double LowPass(double oldVal, double curVal)
        {
            double output = Program.settings.alpha * oldVal + (1.0 - Program.settings.alpha) * curVal;
            return output;
        }

        //read and filter az position, filtered to reduce jitter near bit transitions
        public double azReadPosition()
        {
            double curval = 0.0;
            double retval;
            if (dev.Connected)
            {
                lock (this.dev)
                {
                    curval = azEncoder.countsToDegrees(azEncoder.readNormalizedEncoderBits());
                }
                this.azPos = LowPass(this.azPos, curval);
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
                lock (this.dev)
                {
                    curval = elEncoder.countsToDegrees(elEncoder.readNormalizedEncoderBits());
                }
                this.elPos = LowPass(this.elPos, curval);
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
            double FLimitDist = Math.Abs(Program.settings.azMax - distanceF);
            double RLimitDist = Math.Abs(Program.settings.azMin + distanceR);

            //case when its 180 either way we should move away from nearest limit
            if (Math.Abs(distanceF - distanceR) < 1)
            {
                if (FLimitDist > RLimitDist || (RLimitDist > 0.0 && Program.settings.azMin < 0.0))
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
                if (distanceF < distanceR && (this.azPos + distanceF) < Program.settings.azMax)
                {
                    command = this.azCommand;
                    azForward = true;
                }
                else if (distanceR < distanceF && (this.azPos - distanceR) > Program.settings.azMin)
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
                    this.dev.SetPwmParameters(Program.settings.azPWMchan, Eth32PwmChannel.Normal, 10000.0, Math.Abs(azVel));
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
                    this.dev.SetPwmParameters(Program.settings.elPWMchan, Eth32PwmChannel.Normal, 10000.0, Math.Abs(elVel));
                }
            }
        }

        //set up relays for az/el motion
        private void setControlBits(bool isAz, bool stopped, bool dirCW)
        {
            int val = 0;
            int bit = 0;

            bool polarity = isAz ? Program.settings.azActiveHi : Program.settings.elActiveHi;
            val = polarity ? val : (val > 0 ? 0 : 1);


            if (stopped) //turn all bits "off"
            {
                val = 0;
                polarity = isAz ? Program.settings.azActiveHi : Program.settings.elActiveHi;
                val = polarity ? val : (val > 0 ? 0 : 1);

                bit = isAz ? Program.settings.azEnable : Program.settings.elEnable;
                if (bit >= 0 && bit <= 7)
                {
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                }

                bit = isAz ? Program.settings.azCCWbit : Program.settings.elCCWbit;
                this.dev.OutputBit(this.outputPortNum, bit, val);

                bit = isAz ? Program.settings.azCWbit : Program.settings.elCWbit;
                this.dev.OutputBit(this.outputPortNum, bit, val);
                //don't subsequently turn things on when not driving 
                return;
            }

            driveType mode = isAz ? Program.settings.azDriveType : Program.settings.elDriveType;
            switch (mode)
            {
                case driveType.Both:
                    //CW
                    bit = isAz ? Program.settings.azCWbit : Program.settings.elCWbit;
                    val = dirCW ? 1 : 0;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);

                    //CCW
                    bit = isAz ? Program.settings.azCCWbit : Program.settings.elCCWbit;
                    //polarity ALWAYS opposite of CW bit
                    val = val > 0 ? 0 : 1;
                    this.dev.OutputBit(this.outputPortNum, bit, val);

                    //enable
                    bit = isAz ? Program.settings.azEnable : Program.settings.elEnable;
                    val = stopped ? 0 : 1;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    if (bit >= 0 && bit <= 7)
                    {
                        this.dev.OutputBit(this.outputPortNum, bit, val);
                    }
                    break;

                case driveType.CCW:
                    //CW
                    bit = isAz ? Program.settings.azCWbit : Program.settings.elCWbit;
                    val = dirCW ? 1 : 0;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    //CCW
                    bit = isAz ? Program.settings.azCCWbit : Program.settings.elCCWbit;
                    //polarity ALWAYS opposite of CW bit
                    val = val > 0 ? 0 : 1;
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    break;

                case driveType.DirEnable:
                    //direction
                    bit = isAz ? Program.settings.azCWbit : Program.settings.elCWbit;
                    val = dirCW ? 1 : 0;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    //enable
                    bit = isAz ? Program.settings.azEnable : Program.settings.elEnable;
                    val = stopped ? 0 : 1;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, bit, val);
                    break;
            }

        }

        private void Stop()
        {
            azPid.Disable();
            this.setAz(0.0);
            elPid.Disable();
            this.setEl(0.0);
            Program.state.state = DishState.Stopped;
        }

        //this is Thread callback actual thread handler for the watchdog timer
        //similar to button thread
        private void updateThreadCallback()
        {
            while (Program.state.btEnabled)
            {
                Thread.Sleep(TimerTickms);
                if (!Program.state.btEnabled || !dev.Connected)
                    return;
                timer_Tick();
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
                if (dev.Connected)
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
                    if (Program.state.state == DishState.Stopped && azCommand == -1.0)
                    {
                        azCommand = azPos;
                        elCommand = elPos;
                        Program.state.commandAzimuth = azCommand;
                        Program.state.commandElevation = elCommand;
                    }
                }
                //                azPos = azReadPosition();
                //                elPos = elReadPosition();
                lock (Program.state)
                {
                    Program.state.azimuth = azPos;
                    Program.state.elevation = elPos;
                }
                if (Program.state.command == CommandType.Track)
                {
                    if (Program.state.trackCelestial)
                        trackCelestialUpdate();
                    else
                        trackMoonUpdate();
                    if (Program.state.trackCelestial || Program.state.trackMoon)
                    {
                        azPid.Enable();
                        elPid.Enable();
                    }
                }
            }

            if (Program.state.state != DishState.Stopped && Program.state.state != DishState.Unknown)
            {
                if (azPid.Complete && elPid.Complete && Program.state.state != DishState.Tracking)
                {
                    this.Stop();
                }
            }
        }

        private void trackCelestialUpdate()
        {
            computeAzElCommandFromRaDec();
            if (Program.state.trackCelestial)
            {
                lock (Program.state)
                {
                    azCommand = Program.state.commandAzimuth;
                    elCommand = Program.state.commandElevation;
                }
            }
            else //below HORIZON
                this.Stop();
        }

        private void trackMoonUpdate()
        {
            SunCalc sc = new SunCalc();
            long eDate = sc.epochDate(DateTime.UtcNow);
            double jdate = sc.toJulian(eDate);
            AltAz aa = sc.getMoonPosition(eDate, Program.settings.latitude, Program.settings.longitude);
            moonRiseSet mrs = sc.getMoonTimes(eDate, Program.settings.latitude, Program.settings.longitude, true);
            if (aa.Alt < 0.0)
            {
                Program.state.errorMessage = "Moon is below horizon!";
                Program.state.trackCelestial = false;
                Program.state.trackMoon = false;
                this.Stop();
                return;
            }
            Program.state.commandAzimuth = aa.Az;
            Program.state.commandElevation = aa.Alt;
            lock (Program.state)
            {
                azCommand = Program.state.commandAzimuth;
                elCommand = Program.state.commandElevation;
            }

        }

    }
}
