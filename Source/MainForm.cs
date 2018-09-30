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
    public enum buttonDir
    {
        East,
        West,
        North,
        South
    };
    public partial class MainForm : Form
    {
        Eth32 dev = null;
        configModel settings;
        bool appConfigured = false;
        DishState state = DishState.Unknown;
        System.Windows.Forms.Timer mainTimer = null;
        Thread updateThread = null;
        bool bRunTick = false;
        Encoder azEncoder = null, elEncoder = null;
        bool trackCelestial = false;
        bool trackMoon = false;
        double azCommand = -1.0, elCommand, RAcommand, decCommand;
        bool azForward = true;
        public double azPos = 0.0, elPos = 0.0;
        PID azPid = null;
        PID elPid = null;
        int outputPortNum = 2;
        bool bUpdating = false;
        DateTime messageTime = DateTime.Now;
        int WatchdoLoopcount = 0;
        Thread buttonThread;
        ManualResetEvent jogEvent;
        bool btEnabled = true;
        int currentIncrement = 0;
        buttonDir direction;
        List<presets> Presets = null;
        int TimerTickms = 10;

        public MainForm(Eth32 dev)
        {
            InitializeComponent();
            this.dev = dev;
            string configFile = Application.StartupPath + "\\dishConfig.xml";
            jogEvent = new ManualResetEvent(false);
            this.buttonThread = new Thread(buttonThreadHandler);
            this.buttonThread.Start();
            this.updateThread = new Thread(updateThreadCallback);
            this.updateThread.Priority = ThreadPriority.AboveNormal;

            if (File.Exists(configFile))
            {
                settings = configFileHandler.readConfig(configFile);
                appConfigured = true;
            }
            else
            {
                settings = new configModel();
                Config cfgDialog = new Config(dev, this.settings);
                if (cfgDialog.ShowDialog() == DialogResult.OK)
                {
                    configFileHandler.writeConfig(configFile, cfgDialog.settings);
                    MessageBox.Show("Saved");
                    appConfigured = true;
                }
            }
            Presets = new List<presets>();
            presets none = new presets()
            {
                Value = 0,
                Text = "None",
                Az = 0.0,
                El = 0.0
            };
            Presets.Add(none);
            presets ps1 = new presets()
            {
                Value = 1,
                Text = settings.Preset1Name,
                Az = settings.Preset1Az,
                El = settings.Preset1El
            };
            Presets.Add(ps1);
            presets ps2 = new presets()
            {
                Value = 2,
                Text = settings.Preset2Name,
                Az = settings.Preset2Az,
                El = settings.Preset2El
            };
            Presets.Add(ps2);
            presets ps3 = new presets()
            {
                Value = 3,
                Text = settings.Preset3Name,
                Az = settings.Preset3Az,
                El = settings.Preset3El
            };
            Presets.Add(ps3);
            presets ps4 = new presets()
            {
                Value = 4,
                Text = settings.Preset4Name,
                Az = settings.Preset4Az,
                El = settings.Preset4El
            };
            Presets.Add(ps4);
            presets ps5 = new presets()
            {
                Value = 5,
                Text = settings.Preset5Name,
                Az = settings.Preset5Az,
                El = settings.Preset5El
            };
            Presets.Add(ps5);

            if (appConfigured)
            {
                foreach (presets p in Presets)
                {
                    if (!String.IsNullOrEmpty(p.Text))
                    {
                        presetSelector.Items.Add(p.Text);
                    }
                }
                presetSelector.SelectedIndex = 0;

                string resultString = Regex.Match(settings.outputPort, @"\d+").Value;
                Int32.TryParse(resultString, out outputPortNum);
                state = DishState.Unknown;
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

                RollingLogger.setupRollingLogger(settings.positionFileLog, settings.maxPosLogSizeBytes, settings.maxPosLogFiles);
            }
        }

        private void buttonThreadHandler()
        {
            while (btEnabled)
            {
                jogEvent.WaitOne();
                if (!btEnabled)
                    return;

                double curvel = this.settings.jogIncrement;
                double dirMul = 1.0;
                if (this.direction == buttonDir.South || this.direction == buttonDir.East)
                    dirMul = -1.0;
                state = DishState.Moving;
                updateStatus();
                if (this.direction == buttonDir.South || this.direction == buttonDir.North)
                {
                    Thread.Sleep(100);
                    this.setEl(curvel * dirMul);
                    Thread.Sleep(500);
                    this.setEl(0.0);
                }
                else
                {
                    Thread.Sleep(100);
                    this.setAz(curvel * dirMul);
                    Thread.Sleep(500);
                    this.setAz(0.0);
                }
                state = DishState.Stopped;
                updateStatus();
                jogEvent.Reset();
            }
        }
        private void enableControls()
        {
            lunarTrack.Enabled = dev.Connected;

            bool bEnabled = dev.Connected && !trackMoon && state != DishState.Moving;

            commandAz.Enabled = bEnabled;
            commandEl.Enabled = bEnabled;
            commandRA.Enabled = bEnabled;
            commandDec.Enabled = bEnabled;
            track.Enabled = bEnabled;

            if (trackCelestial)
            {
                trackMoon = false;
                lunarTrack.Enabled = false;
                commandAz.Enabled = false;
                commandEl.Enabled = false;
            }
            else
            {
                commandRA.Enabled = false;
                commandDec.Enabled = false;
            }

            Park.Enabled = (state == DishState.Stopped);
            GO.Enabled = (state == DishState.Stopped);

            STOP.Enabled = (state != DishState.Stopped && state != DishState.Unknown);
        }
        private void updateStatus()
        {
            if (dev.Connected)
            {
                connectedIcon.BackColor = Color.Green;
                connect.Enabled = false;
            }
            else
            {
                connectedIcon.BackColor = Color.Red;
                connect.Enabled = true;
            }

            movingIcon.BackColor = (state == DishState.Unknown) ? Color.Yellow : (state == DishState.Stopped ? Color.Green : Color.Red);
            trackingIcon.BackColor = (state == DishState.Tracking) ? Color.Blue : Color.White;
        }

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

        private void connect_Click(object sender, EventArgs e)
        {
            this.Connect();
        }

        private double LowPass(double oldVal, double curVal)
        {
            double output = settings.alpha * oldVal + (1.0 - settings.alpha) * curVal;
            return output;
        }

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

        private void updatePosition()
        {
            if ((DateTime.Now - messageTime).TotalSeconds > 30)
            {
                Message.Text = "";
            }
            if (dev.Connected && state == DishState.Stopped)
            {
                azPos = azEncoder.countsToDegrees(azEncoder.readNormalizedEncoderBits());
                elPos = elEncoder.countsToDegrees(elEncoder.readNormalizedEncoderBits());
            }

            GeoAngle AzAngle = GeoAngle.FromDouble(azPos, true);
            GeoAngle ElAngle = GeoAngle.FromDouble(elPos);

            RaDec astro = celestialConversion.CalcualteRaDec(elPos, azPos, settings.latitude, settings.longitude);
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

            string posLog = string.Format("RA {0:D3} : {1:D2}\t DEC {2:D3} : {3:D2}", RA.Degrees, RA.Minutes, Dec.Degrees, Dec.Minutes);
            currentIncrement++;
            if (currentIncrement % 9 == 0)
            {
                RollingLogger.LogMessage(posLog);
                currentIncrement = 0;
            }
            this.Azimuth.Text = string.Format("{0:D3} : {1:D2}", AzAngle.Degrees, AzAngle.Minutes);
            this.Elevation.Text = string.Format("{0:D2} : {1:D2}", ElAngle.Degrees, ElAngle.Minutes);

            if (azCommand == -1.0)
            {
                this.commandAz.Text = this.Azimuth.Text;
                this.commandEl.Text = this.Elevation.Text;
                azCommand = azPos;
                elCommand = elPos;
            }


            this.RA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
            this.DEC.Text = string.Format("{0:D2} : {1:D2}", Dec.Degrees, Dec.Minutes);

            if (trackCelestial)
            {
                AltAz aa = celestialConversion.CalculateAltAz(RAcommand, decCommand, settings.latitude, settings.longitude);
                if (aa.Alt < 0)
                {
                    Message.Text = "Requested position is below horizon!";
                    trackMoon = false;
                    messageTime = DateTime.Now;
                    trackCelestial = false;
                }
                GeoAngle cAzAngle = GeoAngle.FromDouble(aa.Az, true);
                GeoAngle cElAngle = GeoAngle.FromDouble(aa.Alt);
                azCommand = aa.Az;
                elCommand = aa.Alt;
                this.commandAz.Text = string.Format("{0:D3} : {1:D2}", cAzAngle.Degrees, cAzAngle.Minutes);
                this.commandEl.Text = string.Format("{0:D2} : {1:D2}", cElAngle.Degrees, cElAngle.Minutes);

                if (state != DishState.Moving && state != DishState.Tracking && aa.Alt > 1.0)
                    this.Go();
            }
            if (trackMoon)
            {
                SunCalc sc = new SunCalc();
                long eDate = sc.epochDate(DateTime.UtcNow);
                double jdate = sc.toJulian(eDate);
                AltAz aa = sc.getMoonPosition(eDate, settings.latitude, settings.longitude);
                if (aa.Alt < 0)
                {
                    Message.Text = "Moon is below horizon!";
                    trackMoon = false;
                    messageTime = DateTime.Now;
                }
                moonRiseSet mrs = sc.getMoonTimes(eDate, settings.latitude, settings.longitude, true);
                azCommand = aa.Az;
                elCommand = aa.Alt;
                GeoAngle mAzAngle = GeoAngle.FromDouble(aa.Az, true);
                GeoAngle mElAngle = GeoAngle.FromDouble(aa.Alt);
                this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
                this.commandEl.Text = string.Format("{2}{0} : {1}", mElAngle.Degrees, mElAngle.Minutes, mElAngle.IsNegative ? "-" : "");

                double Alt = elCommand;
                if (Alt > 90.0) Alt = Alt - 90.0;
                RaDec mastro = celestialConversion.CalcualteRaDec(Alt, azCommand, settings.latitude, settings.longitude);
                GeoAngle mDec = GeoAngle.FromDouble(mastro.Dec);
                GeoAngle mRA = GeoAngle.FromDouble(mastro.RA, true);

                this.commandRA.Text = string.Format("{0:D3} : {1:D2}", mRA.Degrees, mRA.Minutes);
                this.commandDec.Text = string.Format("{0:D2} : {1:D2}", mDec.Degrees, mDec.Minutes);
                if (state != DishState.Moving && state != DishState.Tracking && aa.Alt > 1.0)
                    this.Go();
            }//trackMoon
            if (state != DishState.Stopped && state != DishState.Unknown)
            {
                if (azPid.Complete && elPid.Complete)
                {
                    this.Stop();
                }
            }
            if (TimerTickms == 1000)
            {
                TimerTickms = 10;
            }
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            updatePosition();
            updateStatus();
        }

        private delegate void TimerEventDel();
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
        private void timer_Tick()
        {
            WatchdoLoopcount++;
            if (WatchdoLoopcount > 49)
                WatchdoLoopcount = 0;
            int bitState = (WatchdoLoopcount) % 5 > 0 ? 1 : 0; //1:5 duty cycle
            lock (this.dev)
            {
                this.dev.OutputBit(4, 0, bitState);
            }
            //if (WatchdoLoopcount % 25 == 0) // 1/5th rate 
            //{
            //    updatePosition();
            //    updateStatus();
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dev != null)
            {
                this.enableDrive(false);
                if (mainTimer != null)
                    mainTimer.Stop();
                bRunTick = false;
                btEnabled = false;
                jogEvent.Set();

                // dev is at least instantiated
                if (dev.Connected)
                {
                    dev.Disconnect();
                }
            }

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string startingIP = settings.eth32Address;
            mainTimer.Stop();
            Config cfgDialog = new Config(dev, this.settings);
            if (cfgDialog.ShowDialog() == DialogResult.OK)
            {
                string configFile = Application.StartupPath + "\\dishConfig.xml";
                configFileHandler.writeConfig(configFile, cfgDialog.settings);
                RollingLogger.setupRollingLogger(settings.positionFileLog, settings.maxPosLogSizeBytes, settings.maxPosLogFiles);
                MessageBox.Show("Saved");
                appConfigured = true;
            }
            if (appConfigured)
            {
                string resultString = Regex.Match(settings.outputPort, @"\d+").Value;
                Int32.TryParse(resultString, out outputPortNum);


                azEncoder = new Encoder(this.dev, this.settings, true);
                elEncoder = new Encoder(this.dev, this.settings, false);

                Connect(startingIP.Equals(settings.eth32Address));
            }
        }

        private void track_CheckedChanged(object sender, EventArgs e)
        {
            trackCelestial = track.Checked;
        }

        private void commandRA_TextChanged(object sender, EventArgs e)
        {
            RAcommand = GeoAngle.FromString(commandRA.Text).ToDouble();
        }

        private void commandDec_TextChanged(object sender, EventArgs e)
        {
            decCommand = GeoAngle.FromString(commandDec.Text).ToDouble();
        }

        private void commandEl_TextChanged(object sender, EventArgs e)
        {
            elCommand = GeoAngle.FromString(commandEl.Text).ToDouble();
        }

        private void commandAz_TextChanged(object sender, EventArgs e)
        {
            azCommand = GeoAngle.FromString(commandAz.Text).ToDouble();
        }

        private void lunarTrack_Click(object sender, EventArgs e)
        {
            trackMoon = !trackMoon;
        }

        private void GO_Click(object sender, EventArgs e)
        {
            this.Go();
        }

        public void Go()
        {
            azPid.Enable();
            elPid.Enable();
            if (!trackCelestial && !trackMoon)
                this.state = DishState.Moving;
            else
                this.state = DishState.Tracking;
        }

        private void Park_Click(object sender, EventArgs e)
        {
            azCommand = settings.azPark;
            elCommand = settings.elPark;
            GeoAngle mAzAngle = GeoAngle.FromDouble(azCommand, true);
            GeoAngle mElAngle = GeoAngle.FromDouble(elCommand);
            this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
            this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);

            RaDec astro = celestialConversion.CalcualteRaDec(mElAngle.ToDouble(), mAzAngle.ToDouble(), settings.latitude, settings.longitude);
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

            this.commandRA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
            this.commandDec.Text = string.Format("{0:D2} : {1:D2}", Dec.Degrees, Dec.Minutes);

            this.state = DishState.Parking; ;

            azPid.Enable();
            elPid.Enable();
        }

        private void STOP_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void driveTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testDrive testDialog = new testDrive(dev, this.settings, this);
            testDialog.ShowDialog();

        }

        private void presetSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            int item = presetSelector.SelectedIndex;
            if (item != 0)
            {
                this.azCommand = this.Presets[item].Az;
                this.elCommand = this.Presets[item].El;
                GeoAngle mAzAngle = GeoAngle.FromDouble(azCommand, true);
                GeoAngle mElAngle = GeoAngle.FromDouble(elCommand);
                this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
                this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);

                RaDec astro = celestialConversion.CalcualteRaDec(mElAngle.ToDouble(), mAzAngle.ToDouble(), settings.latitude, settings.longitude);
                GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
                GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

                this.commandRA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
                this.commandDec.Text = string.Format("{0:D2} : {1:D2}", Dec.Degrees, Dec.Minutes);

            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Connect();
            updateStatus();
            updatePosition();
        }

        private void Stop()
        {
            azPid.Disable();
            elPid.Disable();
            this.setAz(0.0);
            this.setEl(0.0);
            this.state = DishState.Stopped;
            bUpdating = false;
        }
        private void Up_Click(object sender, EventArgs e)
        {
            this.direction = buttonDir.North;
            this.jogEvent.Set();
        }

        private void Down_Click(object sender, EventArgs e)
        {
            this.direction = buttonDir.South;
            this.jogEvent.Set();
        }

        private void CW_Click(object sender, EventArgs e)
        {
            this.direction = buttonDir.East;
            this.jogEvent.Set();
        }

        private void CCW_Click(object sender, EventArgs e)
        {
            this.direction = buttonDir.West;
            this.jogEvent.Set();
        }

    }
}
