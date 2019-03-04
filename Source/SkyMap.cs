using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DishControl
{
    public partial class SkyMap : Form
    {
        public SkyMap()
        {
            InitializeComponent();
        }

        private void Map_Paint(object sender, PaintEventArgs e)
        {
            Program.state.SkyVisible = true;

            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, g.ClipBounds);
        }

        private void SkyMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.state.SkyVisible = false;
        }
    }
}
