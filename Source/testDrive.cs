using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    public partial class testDrive : Form
    {
        private Eth32 dev = null;
        public configModel settings = null;
        public MainForm form;
        System.Windows.Forms.Timer timer = null;

        private double azVelCmd = 0.0, elVelCmd = 0.0;
        private double azPos = 0.0, elPos = 0.0;

        public testDrive(Eth32 dev, configModel settings, MainForm main)
        {
            this.dev = dev;
            this.settings = settings;
            this.form = main;

            this.azPos = this.form.azReadPosition();
            this.elPos = this.form.elReadPosition();
            this.form.setAz(0.0);
            this.form.setEl(0.0);

            InitializeComponent();
            this.azimuth.Text =  String.Format("{0:0.00}",this.azPos);
            this.elevation.Text = String.Format("{0:0.00}", this.elPos);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Tick += new EventHandler(timer1_Tick);

        }


        private void azVel_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.azVel.Text, out d);
            if (d < -10.0)
            {
                MessageBox.Show("Velcity command must be between -1 and 1");
                this.azVelCmd = 0.0;
                return;
            }
            this.azVelCmd = d/10.0;
        }

        private void elVel_TextChanged(object sender, EventArgs e)
        {
            double d;
            double.TryParse(this.elVel.Text, out d);
            if (d < -10.0)
            {
                MessageBox.Show("Velcity command must be between -1 and 1");
                this.elVelCmd = 0.0;
                return;
            }
            this.elVelCmd = d/10.0;
        }

        private void goEl_Click(object sender, EventArgs e)
        {
            timer.Start();
            this.form.setEl(this.elVelCmd);
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (timer != null && timer.Enabled)
                timer.Stop();
            this.form.setAz(0.0);
            this.form.setEl(0.0);
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            this.azimuth.Text = String.Format("0:0.00", this.azPos);
            this.elevation.Text = String.Format("0:0.00", this.elPos);
        }

        private void Clos_Click(object sender, EventArgs e)
        {
            if (timer != null && timer.Enabled)
                timer.Stop();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void goAz_Click(object sender, EventArgs e)
        {
            timer.Start();
            this.form.setAz(this.azVelCmd);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.azimuth.Text = String.Format("0:0.00", this.form.azPos);
            this.elevation.Text = String.Format("0:0.00", this.form.elPos);
        }

    }
}
