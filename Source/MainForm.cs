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
        configModel settings;
        bool appConfigured = false;
        DishState state = DishState.Unknown;
        System.Windows.Forms.Timer mainTimer = null;
        bool bRunTick = false;
        bool trackCelestial = false;
        bool trackMoon = false;
        bool bUpdating = false;
        DateTime messageTime = DateTime.Now;
        Thread buttonThread;
        ManualResetEvent jogEvent;
        bool btEnabled = true;
        buttonDir direction;
        List<presets> Presets = null;
        int TimerTickms = 10;

        public MainForm(Eth32 dev)
        {
            //set up UI control attributes (auto-generated)
            InitializeComponent();

            this.dev = dev;
            
            //setup for jog button handler thread, triggering event
            jogEvent = new ManualResetEvent(false);
            this.buttonThread = new Thread(buttonThreadHandler);
            this.buttonThread.Start();

            //try to read config file - open config dialog if not found
            string configFile = Application.StartupPath + "\\dishConfig.xml";
            if (File.Exists(configFile))
            {
                settings = configFileHandler.readConfig(configFile);
                appConfigured = true;
            }//if config exists
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
            }//no config found

            state = DishState.Unknown;

            //set up presets, set up various Motion control objects, logger but only if config exists and is valid.
            if (appConfigured)
            {
                //get presets array
                Presets = settings.getPresetList();
                foreach (presets p in Presets)
                {
                    if (!String.IsNullOrEmpty(p.Text))
                    {
                        presetSelector.Items.Add(p.Text);
                    }
                }
                presetSelector.SelectedIndex = 0;

                RollingLogger.setupRollingLogger(settings.positionFileLog, settings.maxPosLogSizeBytes, settings.maxPosLogFiles);

                MotionSetup();
            }//if config exists
        }

        //call back for button thread.  Waits for jogEvent to be signalled.  If btEnabled is false exists (allows clean shutdown)
        //if not existing figures out which button was clicked from direction var, jogs appropriate axis with command voltage specified in
        //jog Increment setting for 1/2 second.
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
                    this.setEl(curvel * dirMul);
                    Thread.Sleep(500);
                    this.setEl(0.0);
                }
                else
                {
                    this.setAz(curvel * dirMul);
                    Thread.Sleep(500);
                    this.setAz(0.0);
                }
                state = DishState.Stopped;
                updateStatus();
                jogEvent.Reset();
            }
        }

        //enable/disable certain controls on main form depending on dish state (e.g. don't allow changes in target pos when moving)
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
        
        //update status indicators
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

        //called when connect button clicked
        private void connect_Click(object sender, EventArgs e)
        {
            if (!dev.Connected) 
                this.Connect();
        }

        //update position display
        private void updatePosition()
        {
            if ((DateTime.Now - messageTime).TotalSeconds > 30)
            {
                Message.Text = "";
            }

            //read actual form encoders
            //if (dev.Connected && state == DishState.Stopped)
            //{
            //    azPos = azReadPosition();
            //    elPos = elReadPosition();
            //}
            //set up display variables in deg, min, sec format
            GeoAngle AzAngle = GeoAngle.FromDouble(azPos, true);
            GeoAngle ElAngle = GeoAngle.FromDouble(elPos);
            
            //convert to RA/DEC
            RaDec astro = celestialConversion.CalcualteRaDec(elPos, azPos, settings.latitude, settings.longitude);
            //set up RA DEC as deg,min,sec
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);
            
            //log position every tenth time through
            string posLog = string.Format("RA {0:D3} : {1:D2}\t DEC {2:D3} : {3:D2}", RA.Degrees, RA.Minutes, Dec.Degrees, Dec.Minutes);
            currentIncrement++;
            if (currentIncrement % 9 == 0)
            {
                RollingLogger.LogMessage(posLog);
                currentIncrement = 0;
            }

            //show AZ, EL on main form
            this.Azimuth.Text = string.Format("{0:D3} : {1:D2}", AzAngle.Degrees, AzAngle.Minutes);
            this.Elevation.Text = string.Format("{0:D2} : {1:D2}", ElAngle.Degrees, ElAngle.Minutes);
            
            //first time through set az, el command to current pos
            if (azCommand == -1.0)
            {
                this.commandAz.Text = this.Azimuth.Text;
                this.commandEl.Text = this.Elevation.Text;
                azCommand = azPos;
                elCommand = elPos;
            }

            //show RA,DEC on main form
            this.RA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
            this.DEC.Text = string.Format("{0:D2} : {1:D2}", Dec.Degrees, Dec.Minutes);

            //if celestial track is set, check to make sure command position is above horizon if so then enable motion
            //probably have to have a completely seperate velocity track loop here - PID loop may not be satisfactor
            //-----NEEDS TESTING
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


            //if Lunar track is set, check to make sure Moon position is above horizon if so then enable motion
            //probably have to have a completely seperate velocity track loop here - PID loop may not be satisfactor
            //-----NEEDS TESTING
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

            //send stop command if we have to here
            if (state != DishState.Stopped && state != DishState.Unknown)
            {
                if (azPid.Complete && elPid.Complete)
                {
                    this.Stop();
                }
            }
        }

        //this is the sloppy Forms timer that controls UI update
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            updatePosition();
            updateStatus();
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
            this.Stop();
            mainTimer.Stop();
            updateThread.Abort();
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

        private void SouthPark_Click(object sender, EventArgs e)
        {
            azCommand = settings.azSouthPark;
            elCommand = settings.elSouthPark;
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
    }
}
