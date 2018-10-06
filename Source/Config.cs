using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#if _TEST
using InterfaceFake;
#else
using WinfordEthIO;
#endif

namespace DishControl
{
    public partial class Config : Form
    {
        private Queue<string> messages = new Queue<string>(101);
        private Eth32 dev = null;
        private NmeaParser.SerialPortDevice device;

        public configModel settings = null;
        public Config(Eth32 dev, configModel settings)
        {
            InitializeComponent();
            this.dev = dev;
            this.settings = settings;
            if (settings.latitude != 0.0)
            {
                
                this.latitude.Text = GeoAngle.FromDouble (settings.latitude).ToString("NS");
                this.longitude.Text = GeoAngle.FromDouble(settings.longitude).ToString("WE");
                this.altitude.Text = settings.altitude.ToString();
                this.alpha.Text = settings.alpha.ToString();
                this.AzRevsPerRot.Text = settings.AzimuthRevsPerRot.ToString();
                this.encBitsAz.Text = settings.AzimuthEncoderBits.ToString();
                this.AzStartBit.Text = settings.AzimuthStartBit.ToString();
                this.AzOffset.Text = settings.AzimuthOffsetDeg.ToString();
                this.ElRevsPerRot.Text = settings.ElevationRevsPerRot.ToString();
                this.encBitsEl.Text = settings.ElevationEncoderBits.ToString();
                this.ElStartBit.Text = settings.ElevationStartBit.ToString();
                this.ElOffset.Text = settings.ElevationOffsetDeg.ToString();
                this.encoder_coding.SelectedItem = settings.coding.ToString();
                this.cmbETH32.Items.Add(settings.eth32Address);
                this.cmbETH32.SelectedItem = settings.eth32Address;
                this.outputPort.SelectedItem = settings.outputPort;
                this.jogIncrement.Text = settings.jogIncrement.ToString();
                this.driveEnableBit.Text = settings.driveEnablebit.ToString();
                this.azCBactiveHi.Checked = settings.azActiveHi;
                this.azCW_CCW.Checked = settings.azDriveType == driveType.CCW;
                this.azEnbDir.Checked = settings.azDriveType == driveType.DirEnable;
                this.azBoth.Checked = settings.azDriveType == driveType.Both;
                if (settings.azDriveType == driveType.CCW)
                {
                    azCB1.Text = "CW";
                    azCB2.Text = "CCW";
                    azCB3.Text = "";
                    azEnBit.Enabled = false;
                }
                else if (settings.azDriveType == driveType.DirEnable)
                {
                    azCB1.Text = "Dir";
                    azCB2.Text = "Enable";
                    azCB3.Text = "";
                    azEnBit.Enabled = false;
                }
                else
                {
                    azCB1.Text = "CW";
                    azCB2.Text = "CCW";
                    azCB3.Text = "Enable";
                    azEnBit.Enabled = true;
                }
                this.azCWbit.Text = settings.azCWbit.ToString();
                this.azCCWbit.Text = settings.azCCWbit.ToString();
                this.azEnBit.Text = settings.azEnable.ToString();
                this.azPWMch.Text = settings.azPWMchan.ToString();
                this.azKd.Text = settings.azKd.ToString();
                this.azKi.Text = settings.azKi.ToString();
                this.azKp.Text = settings.azKp.ToString();
                this.azMax.Text = settings.azMax.ToString();
                this.azMin.Text = settings.azMin.ToString();
                this.azOutMax.Text = settings.azOutMax.ToString();
                this.azOutMin.Text = settings.azOutMin.ToString();
                this.azPark.Text = settings.azPark.ToString();
                this.SouthParkPosAz.Text = settings.azSouthPark.ToString();

                this.elCBactiveHi.Checked = settings.elActiveHi;
                this.elCW_CCW.Checked = settings.elDriveType == driveType.CCW;
                this.elEnbDir.Checked = settings.elDriveType == driveType.DirEnable;
                this.elBoth.Checked = settings.elDriveType == driveType.Both;
                if (settings.azDriveType == driveType.CCW)
                {
                    elCB1.Text = "CW";
                    elCB2.Text = "CCW";
                    elCB3.Text = "";
                    elEnBit.Enabled = false;
                }
                else if (settings.azDriveType == driveType.DirEnable)
                {
                    elCB1.Text = "Dir";
                    elCB2.Text = "Enable";
                    elCB3.Text = "";
                    elEnBit.Enabled = false;
                }
                else
                {
                    elCB1.Text = "CW";
                    elCB2.Text = "CCW";
                    elCB3.Text = "Enable";
                    elEnBit.Enabled = true;
                }
                this.elCWbit.Text = settings.elCWbit.ToString();
                this.elCCWbit.Text = settings.elCCWbit.ToString();
                this.elEnBit.Text = settings.elEnable.ToString();
                this.elPWMch.Text = settings.elPWMchan.ToString();
                this.elKd.Text = settings.elKd.ToString();
                this.elKi.Text = settings.elKi.ToString();
                this.elKp.Text = settings.elKp.ToString();
                this.elMax.Text = settings.elMax.ToString();
                this.elMin.Text = settings.elMin.ToString();
                this.elOutMax.Text = settings.elOutMax.ToString();
                this.elOutMin.Text = settings.elOutMin.ToString();
                this.elPark.Text = settings.elPark.ToString();
                this.SouthParkPosEl.Text = settings.elSouthPark.ToString();

                this.PosLogFile.Text = settings.positionFileLog;
                this.maxPosLogSize.Text = settings.maxPosLogSizeBytes.ToString();
                this.MaxFiles.Text = settings.maxPosLogFiles.ToString();

                this.PS1Name.Text = settings.Preset1Name;
                this.PS1Az.Text =  settings.Preset1Az.ToString();
                this.PS1El.Text =  settings.Preset1El.ToString();

                this.PS2Name.Text = settings.Preset2Name;
                this.PS2Az.Text =  settings.Preset2Az.ToString();
                this.PS2El.Text =  settings.Preset2El.ToString();

                this.PS3Name.Text = settings.Preset3Name;
                this.PS3Az.Text =  settings.Preset3Az.ToString();
                this.PS3El.Text =  settings.Preset3El.ToString();

                this.PS4Name.Text = settings.Preset4Name;
                this.PS4Az.Text =  settings.Preset4Az.ToString();
                this.PS4El.Text =  settings.Preset4El.ToString();

                this.PS5Name.Text = settings.Preset5Name;
                this.PS5Az.Text =  settings.Preset5Az.ToString();
                this.PS5El.Text =  settings.Preset5El.ToString();
            }
        }

        private void detect_Click(object sender, EventArgs e)
        {
#if !_TEST
            Eth32Config ethdetect = new Eth32Config();
            int i;

            // Start with an empty Combo Box
            cmbETH32.Items.Clear();

            // Detect any ETH32 devices on the local network
            // Make the mouse pointer an hour-glass during detection (which takes a couple seconds)
            // so the user knows it's in progress.
            Cursor.Current = Cursors.WaitCursor;
            ethdetect.Query();
            Cursor.Current = Cursors.Default;

            if (ethdetect.NumResults > 0) // If we found at least one device
            {
                // loop through them and add the Active IP of each to the combo box list
                for (i = 0; i < ethdetect.NumResults; i++)
                {
                    // Retrieve the active IP of this particular device and add it to the combo box
                    cmbETH32.Items.Add(ethdetect.Result[i].active_ip.ToString());

                }
                // Make the combo box drop down so the user can see the available options
                cmbETH32.DroppedDown = true;
            }
            else
            {
                // Otherwise, we didn't find any devices.  Let the user know
                MessageBox.Show("No devices were found.  Detection will only find ETH32 devices on the local network segment.  For devices that are outside the local segment, please enter the IP address or host name into the combo, and click Connect.");
            }
#endif
        }

        private void readGPSpos_Click(object sender, EventArgs e)
        {
            messages.Clear();
            var port = new System.IO.Ports.SerialPort(GPScomPort.Text, 4800);
            device = new NmeaParser.SerialPortDevice(port);
            device.MessageReceived += device_MessageReceived;
            device.OpenAsync();

        }

        private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
        {
            messages.Enqueue(args.Message.MessageType + ": " + args.Message.ToString());
            if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100

            NmeaParser.Nmea.Gps.Gpgll gll;
            NmeaParser.Nmea.Gps.Gpgga gga;
            NmeaParser.Nmea.Gps.Garmin.Pgrme rme;
            IEnumerable<NmeaParser.Nmea.Gps.Gpgsv> gpgsvParts;

            if (args.Message is NmeaParser.Nmea.Gps.Gpgsv)
            {
                var gpgsv = (NmeaParser.Nmea.Gps.Gpgsv)args.Message;
                if (args.IsMultipart && args.MessageParts != null)
                    gpgsvParts = args.MessageParts.OfType<NmeaParser.Nmea.Gps.Gpgsv>();
            }
            if (args.Message is NmeaParser.Nmea.Gps.Gpgga)
            {
                gga = args.Message as NmeaParser.Nmea.Gps.Gpgga;
                settings.latitude = gga.Latitude;
                this.latitude.Invoke(new MethodInvoker(delegate
                {
                    this.latitude.Text = gga.Latitude.ToString();
                }));
                settings.longitude = gga.Longitude;
                this.longitude.Invoke(new MethodInvoker(delegate
                {
                    this.longitude.Text = gga.Longitude.ToString();
                }));
                settings.altitude = gga.Altitude;
                this.altitude.Invoke(new MethodInvoker(delegate
                {
                    this.altitude.Text = gga.Altitude.ToString() + " " + gga.AltitudeUnits;
                }));
            }
            else if (args.Message is NmeaParser.Nmea.Gps.Garmin.Pgrme)
            {
                rme = args.Message as NmeaParser.Nmea.Gps.Garmin.Pgrme;
                double total_error = Math.Sqrt(rme.HorizontalError * rme.HorizontalError + rme.VerticalError * rme.VerticalError);
                if (total_error < 10.0)
                {
                    this.fix.Invoke(new MethodInvoker(delegate
                    {
                        fix.Text = "Good position";
                    }));
                    device.CloseAsync();
                }
            }
        }

        private void latitude_TextChanged(object sender, EventArgs e)
        {
            settings.latitude = GeoAngle.FromString(this.latitude.Text).ToDouble(); 
        }

        private void longitude_TextChanged(object sender, EventArgs e)
        {
            settings.longitude = GeoAngle.FromString(this.longitude.Text).ToDouble();
        }

        private void altitude_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.altitude.Text, out d);
            settings.altitude = d;
        }

        private void cmbETH32_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            settings.eth32Address = comboBox.SelectedItem.ToString();
        }

        private void encoder_coding_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            settings.coding = (encoderCode)Enum.Parse(typeof(encoderCode), comboBox.SelectedItem.ToString());
        }

        private void AzRevsPerRot_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.AzRevsPerRot.Text, out i);
            settings.AzimuthRevsPerRot = i;
        }

        private void encBitsAz_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.encBitsAz.Text, out i);
            settings.AzimuthEncoderBits = i;

        }

        private void AzStartBit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.AzStartBit.Text, out i);
            settings.AzimuthStartBit = i;
        }

        private void AzOffset_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.AzOffset.Text, out d);
            settings.AzimuthOffsetDeg = d;
        }

        private void ElRevsPerRot_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.ElRevsPerRot.Text, out i);
            settings.ElevationRevsPerRot = i;
        }

        private void encBitsEl_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.encBitsEl.Text, out i);
            settings.ElevationEncoderBits = i;
        }

         private void ElStartBit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.ElStartBit.Text, out i);
            settings.ElevationStartBit = i;
        }

        private void ElOffset_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.ElOffset.Text, out d);
            settings.ElevationOffsetDeg = d;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void outputPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            settings.outputPort = comboBox.SelectedItem.ToString();
        }

        private void jogIncrement_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.jogIncrement.Text, out d);
            settings.jogIncrement = d;
        }

        private void driveEnableBit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.driveEnableBit.Text, out i);
            if (i < -1 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7, -1 to disable");
                return;
            }
            settings.driveEnablebit = i;
        }

        private void azCWbit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.azCWbit.Text, out i);
            if (i < 0 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7");
                return;
            }
            settings.azCWbit = i;
        }

        private void azCCWbit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.azCCWbit.Text, out i);
            if (i < 0 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7");
                return;
            }
            settings.azCCWbit = i;
        }
        private void azEnBit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.azEnBit.Text, out i);
            if (i < 0 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7");
                return;
            }
            settings.azEnable = i;
        }

        private void azPWMch_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.azPWMch.Text, out i);
            if (i < 0 || i > 1)
            {
                MessageBox.Show("PWM chan must be 0 or 1");
                return;
            }
            settings.azPWMchan = i;
        }

        private void azKp_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azKp.Text, out d);
            settings.azKp = d;
        }

        private void azKi_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azKi.Text, out d);
            settings.azKi = d;
        }

        private void azKd_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azKd.Text, out d);
            settings.azKd = d;
        }

        private void azMax_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azMax.Text, out d);
            settings.azMax = d;
        }

        private void azMin_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azMin.Text, out d);
            settings.azMin = d;
        }

        private void azPark_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azPark.Text, out d);
            settings.azPark = d;
        }

        private void azOutMax_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azOutMax.Text, out d);
            settings.azOutMax = d;
        }

        private void azOutMin_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azOutMin.Text, out d);
            settings.azOutMin = d;
        }

        private void azCBactiveHi_CheckedChanged(object sender, EventArgs e)
        {
            settings.azActiveHi = azCBactiveHi.Checked;
        }

        private void azCW_CCW_CheckedChanged(object sender, EventArgs e)
        {
            settings.azDriveType = azCW_CCW.Checked?driveType.CCW:(azEnbDir.Checked?driveType.DirEnable:driveType.Both);
            azCB1.Text = "CW";
            azCB2.Text = "CCW";
            azCB3.Text = "";
            azEnBit.Enabled = false;
        }

        private void azEnbDir_CheckedChanged(object sender, EventArgs e)
        {
            settings.azDriveType = azCW_CCW.Checked ? driveType.CCW : (azEnbDir.Checked ? driveType.DirEnable : driveType.Both);
            azCB1.Text = "Dir";
            azCB2.Text = "Enable";
            azCB3.Text = "";
            azEnBit.Enabled = false;
        }

        private void azBoth_CheckedChanged(object sender, EventArgs e)
        {
            settings.azDriveType = azCW_CCW.Checked ? driveType.CCW : (azEnbDir.Checked ? driveType.DirEnable : driveType.Both);
            azCB1.Text = "CW";
            azCB2.Text = "CCW";
            azCB3.Text = "Enable";
            azEnBit.Enabled = true;
        }



        private void elCWbit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.elCWbit.Text, out i);
            if (i < 0 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7");
                return;
            }
            settings.elCWbit = i;
        }

        private void elCCWbit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.elCCWbit.Text, out i);
            if (i < 0 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7");
                return;
            }
            settings.elCCWbit = i;

        }

        private void elPWMch_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.elPWMch.Text, out i);
            if (i < 0 || i > 1)
            {
                MessageBox.Show("PWM chan must be 0 or 1");
                return;
            }
            settings.elPWMchan = i;
        }

        private void elKp_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elKp.Text, out d);
            settings.elKp = d;
        }

        private void elKi_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elKi.Text, out d);
            settings.elKi = d;
        }

        private void elKd_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elKd.Text, out d);
            settings.elKd = d;
        }

        private void elMax_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elMax.Text, out d);
            settings.elMax = d;
        }

        private void elMin_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elMin.Text, out d);
            settings.elMin = d;
        }

        private void elPark_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elPark.Text, out d);
            settings.elPark = d;
        }

        private void elOutMax_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elOutMax.Text, out d);
            settings.elOutMax = d;
        }

        private void elOutMin_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elOutMin.Text, out d);
            settings.elOutMin = d;
        }

        private void elCBactiveHi_CheckedChanged(object sender, EventArgs e)
        {
            settings.elActiveHi = elCBactiveHi.Checked;
        }

        private void elCW_CCW_CheckedChanged(object sender, EventArgs e)
        {
            settings.elDriveType = elCW_CCW.Checked ? driveType.CCW : (elEnbDir.Checked ? driveType.DirEnable : driveType.Both);
            elCB1.Text = "CW";
            elCB2.Text = "CCW";
            elCB3.Text = "";
            elEnBit.Enabled = false;
        }

        private void elEnbDir_CheckedChanged(object sender, EventArgs e)
        {
            settings.elDriveType = elCW_CCW.Checked ? driveType.CCW : (elEnbDir.Checked ? driveType.DirEnable : driveType.Both);
            elCB1.Text = "Dir";
            elCB2.Text = "Enable";
            elCB3.Text = "";
            elEnBit.Enabled = false;
        }
        private void elBoth_CheckedChanged(object sender, EventArgs e)
        {
            settings.elDriveType = elCW_CCW.Checked ? driveType.CCW : (elEnbDir.Checked ? driveType.DirEnable : driveType.Both);
            elCB1.Text = "CW";
            elCB2.Text = "CCW";
            elCB3.Text = "Enable";
            elEnBit.Enabled = true;
        }


        private void elEnBit_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.elEnBit.Text, out i);
            if (i < -1 || i > 7)
            {
                MessageBox.Show("Bit must be between 0 and 7");
                return;
            }
            settings.elEnable = i;
        }

        private void alpha_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.alpha.Text, out d);
            settings.alpha = d;
        }

        private void PosLogFile_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Path.GetFullPath(this.PosLogFile.Text);
            }
            catch (PathTooLongException ex)
            {
                MessageBox.Show("File name is not valid : "+ex.Message);
                return;
            }
            settings.positionFileLog = this.PosLogFile.Text;
        }

        private void maxPosLogSize_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.maxPosLogSize.Text, out i);
            if (this.maxPosLogSize.Text.ToLower().Contains ("k"))
            {
                i *= 1024;
            }
            if (this.maxPosLogSize.Text.ToLower().Contains("m"))
            {
                i *= 1024*1024;
            }
            if (this.maxPosLogSize.Text.ToLower().Contains("g"))
            {
                i *= 1024 * 1024 * 1024;
            }

            if (i < 0 || i > 1*1024*1024*1024)
            {
                MessageBox.Show("File size must be greater than 0 and less than 1GB");
                return;
            }
            settings.maxPosLogSizeBytes = i;
        }

        private void MaxFiles_TextChanged(object sender, EventArgs e)
        {
            int i;
            Int32.TryParse(this.MaxFiles.Text, out i);
            if (i < 0 || i > 100)
            {
                MessageBox.Show("Number of files can range from 0 to 100");
                return;
            }
            settings.maxPosLogFiles = i;
        }

        private void PS1Name_TextChanged(object sender, EventArgs e)
        {
            settings.Preset1Name = PS1Name.Text;
        }

        private void PS1Az_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS1Az.Text, out d);
            settings.Preset1Az = d;
        }

        private void PS1El_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS1El.Text, out d);
            settings.Preset1El = d;
        }

        private void PS2Name_TextChanged(object sender, EventArgs e)
        {
            settings.Preset2Name = PS2Name.Text;
        }

        private void PS2Az_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS2Az.Text, out d);
            settings.Preset2Az = d;

        }

        private void PS2El_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS2El.Text, out d);
            settings.Preset2El = d;
        }

        private void PS3Name_TextChanged(object sender, EventArgs e)
        {
            settings.Preset3Name = PS3Name.Text;

        }

        private void PS3Az_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS3Az.Text, out d);
            settings.Preset3Az = d;

        }

        private void PS3El_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS3El.Text, out d);
            settings.Preset3El = d;
        }

        private void PS4Name_TextChanged(object sender, EventArgs e)
        {
            settings.Preset4Name = PS4Name.Text;
        }

        private void PS4Az_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS4Az.Text, out d);
            settings.Preset4Az = d;
        }

        private void PS4El_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS4El.Text, out d);
            settings.Preset4El = d;
        }

        private void PS5Name_TextChanged(object sender, EventArgs e)
        {
            settings.Preset5Name = PS5Name.Text;
        }

        private void PS5Az_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS5Az.Text, out d);
            settings.Preset5Az = d;

        }

        private void PS5El_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.PS5El.Text, out d);
            settings.Preset5El = d;
        }

        private void SouthParkPosEl_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.SouthParkPosEl.Text, out d);
            settings.elSouthPark = d;
        }

        private void SouthParkPosAz_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.SouthParkPosAz.Text, out d);
            settings.azSouthPark = d;
        }
    }
}
