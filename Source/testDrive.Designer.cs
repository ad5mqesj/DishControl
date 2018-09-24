namespace DishControl
{
    partial class testDrive
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
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
            this.clos = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.azimuth = new System.Windows.Forms.Label();
            this.elevation = new System.Windows.Forms.Label();
            this.azVel = new System.Windows.Forms.TextBox();
            this.elVel = new System.Windows.Forms.TextBox();
            this.goAz = new System.Windows.Forms.Button();
            this.goEl = new System.Windows.Forms.Button();
            this.refresh = new System.Windows.Forms.Button();
            this.stop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clos
            // 
            this.clos.Location = new System.Drawing.Point(492, 322);
            this.clos.Name = "clos";
            this.clos.Size = new System.Drawing.Size(75, 23);
            this.clos.TabIndex = 0;
            this.clos.Text = "Close";
            this.clos.UseVisualStyleBackColor = true;
            this.clos.Click += new System.EventHandler(this.Clos_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Azimuth";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Elevation";
            // 
            // azimuth
            // 
            this.azimuth.AutoSize = true;
            this.azimuth.Location = new System.Drawing.Point(147, 30);
            this.azimuth.Name = "azimuth";
            this.azimuth.Size = new System.Drawing.Size(40, 13);
            this.azimuth.TabIndex = 3;
            this.azimuth.Text = "360.00";
            // 
            // elevation
            // 
            this.elevation.AutoSize = true;
            this.elevation.Location = new System.Drawing.Point(147, 104);
            this.elevation.Name = "elevation";
            this.elevation.Size = new System.Drawing.Size(40, 13);
            this.elevation.TabIndex = 4;
            this.elevation.Text = "360.00";
            // 
            // azVel
            // 
            this.azVel.Location = new System.Drawing.Point(226, 27);
            this.azVel.Name = "azVel";
            this.azVel.Size = new System.Drawing.Size(80, 20);
            this.azVel.TabIndex = 5;
            this.azVel.TextChanged += new System.EventHandler(this.azVel_TextChanged);
            // 
            // elVel
            // 
            this.elVel.Location = new System.Drawing.Point(226, 101);
            this.elVel.Name = "elVel";
            this.elVel.Size = new System.Drawing.Size(80, 20);
            this.elVel.TabIndex = 6;
            this.elVel.TextChanged += new System.EventHandler(this.elVel_TextChanged);
            // 
            // goAz
            // 
            this.goAz.Location = new System.Drawing.Point(342, 25);
            this.goAz.Name = "goAz";
            this.goAz.Size = new System.Drawing.Size(75, 23);
            this.goAz.TabIndex = 7;
            this.goAz.Text = "GO";
            this.goAz.UseVisualStyleBackColor = true;
            this.goAz.Click += new System.EventHandler(this.goAz_Click);
            // 
            // goEl
            // 
            this.goEl.Location = new System.Drawing.Point(342, 99);
            this.goEl.Name = "goEl";
            this.goEl.Size = new System.Drawing.Size(75, 23);
            this.goEl.TabIndex = 8;
            this.goEl.Text = "GO";
            this.goEl.UseVisualStyleBackColor = true;
            this.goEl.Click += new System.EventHandler(this.goEl_Click);
            // 
            // refresh
            // 
            this.refresh.Location = new System.Drawing.Point(130, 62);
            this.refresh.Name = "refresh";
            this.refresh.Size = new System.Drawing.Size(75, 23);
            this.refresh.TabIndex = 9;
            this.refresh.Text = "Refresh";
            this.refresh.UseVisualStyleBackColor = true;
            this.refresh.Click += new System.EventHandler(this.refresh_Click);
            // 
            // stop
            // 
            this.stop.Location = new System.Drawing.Point(342, 62);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(75, 23);
            this.stop.TabIndex = 10;
            this.stop.Text = "STOP";
            this.stop.UseVisualStyleBackColor = true;
            this.stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // testDrive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 357);
            this.Controls.Add(this.stop);
            this.Controls.Add(this.refresh);
            this.Controls.Add(this.goEl);
            this.Controls.Add(this.goAz);
            this.Controls.Add(this.elVel);
            this.Controls.Add(this.azVel);
            this.Controls.Add(this.elevation);
            this.Controls.Add(this.azimuth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clos);
            this.Name = "testDrive";
            this.Text = "Drive Test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button clos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label azimuth;
        private System.Windows.Forms.Label elevation;
        private System.Windows.Forms.TextBox azVel;
        private System.Windows.Forms.TextBox elVel;
        private System.Windows.Forms.Button goAz;
        private System.Windows.Forms.Button goEl;
        private System.Windows.Forms.Button refresh;
        private System.Windows.Forms.Button stop;
    }
}