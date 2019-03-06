using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace canvas_test
{

    public partial class Form1 : Form
    {

        SplitContainer mainCon;
        PictureBox surface;
        Bitmap canvas;

        public Graph g;
        
        public Form1()
        {
            InitializeComponent();
            mainCon = new SplitContainer
            {
                Parent = this,
                Dock = DockStyle.Fill,

                //Feste Max/Min Werte, um Anzeige des Inhalts auf der rechten Seite zu gewährleisten
                Panel1MinSize = Convert.ToInt32(Math.Round(this.Size.Width * 0.3, 0)),
                Panel2MinSize = Convert.ToInt32(Math.Round(this.Size.Width * 0.3, 0))
            };

            surface = new PictureBox
            {
                Parent = mainCon.Panel1,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
            };

            canvas = new Bitmap(mainCon.Panel1.Width, mainCon.Panel1.Height);

            g = new Graph();
            //g.DrawCross();
            Point[] ps = new Point[5];
            ps[0] = new Point(10, 50);
            ps[1] = new Point(80, 70);
            ps[2] = new Point(25, 80);
            ps[3] = new Point(50, 35);
            ps[4] = new Point(65, 95);
            g.DrawLines( ps, true);

            Button btnTr = new Button
            {
                Text = "Toggle transparent Labels",
                Parent = mainCon.Panel2,
                Top = 10,
                Left = 10,
            };
            btnTr.Click += Click_btnTr;
        }

        private void Click_btnTr(object sender, EventArgs e)
        {
            GetGraph().ToggleLabelTransparency();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Bei Start Splitter in die Mitte setzen
            mainCon.SplitterDistance = Convert.ToInt32(Math.Round(this.Width * 0.5, 0));
        }

        public Graph GetGraph()
        {
            return this.g;
        }
        
    }
}
