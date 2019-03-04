﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using System.Windows.Forms;
using System.Diagnostics;

#if _TEST
using InterfaceFake;
#else
using WinfordEthIO;
#endif

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
        System.Windows.Forms.Timer mainTimer = null;
        DateTime messageTime = DateTime.Now;
        Thread buttonThread;
        ManualResetEvent jogEvent;
        buttonDir direction;
        List<presets> Presets = null;
        int currentIncrement = 0;
        Stopwatch sw;

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
                Program.settings = configFileHandler.readConfig(configFile);
                Program.state.appConfigured = true;
                Program.state.connectEvent.Set();
            }//if config exists
            else
            {
                Program.settings = new configModel();
                Config cfgDialog = new Config(dev, Program.settings);
                if (cfgDialog.ShowDialog() == DialogResult.OK)
                {
                    configFileHandler.writeConfig(configFile, cfgDialog.settings);
                    MessageBox.Show("Saved");
                    Program.state.appConfigured = true;
                    Program.state.connectEvent.Set();
                }
            }//no config found

            Program.state.state = DishState.Unknown;

            //set up presets, set up various Motion control objects, logger but only if config exists and is valid.
            if (Program.state.appConfigured)
            {
                //get presets array
                Presets = Program.settings.getPresetList();
                foreach (presets p in Presets)
                {
                    if (!String.IsNullOrEmpty(p.Text))
                    {
                        presetSelector.Items.Add(p.Text);
                    }
                }
                presetSelector.SelectedIndex = 0;
                RollingLogger.setupRollingLogger(Program.settings.positionFileLog, Program.settings.maxPosLogSizeBytes, Program.settings.maxPosLogFiles);
                sw = Stopwatch.StartNew();
                //we are configured so signal the Connect event so main motion can connect hardware
                Program.state.connectEvent.Set();
                mainTimer = new System.Windows.Forms.Timer();
                mainTimer.Interval = 150;
                mainTimer.Tick += new EventHandler(TimerEventProcessor);
                mainTimer.Start();
            }//if config exists
        }

        //call back for button thread.  Waits for jogEvent to be signalled.  If btEnabled is false exists (allows clean shutdown)
        //if not existing figures out which button was clicked from direction var, jogs appropriate axis with command voltage specified in
        //jog Increment setting for 1/2 second.
        private void buttonThreadHandler()
        {
            while (Program.state.btEnabled)
            {
                jogEvent.WaitOne();
                if (!Program.state.btEnabled) // bail if we are being singalled due to exit
                    return;
                if (Program.state.state != DishState.Stopped) //do nothing if already moving, or in unknown state
                {
                    jogEvent.Reset();
                    continue;
                }
                double curvel = Program.settings.jogIncrement;
                double dirMul = 1.0;
                if (this.direction == buttonDir.South || this.direction == buttonDir.East)
                    dirMul = -1.0;
                Program.state.state = DishState.Moving;
                updateStatus();

                if (this.direction == buttonDir.South || this.direction == buttonDir.North)
                {
                    Program.state.commandElevationRate = curvel * dirMul;
                    Program.state.command = CommandType.Jog;
                    Program.state.go.Set();
                    while (Program.state.state != DishState.Moving)
                        Thread.Sleep(10); //wait till we move before timing stop.
                    Thread.Sleep(500);
                    Program.state.command = CommandType.Stop;
                    Program.state.go.Set();
                }
                else
                {
                    Program.state.commandAzimuthRate = curvel * dirMul;
                    Program.state.command = CommandType.Jog;
                    Program.state.go.Set();
                    while (Program.state.state != DishState.Moving)
                        Thread.Sleep(10); //wait till we move before timing stop.
                    Thread.Sleep(500);
                    Program.state.command = CommandType.Stop;
                    Program.state.go.Set();
                }
                Program.state.state = DishState.Stopped;
                updateStatus();
                jogEvent.Reset();
            }
        }

        //enable/disable certain controls on main form depending on dish state (e.g. don't allow changes in target pos when moving)
        private void enableControls()
        {
            lunarTrack.Enabled = dev.Connected;

            bool bEnabled = dev.Connected && !Program.state.trackMoon && Program.state.state != DishState.Moving;

            commandAz.Enabled = bEnabled;
            commandEl.Enabled = bEnabled;
            commandRA.Enabled = bEnabled;
            commandDec.Enabled = bEnabled;
            track.Enabled = bEnabled;

            if (Program.state.trackCelestial)
            {
                Program.state.trackMoon = false;
                lunarTrack.Enabled = false;
                commandAz.Enabled = false;
                commandEl.Enabled = false;
            }
            else
            {
                commandRA.Enabled = false;
                commandDec.Enabled = false;
            }

            Park.Enabled = (Program.state.state == DishState.Stopped);
            GO.Enabled = (Program.state.state == DishState.Stopped);

            STOP.Enabled = (Program.state.state != DishState.Stopped && Program.state.state != DishState.Unknown);
        }

        //update status indicators
        private void updateStatus()
        {
            if (Program.state.connected)
            {
                connectedIcon.BackColor = Color.Green;
                connect.Enabled = false;
            }
            else
            {
                connectedIcon.BackColor = Color.Red;
                connect.Enabled = true;
            }

            movingIcon.BackColor = (Program.state.state == DishState.Unknown) ? Color.Yellow : (Program.state.state == DishState.Stopped ? Color.Green : Color.Red);
            trackingIcon.BackColor = (Program.state.state == DishState.Tracking) ? Color.Blue : Color.White;
        }

        //called when connect button clicked
        private void connect_Click(object sender, EventArgs e)
        {
            Program.state.connectEvent.Set();
        }

        //update position display
        private void updatePosition()
        {
            if ((DateTime.Now - messageTime).TotalSeconds > 30)
            {
                Message.Text = "";
            }
            double az, el;
            az = Program.state.azimuth;
            el = Program.state.elevation;
            //set up display variables in deg, min, sec format
            if (el > 89.99)
            {
                az -= 180.0;
                if (az < 0.0) az += 360.0;
                el -= 90.0;
            }
           GeoAngle AzAngle = GeoAngle.FromDouble(az, true);
           GeoAngle ElAngle = GeoAngle.FromDouble(el, true);

            //convert to RA/DEC
            RaDec astro = celestialConversion.CalcualteRaDec(el, az, Program.settings.latitude, Program.settings.longitude, !rADegreesToolStripMenuItem.Checked);
            //set up RA DEC as deg,min,sec
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

            //log position every tenth time through
            string posLog = string.Format("RA {0:D3} : {1:D2}\t DEC {2}{3:D3} : {4:D2}", RA.Degrees, RA.Minutes, Dec.IsNegative?"-":"", Dec.Degrees, Dec.Minutes);
            currentIncrement++;
            if (!Program.settings.tuningLog && currentIncrement % 20 == 0)
            {
                RollingLogger.LogMessage(posLog);
                currentIncrement = 0;
            }
            else if (Program.settings.tuningLog)
            {
                long time = sw.ElapsedMilliseconds;
                posLog = string.Format("{0},{1},{2}", time, az, el);
                RollingLogger.LogMessage(posLog);
                currentIncrement = 0;
            }
            
            if (!showSecondsToolStripMenuItem.Checked)
            {
                //show AZ, EL on main form
                this.Azimuth.Text = string.Format("{0:D3} : {1:D2}", AzAngle.Degrees, AzAngle.Minutes);
                this.Elevation.Text = string.Format("{0:D2} : {1:D2}", ElAngle.Degrees, ElAngle.Minutes);

                //show RA,DEC on main form
                this.RA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
                this.DEC.Text = string.Format("{0}{1:D2} : {2:D2}", Dec.IsNegative ? "-" : "", Dec.Degrees, Dec.Minutes);
            }
            else
            {
                //show AZ, EL on main form
                this.Azimuth.Text = string.Format("{0:D3} : {1:D2} : {2:D2}", AzAngle.Degrees, AzAngle.Minutes, (int)(AzAngle.Seconds+0.5)%60);
                this.Elevation.Text = string.Format("{0:D2} : {1:D2} : {2:D2}", ElAngle.Degrees, ElAngle.Minutes, (int)(ElAngle.Seconds + 0.5)%60);

                //show RA,DEC on main form
                this.RA.Text = string.Format("{0:D3} : {1:D2} : {2:D2}", RA.Degrees, RA.Minutes, (int)(RA.Seconds + 0.5)%60);
                this.DEC.Text = string.Format("{0}{1:D2} : {2:D2} : {2:D2}", Dec.IsNegative ? "-" : "", Dec.Degrees, Dec.Minutes, (int)(Dec.Seconds + 0.5)%60);
            }
            if (!string.IsNullOrEmpty(Program.state.errorMessage))
            {
                Message.Text = Program.state.errorMessage;
                Program.state.errorMessage = string.Empty;
            }

            if (Program.state.command == CommandType.Track)
            {
                AltAz aa = celestialConversion.CalculateAltAz(Program.state.commandRightAscension, Program.state.commandDeclination, Program.settings.latitude, Program.settings.longitude);
                GeoAngle cAzAngle = GeoAngle.FromDouble(aa.Az, true);
                GeoAngle cElAngle = GeoAngle.FromDouble(aa.Alt);
                this.commandAz.Text = string.Format("{0:D3} : {1:D2}", cAzAngle.Degrees, cAzAngle.Minutes);
                this.commandEl.Text = string.Format("{0:D2} : {1:D2}", cElAngle.Degrees, cElAngle.Minutes);
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
                Program.state.btEnabled = false;
                if (mainTimer != null)
                    mainTimer.Stop();
                Thread.Sleep(100);
                jogEvent.Set();
                Thread.Sleep(50);
                Program.state.command = CommandType.Stop;
                Program.state.go.Set();
                Thread.Sleep(250);
                Program.state.appConfigured = false;
                Program.state.connectEvent.Set();
                Thread.Sleep(250);
            }

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string startingIP = Program.settings.eth32Address;
            this.Stop();
            mainTimer.Stop();
            Thread.Sleep(200);
            Program.state.appConfigured = false;
            Program.state.connectEvent.Set();

            Config cfgDialog = new Config(dev, Program.settings);
            if (cfgDialog.ShowDialog() == DialogResult.OK)
            {
                string configFile = Application.StartupPath + "\\dishConfig.xml";
                configFileHandler.writeConfig(configFile, cfgDialog.settings);
                RollingLogger.setupRollingLogger(Program.settings.positionFileLog, Program.settings.maxPosLogSizeBytes, Program.settings.maxPosLogFiles);
                MessageBox.Show("Saved");
                Program.state.appConfigured = true;
                Program.state.connectEvent.Set();
                mainTimer.Start();
            }
        }

        private void track_CheckedChanged(object sender, EventArgs e)
        {
            Program.state.trackCelestial = track.Checked;
        }

        private void commandRA_TextChanged(object sender, EventArgs e)
        {
            if (Program.state.command != CommandType.Track)
            {
                Program.state.commandRightAscension = GeoAngle.FromString(commandRA.Text).ToDouble();
                Program.state.raDecCommand = true;
            }
        }

        private void commandDec_TextChanged(object sender, EventArgs e)
        {
            if (Program.state.command != CommandType.Track)
            {
                Program.state.commandDeclination = GeoAngle.FromString(commandDec.Text).ToDouble();
                Program.state.raDecCommand = true;
            }
        }

        private void commandEl_TextChanged(object sender, EventArgs e)
        {
            if (Program.state.command != CommandType.Track)
            {
                Program.state.commandElevation = GeoAngle.FromString(commandEl.Text).ToDouble();
                Program.state.raDecCommand = false;
            }
        }

        private void commandAz_TextChanged(object sender, EventArgs e)
        {
            if (Program.state.command != CommandType.Track)
            {
                Program.state.commandAzimuth = GeoAngle.FromString(commandAz.Text).ToDouble();
                Program.state.raDecCommand = false;
            }
        }

        private void lunarTrack_Click(object sender, EventArgs e)
        {
            Program.state.trackMoon = !Program.state.trackMoon;
        }

        private void GO_Click(object sender, EventArgs e)
        {
            this.Go();
        }

        public void Go()
        {
            if ((Program.state.trackCelestial || Program.state.trackMoon))
                Program.state.command = CommandType.Track;
            else
                Program.state.command = CommandType.Move;
            Program.state.go.Set();
        }

        private void Park_Click(object sender, EventArgs e)
        {
            Program.state.commandAzimuth = Program.settings.azPark;
            Program.state.commandElevation = Program.settings.elPark;
            GeoAngle mAzAngle = GeoAngle.FromDouble(Program.state.commandAzimuth, true);
            GeoAngle mElAngle = GeoAngle.FromDouble(Program.state.commandElevation);
            this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
            this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);

            RaDec astro = celestialConversion.CalcualteRaDec(mElAngle.ToDouble(), mAzAngle.ToDouble(), Program.settings.latitude, Program.settings.longitude);
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

            this.commandRA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
            this.commandDec.Text = string.Format("{0:D2} : {1:D2}", Dec.Degrees, Dec.Minutes);

            Program.state.state = DishState.Parking; ;
            Program.state.command = CommandType.Move;
            Program.state.go.Set();
        }

        private void STOP_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void driveTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            testDrive testDialog = new testDrive(dev, Program.settings, this);
            testDialog.ShowDialog();

        }

        private void presetSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            int item = presetSelector.SelectedIndex;
            if (item != 0)
            {
                Program.state.commandAzimuth = this.Presets[item].Az;
                Program.state.commandElevation = this.Presets[item].El;
                GeoAngle mAzAngle = GeoAngle.FromDouble(Program.state.commandAzimuth, true);
                GeoAngle mElAngle = GeoAngle.FromDouble(Program.state.commandElevation);
                this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
                this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);

                RaDec astro = celestialConversion.CalcualteRaDec(mElAngle.ToDouble(), mAzAngle.ToDouble(), Program.settings.latitude, Program.settings.longitude);
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
            updateStatus();
            updatePosition();
        }

        private void Stop()
        {
            Program.state.command = CommandType.Stop;
            Program.state.go.Set();
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
            Program.state.commandAzimuth = Program.settings.azSouthPark;
            Program.state.commandElevation = Program.settings.elSouthPark;
            GeoAngle mAzAngle = GeoAngle.FromDouble(Program.state.commandAzimuth, true);
            GeoAngle mElAngle = GeoAngle.FromDouble(Program.state.commandElevation);
            this.commandAz.Text = string.Format("{0} : {1}", mAzAngle.Degrees, mAzAngle.Minutes);
            this.commandEl.Text = string.Format("{0} : {1}", mElAngle.Degrees, mElAngle.Minutes);

            RaDec astro = celestialConversion.CalcualteRaDec(mElAngle.ToDouble(), mAzAngle.ToDouble(), Program.settings.latitude, Program.settings.longitude);
            GeoAngle Dec = GeoAngle.FromDouble(astro.Dec);
            GeoAngle RA = GeoAngle.FromDouble(astro.RA, true);

            this.commandRA.Text = string.Format("{0:D3} : {1:D2}", RA.Degrees, RA.Minutes);
            this.commandDec.Text = string.Format("{0:D2} : {1:D2}", Dec.Degrees, Dec.Minutes);

            Program.state.state = DishState.Parking;
            Program.state.go.Set();

            Program.state.command = CommandType.Move;
            Program.state.go.Set();
        }

        private void ShowSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSecondsToolStripMenuItem.Checked = !showSecondsToolStripMenuItem.Checked;
        }

        private void RADegreesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rADegreesToolStripMenuItem.Checked = !rADegreesToolStripMenuItem.Checked;
        }

        private void ShowSkyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSkyToolStripMenuItem.Enabled = Program.state.SkyVisible;
            SkyMap sky = new SkyMap();
            sky.Show();
        }
    }
}
