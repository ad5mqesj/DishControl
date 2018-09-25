﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Windows.Forms;
using WinfordEthIO;
using PIDLibrary;
using System.Threading;

namespace DishControl
{
    public partial class MainForm : Form
    {
        Eth32 dev = null;
        configModel settings;
        bool appConfigured = false;
        DishState state = DishState.Unknown;
        System.Windows.Forms.Timer timer = null;
        System.Windows.Forms.Timer watchdog = null;
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

        public MainForm(Eth32 dev)
        {
            InitializeComponent();
            this.dev = dev;
            string configFile = Application.StartupPath + "\\dishConfig.xml";

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
            if (appConfigured)
            {
                string resultString = Regex.Match(settings.outputPort, @"\d+").Value;
                Int32.TryParse(resultString, out outputPortNum);
                state = DishState.Unknown;
                azEncoder = new Encoder(this.dev, this.settings, true);
                elEncoder = new Encoder(this.dev, this.settings, false);
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 20;
                timer.Enabled = true;
                timer.Tick += new EventHandler(timer1_Tick);

                watchdog = new System.Windows.Forms.Timer();
                watchdog.Interval = 10;
                watchdog.Enabled = true;
                watchdog.Tick += new EventHandler(watchdog_Tick);

                azPid = new PID(settings.azKp, settings.azKi, settings.azKd, settings.azMax, settings.azMin, settings.azOutMax, settings.azOutMin, this.azReadPosition, this.azSetpoint, this.setAz);
                azPid.resolution = (settings.azMax - settings.azMin) / (double)((1 << settings.AzimuthEncoderBits) - 1);

                elPid = new PID(settings.elKp, settings.elKi, settings.elKd, settings.elMax, settings.elMin, settings.elOutMax, settings.elOutMin, this.elReadPosition, this.elSetpoint, this.setEl);
                elPid.resolution = (settings.elMax - settings.elMin) / (double)((1 << settings.ElevationEncoderBits) - 1);

                azPos = settings.azPark;
                elPos = settings.elPark;
                Connect();
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
            if (timer != null && timer.Enabled)
                timer.Stop();
            if (watchdog != null && watchdog.Enabled)
                watchdog.Stop();
            // If we're already connected, disconnect and reconnect
            if (dev.Connected && !bSameAddress)
            {
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
                    //setup PWM
                    this.dev.SetDirection(this.outputPortNum, 0xff);
                    //watchdog output
                    this.dev.SetDirection(4, 1);

                    //set up initial state of output control bits
                    this.setControlBits(true, true, false);
                    this.setControlBits(false, true, false);

                    this.dev.PwmClockState = Eth32PwmClock.Enabled;
                    this.dev.PwmBasePeriod = 199; //10 khz
                    this.setAz(0.0);
                    this.setEl(0.0);
                }
                timer.Start();
                watchdog.Start();
            }
            catch (Eth32Exception etherr)
            {
                MessageBox.Show("Error connecting to the ETH32: " + Eth32.ErrorString(etherr.ErrorCode));
            }
        }

        private void connect_Click(object sender, EventArgs e)
        {
            this.Connect();
        }

        private double LowPass (double oldVal, double curVal)
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
                curval = azEncoder.countsToDegrees(azEncoder.readNormalizedEncoderBits());
                if (curval > settings.azMax)
                    curval = settings.azMax;
                else if (curval < settings.azMin)
                    curval = settings.azMin;

                this.azPos = LowPass(curval, this.azPos);
                if (bUpdating)
                    bUpdating = false;

            }
            retval = (this.azForward ? this.azPos : this.azPos-360.0);
            return retval;
        }
        public double elReadPosition()
        {
            double curval = 0.0;
            if (dev.Connected)
            {
                bUpdating = true;
                curval = elEncoder.countsToDegrees(elEncoder.readNormalizedEncoderBits());
                if (curval > settings.elMax)
                    curval = settings.elMax;
                else if (curval < settings.elMin)
                    curval = settings.elMin;

                this.elPos = LowPass(curval, this.elPos);
                if (bUpdating)
                    bUpdating = false;
            }
            return this.elPos;
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
            if (Math.Abs(distanceF-distanceR) < 1)
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
                command = distanceF*dir;
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
            if (dev.Connected)
            {
                setControlBits(true, Math.Abs(azVel) < 0.05, (azVel > 0.05));
                this.dev.SetPwmParameters(settings.azPWMchan, Eth32PwmChannel.Normal, 10000.0, Math.Abs(azVel));
            }
        }
        public void setEl(double elVel)
        {
            if (dev.Connected)
            {
                setControlBits(false, Math.Abs(elVel) < 0.05, (elVel < -0.05));
                this.dev.SetPwmParameters(settings.elPWMchan, Eth32PwmChannel.Normal, 10000.0, Math.Abs(elVel));
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
                if (settings.driveEnablebit >= 0)
                {
                    this.dev.OutputBit(this.outputPortNum, settings.driveEnablebit, val);
                }
                bit = isAz ? settings.azEnable : settings.elEnable;
                this.dev.OutputBit(this.outputPortNum, bit, val);

                if (!isAz) val = polarity ? val : (val > 0 ? 0 : 1);
                bit = isAz ? settings.azCCWbit : settings.elCCWbit;
                this.dev.OutputBit(this.outputPortNum, bit, val);

                bit = isAz ? settings.azCWbit : settings.elCWbit;
                this.dev.OutputBit(this.outputPortNum, bit, val);
            }
            else //if not stopped toggle the global enable which is 
            {
                if (settings.driveEnablebit >= 0)
                {
                    val = 1;
                    val = polarity ? val : (val > 0 ? 0 : 1);
                    this.dev.OutputBit(this.outputPortNum, settings.driveEnablebit, val);
                }
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
                    this.dev.OutputBit(this.outputPortNum, bit, val);
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
                    bit = isAz ? settings.azCCWbit : settings.elCCWbit;
                    val = stopped?0:1;
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

            while (bUpdating)
            {
                Thread.Sleep(50);
                if (azPid.Complete && elPid.Complete)
                {
                    bUpdating = false;
                }
            }
            if (azPos > settings.azMax)
                azPos = settings.azMax;
            else if (azPos < settings.azMin)
                azPos = settings.azMin;
            if (elPos > settings.elMax)
                elPos = settings.elMax;
            else if (elPos < settings.elMin)
                elPos = settings.elMin;

            GeoAngle AzAngle = GeoAngle.FromDouble(azPos, true);
            GeoAngle ElAngle = GeoAngle.FromDouble(elPos);
            this.Azimuth.Text = string.Format("{0:D3} : {1:D2}", AzAngle.Degrees, AzAngle.Minutes);
            this.Elevation.Text = string.Format("{0:D2} : {1:D2}", ElAngle.Degrees, ElAngle.Minutes);
            //this.Elevation.Text = string.Format("{0:D4}", elEncoder.readNormalizedEncoderBits());
            if (azCommand == -1.0)
            {
                this.commandAz.Text = this.Azimuth.Text;
                this.commandEl.Text = this.Elevation.Text;
                azCommand = azPos;
                elCommand = elPos;
            }

            //            RaDec astro = celestialConversion.CalcualteRaDec(89.99 - elPos, azPos, settings.latitude, settings.longitude);
            RaDec astro = celestialConversion.CalcualteRaDec(elPos, azPos, settings.latitude, settings.longitude);
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

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
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            updatePosition();
            updateStatus();
        }

        private void watchdog_Tick(object sender, EventArgs e)
        {
            WatchdoLoopcount++;
            if (WatchdoLoopcount > 5)
                WatchdoLoopcount = 0;
            int bitState = (WatchdoLoopcount /*/ 10*/) % 5 > 0 ? 1 : 0;
            this.dev.OutputBit(4, 0, bitState);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dev != null)
            {
                if (timer != null)
                    timer.Stop();
                if (watchdog != null)
                    watchdog.Stop();

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
            timer.Stop();
            watchdog.Stop();
            Config cfgDialog = new Config(dev, this.settings);
            if (cfgDialog.ShowDialog() == DialogResult.OK)
            {
                string configFile = Application.StartupPath + "\\dishConfig.xml";
                configFileHandler.writeConfig(configFile, cfgDialog.settings);
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
            this.elCommand += settings.jogIncrement;
            if (this.elCommand > settings.elMax)
                this.elCommand = settings.elMax;
            GeoAngle mElAngle = GeoAngle.FromDouble(elCommand);
            this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);
            this.Go();
        }

        private void Down_Click(object sender, EventArgs e)
        {
            this.elCommand += settings.jogIncrement;
            if (this.elCommand < settings.elMin)
                this.elCommand = settings.elMin;
            GeoAngle mElAngle = GeoAngle.FromDouble(elCommand);
            this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);
            this.Go();
        }

        private void CW_Click(object sender, EventArgs e)
        {
            this.azCommand += settings.jogIncrement;
            if (this.azCommand > settings.azMax)
                this.azCommand = settings.azMax;
            GeoAngle mAzAngle = GeoAngle.FromDouble(azCommand, true);
            this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
            this.Go();
        }

        private void CCW_Click(object sender, EventArgs e)
        {
            this.azCommand -= settings.jogIncrement;
            if (this.azCommand < settings.azMin)
                this.azCommand = settings.azMin;
            GeoAngle mAzAngle = GeoAngle.FromDouble(azCommand, true);
            this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
            this.Go();
        }

    }
}