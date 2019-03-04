using WinfordEthIO;

namespace DishControl
{
    partial class MainForm
    {

         private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.connect = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.driveTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rADegreesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSkyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.RA = new System.Windows.Forms.Label();
            this.DEC = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Elevation = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Azimuth = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SouthPark = new System.Windows.Forms.Button();
            this.presetSelector = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lunarTrack = new System.Windows.Forms.Button();
            this.track = new System.Windows.Forms.CheckBox();
            this.STOP = new System.Windows.Forms.Button();
            this.Park = new System.Windows.Forms.Button();
            this.GO = new System.Windows.Forms.Button();
            this.commandEl = new System.Windows.Forms.TextBox();
            this.commandAz = new System.Windows.Forms.TextBox();
            this.commandDec = new System.Windows.Forms.TextBox();
            this.commandRA = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.trackingIcon = new System.Windows.Forms.PictureBox();
            this.label9 = new System.Windows.Forms.Label();
            this.movingIcon = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.connectedIcon = new System.Windows.Forms.PictureBox();
            this.Up = new System.Windows.Forms.Button();
            this.CW = new System.Windows.Forms.Button();
            this.CCW = new System.Windows.Forms.Button();
            this.Down = new System.Windows.Forms.Button();
            this.Message = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackingIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.movingIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectedIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(194, 17);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(75, 23);
            this.connect.TabIndex = 3;
            this.connect.Text = "Reconnect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 370);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(744, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.driveTestToolStripMenuItem,
            this.displayToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(744, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // driveTestToolStripMenuItem
            // 
            this.driveTestToolStripMenuItem.Name = "driveTestToolStripMenuItem";
            this.driveTestToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.driveTestToolStripMenuItem.Text = "Drive Test";
            this.driveTestToolStripMenuItem.Visible = false;
            this.driveTestToolStripMenuItem.Click += new System.EventHandler(this.driveTestToolStripMenuItem_Click);
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showSecondsToolStripMenuItem,
            this.rADegreesToolStripMenuItem,
            this.showSkyToolStripMenuItem});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.displayToolStripMenuItem.Text = "Display";
            // 
            // showSecondsToolStripMenuItem
            // 
            this.showSecondsToolStripMenuItem.Name = "showSecondsToolStripMenuItem";
            this.showSecondsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showSecondsToolStripMenuItem.Text = "Show Seconds";
            this.showSecondsToolStripMenuItem.Click += new System.EventHandler(this.ShowSecondsToolStripMenuItem_Click);
            // 
            // rADegreesToolStripMenuItem
            // 
            this.rADegreesToolStripMenuItem.Name = "rADegreesToolStripMenuItem";
            this.rADegreesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.rADegreesToolStripMenuItem.Text = "RA Degrees";
            this.rADegreesToolStripMenuItem.Click += new System.EventHandler(this.RADegreesToolStripMenuItem_Click);
            // 
            // showSkyToolStripMenuItem
            // 
            this.showSkyToolStripMenuItem.Name = "showSkyToolStripMenuItem";
            this.showSkyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showSkyToolStripMenuItem.Text = "Show Sky";
            this.showSkyToolStripMenuItem.Click += new System.EventHandler(this.ShowSkyToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 26);
            this.label1.TabIndex = 8;
            this.label1.Text = "R.A.";
            // 
            // RA
            // 
            this.RA.AutoSize = true;
            this.RA.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RA.ForeColor = System.Drawing.Color.Blue;
            this.RA.Location = new System.Drawing.Point(59, 24);
            this.RA.Name = "RA";
            this.RA.Size = new System.Drawing.Size(114, 26);
            this.RA.TabIndex = 9;
            this.RA.Text = "12:32:12.1";
            // 
            // DEC
            // 
            this.DEC.AutoSize = true;
            this.DEC.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DEC.ForeColor = System.Drawing.Color.Blue;
            this.DEC.Location = new System.Drawing.Point(269, 24);
            this.DEC.Name = "DEC";
            this.DEC.Size = new System.Drawing.Size(114, 26);
            this.DEC.TabIndex = 11;
            this.DEC.Text = "44:23:11.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Tai Le", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(208, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 29);
            this.label3.TabIndex = 10;
            this.label3.Text = "DEC";
            // 
            // Elevation
            // 
            this.Elevation.AutoSize = true;
            this.Elevation.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Elevation.ForeColor = System.Drawing.Color.Blue;
            this.Elevation.Location = new System.Drawing.Point(269, 59);
            this.Elevation.Name = "Elevation";
            this.Elevation.Size = new System.Drawing.Size(66, 26);
            this.Elevation.TabIndex = 15;
            this.Elevation.Text = "23:10";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Tai Le", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(208, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 29);
            this.label4.TabIndex = 14;
            this.label4.Text = "El";
            // 
            // Azimuth
            // 
            this.Azimuth.AutoSize = true;
            this.Azimuth.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Azimuth.ForeColor = System.Drawing.Color.Blue;
            this.Azimuth.Location = new System.Drawing.Point(59, 59);
            this.Azimuth.Name = "Azimuth";
            this.Azimuth.Size = new System.Drawing.Size(78, 26);
            this.Azimuth.TabIndex = 13;
            this.Azimuth.Text = "241:14";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(5, 59);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 26);
            this.label6.TabIndex = 12;
            this.label6.Text = "Az";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Elevation);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.RA);
            this.groupBox1.Controls.Add(this.Azimuth);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.DEC);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(414, 102);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Actual Position";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.SouthPark);
            this.groupBox2.Controls.Add(this.presetSelector);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.lunarTrack);
            this.groupBox2.Controls.Add(this.track);
            this.groupBox2.Controls.Add(this.STOP);
            this.groupBox2.Controls.Add(this.Park);
            this.groupBox2.Controls.Add(this.GO);
            this.groupBox2.Controls.Add(this.commandEl);
            this.groupBox2.Controls.Add(this.commandAz);
            this.groupBox2.Controls.Add(this.commandDec);
            this.groupBox2.Controls.Add(this.commandRA);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Location = new System.Drawing.Point(12, 151);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(390, 211);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Command Position";
            // 
            // SouthPark
            // 
            this.SouthPark.Location = new System.Drawing.Point(98, 178);
            this.SouthPark.Name = "SouthPark";
            this.SouthPark.Size = new System.Drawing.Size(75, 23);
            this.SouthPark.TabIndex = 26;
            this.SouthPark.Text = "South Park";
            this.SouthPark.UseVisualStyleBackColor = true;
            this.SouthPark.Click += new System.EventHandler(this.SouthPark_Click);
            // 
            // presetSelector
            // 
            this.presetSelector.AllowDrop = true;
            this.presetSelector.FormattingEnabled = true;
            this.presetSelector.Location = new System.Drawing.Point(64, 69);
            this.presetSelector.Name = "presetSelector";
            this.presetSelector.Size = new System.Drawing.Size(191, 21);
            this.presetSelector.TabIndex = 25;
            this.presetSelector.SelectedIndexChanged += new System.EventHandler(this.presetSelector_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 17);
            this.label2.TabIndex = 24;
            this.label2.Text = "Preset";
            // 
            // lunarTrack
            // 
            this.lunarTrack.Location = new System.Drawing.Point(156, 149);
            this.lunarTrack.Name = "lunarTrack";
            this.lunarTrack.Size = new System.Drawing.Size(75, 23);
            this.lunarTrack.TabIndex = 23;
            this.lunarTrack.Text = "Track Moon";
            this.lunarTrack.UseVisualStyleBackColor = true;
            this.lunarTrack.Click += new System.EventHandler(this.lunarTrack_Click);
            // 
            // track
            // 
            this.track.AutoSize = true;
            this.track.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.track.Location = new System.Drawing.Point(170, 28);
            this.track.Name = "track";
            this.track.Size = new System.Drawing.Size(61, 19);
            this.track.TabIndex = 22;
            this.track.Text = "Track";
            this.track.UseVisualStyleBackColor = true;
            this.track.CheckedChanged += new System.EventHandler(this.track_CheckedChanged);
            // 
            // STOP
            // 
            this.STOP.Location = new System.Drawing.Point(299, 178);
            this.STOP.Name = "STOP";
            this.STOP.Size = new System.Drawing.Size(75, 23);
            this.STOP.TabIndex = 21;
            this.STOP.Text = "STOP";
            this.STOP.UseVisualStyleBackColor = true;
            this.STOP.Click += new System.EventHandler(this.STOP_Click);
            // 
            // Park
            // 
            this.Park.Location = new System.Drawing.Point(210, 178);
            this.Park.Name = "Park";
            this.Park.Size = new System.Drawing.Size(75, 23);
            this.Park.TabIndex = 20;
            this.Park.Text = "Maint. Park";
            this.Park.UseVisualStyleBackColor = true;
            this.Park.Click += new System.EventHandler(this.Park_Click);
            // 
            // GO
            // 
            this.GO.Location = new System.Drawing.Point(10, 178);
            this.GO.Name = "GO";
            this.GO.Size = new System.Drawing.Size(75, 23);
            this.GO.TabIndex = 19;
            this.GO.Text = "Go";
            this.GO.UseVisualStyleBackColor = true;
            this.GO.Click += new System.EventHandler(this.GO_Click);
            // 
            // commandEl
            // 
            this.commandEl.Location = new System.Drawing.Point(283, 112);
            this.commandEl.Name = "commandEl";
            this.commandEl.Size = new System.Drawing.Size(100, 20);
            this.commandEl.TabIndex = 18;
            this.commandEl.TextChanged += new System.EventHandler(this.commandEl_TextChanged);
            // 
            // commandAz
            // 
            this.commandAz.Location = new System.Drawing.Point(64, 112);
            this.commandAz.Name = "commandAz";
            this.commandAz.Size = new System.Drawing.Size(100, 20);
            this.commandAz.TabIndex = 17;
            this.commandAz.TextChanged += new System.EventHandler(this.commandAz_TextChanged);
            // 
            // commandDec
            // 
            this.commandDec.Location = new System.Drawing.Point(283, 28);
            this.commandDec.Name = "commandDec";
            this.commandDec.Size = new System.Drawing.Size(100, 20);
            this.commandDec.TabIndex = 16;
            this.commandDec.TextChanged += new System.EventHandler(this.commandDec_TextChanged);
            // 
            // commandRA
            // 
            this.commandRA.Location = new System.Drawing.Point(64, 30);
            this.commandRA.Name = "commandRA";
            this.commandRA.Size = new System.Drawing.Size(100, 20);
            this.commandRA.TabIndex = 15;
            this.commandRA.TextChanged += new System.EventHandler(this.commandRA_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(5, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 26);
            this.label7.TabIndex = 8;
            this.label7.Text = "R.A.";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Tai Le", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(230, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 29);
            this.label8.TabIndex = 14;
            this.label8.Text = "El";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Tai Le", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(230, 24);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(55, 29);
            this.label11.TabIndex = 10;
            this.label11.Text = "DEC";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(6, 106);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(40, 26);
            this.label12.TabIndex = 12;
            this.label12.Text = "Az";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.trackingIcon);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.movingIcon);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.connectedIcon);
            this.groupBox3.Controls.Add(this.connect);
            this.groupBox3.Location = new System.Drawing.Point(432, 29);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(291, 100);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Status";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(28, 70);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(123, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "Tracking (Blue: tracking)";
            // 
            // trackingIcon
            // 
            this.trackingIcon.BackColor = System.Drawing.SystemColors.Info;
            this.trackingIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.trackingIcon.Location = new System.Drawing.Point(6, 67);
            this.trackingIcon.Name = "trackingIcon";
            this.trackingIcon.Size = new System.Drawing.Size(16, 17);
            this.trackingIcon.TabIndex = 6;
            this.trackingIcon.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(28, 45);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(253, 13);
            this.label9.TabIndex = 5;
            this.label9.Text = "Moving (Red: moving, Green: safe, Yellow: unkown)";
            // 
            // movingIcon
            // 
            this.movingIcon.BackColor = System.Drawing.SystemColors.Info;
            this.movingIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.movingIcon.Location = new System.Drawing.Point(6, 42);
            this.movingIcon.Name = "movingIcon";
            this.movingIcon.Size = new System.Drawing.Size(16, 17);
            this.movingIcon.TabIndex = 4;
            this.movingIcon.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(154, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Connected (Green: connected)";
            // 
            // connectedIcon
            // 
            this.connectedIcon.BackColor = System.Drawing.SystemColors.Info;
            this.connectedIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.connectedIcon.Location = new System.Drawing.Point(6, 19);
            this.connectedIcon.Name = "connectedIcon";
            this.connectedIcon.Size = new System.Drawing.Size(16, 17);
            this.connectedIcon.TabIndex = 0;
            this.connectedIcon.TabStop = false;
            // 
            // Up
            // 
            this.Up.AccessibleDescription = "Elevation Up";
            this.Up.Image = ((System.Drawing.Image)(resources.GetObject("Up.Image")));
            this.Up.Location = new System.Drawing.Point(527, 208);
            this.Up.Name = "Up";
            this.Up.Size = new System.Drawing.Size(56, 41);
            this.Up.TabIndex = 19;
            this.Up.UseVisualStyleBackColor = true;
            this.Up.Click += new System.EventHandler(this.Up_Click);
            // 
            // CW
            // 
            this.CW.AccessibleDescription = "Azimuth clockwise";
            this.CW.Image = ((System.Drawing.Image)(resources.GetObject("CW.Image")));
            this.CW.Location = new System.Drawing.Point(485, 255);
            this.CW.Name = "CW";
            this.CW.Size = new System.Drawing.Size(42, 56);
            this.CW.TabIndex = 20;
            this.CW.UseVisualStyleBackColor = true;
            this.CW.Click += new System.EventHandler(this.CW_Click);
            // 
            // CCW
            // 
            this.CCW.AccessibleDescription = "Azimuth counter-clockwise";
            this.CCW.Image = ((System.Drawing.Image)(resources.GetObject("CCW.Image")));
            this.CCW.Location = new System.Drawing.Point(581, 255);
            this.CCW.Name = "CCW";
            this.CCW.Size = new System.Drawing.Size(42, 56);
            this.CCW.TabIndex = 21;
            this.CCW.UseVisualStyleBackColor = true;
            this.CCW.Click += new System.EventHandler(this.CCW_Click);
            // 
            // Down
            // 
            this.Down.AccessibleDescription = "Elevation Down";
            this.Down.Image = ((System.Drawing.Image)(resources.GetObject("Down.Image")));
            this.Down.Location = new System.Drawing.Point(527, 317);
            this.Down.Name = "Down";
            this.Down.Size = new System.Drawing.Size(56, 42);
            this.Down.TabIndex = 22;
            this.Down.UseVisualStyleBackColor = true;
            this.Down.Click += new System.EventHandler(this.Down_Click);
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.ForeColor = System.Drawing.Color.Red;
            this.Message.Location = new System.Drawing.Point(129, 135);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(0, 13);
            this.Message.TabIndex = 23;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 392);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.Down);
            this.Controls.Add(this.CCW);
            this.Controls.Add(this.CW);
            this.Controls.Add(this.Up);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Plishner Dish";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackingIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.movingIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectedIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label RA;
        private System.Windows.Forms.Label DEC;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Elevation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label Azimuth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox track;
        private System.Windows.Forms.Button STOP;
        private System.Windows.Forms.Button Park;
        private System.Windows.Forms.Button GO;
        private System.Windows.Forms.TextBox commandEl;
        private System.Windows.Forms.TextBox commandAz;
        private System.Windows.Forms.TextBox commandDec;
        private System.Windows.Forms.TextBox commandRA;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox trackingIcon;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox movingIcon;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox connectedIcon;
        private System.Windows.Forms.Button lunarTrack;
        private System.Windows.Forms.Button Up;
        private System.Windows.Forms.Button CW;
        private System.Windows.Forms.Button CCW;
        private System.Windows.Forms.Button Down;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.ToolStripMenuItem driveTestToolStripMenuItem;
        private System.Windows.Forms.ComboBox presetSelector;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button SouthPark;
        private System.Windows.Forms.ToolStripMenuItem displayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rADegreesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSkyToolStripMenuItem;
    }
}

