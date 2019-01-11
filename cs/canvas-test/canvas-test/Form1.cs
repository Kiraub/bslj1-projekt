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

        SplitContainer spc1;
        PictureBox pcb1;
        Bitmap bmp1;

        Graph g;
        
        public Form1()
        {
            InitializeComponent();
            spc1 = new SplitContainer
            {
                Parent = this,
                Dock = DockStyle.Fill
            };

            pcb1 = new PictureBox
            {
                Parent = spc1.Panel1,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            bmp1 = new Bitmap(spc1.Panel1.Width, spc1.Panel1.Height);

            g = new Graph(pcb1, bmp1);
            g.DrawCross();

            Button btnTr = new Button
            {
                Text = "Toggle transparent Labels",
                Parent = spc1.Panel2,
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
            
        }

        public Graph GetGraph()
        {
            return g;
        }
        
    }

    public class Graph
    {
        public static int DEFAULT_WIDTH = 100;
        public static int DEFAULT_HEIGHT = 100;

        private Color control;

        private PictureBox imageBox;
        private Bitmap image;
        private Label labelX;
        private Label labelY;
        private bool transparent;

        public Graph() : this(new PictureBox())
        {

        }

        public Graph(PictureBox p) : this(p, new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {

        }

        public Graph(PictureBox p, Bitmap b)
        {
            this.imageBox = p;
            this.image = b;
            this.imageBox.Image = this.image;

            this.labelX = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "X Achse",
                Parent = this.imageBox,
                Top = 10,
                Left = 10
            };

            this.labelY = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Y Achse",
                Parent = this.imageBox
            };
            this.labelY.Top = this.imageBox.Height - this.labelY.Height - 20;
            this.labelY.Left = this.imageBox.Width - this.labelY.Width - 20;

            this.control = this.labelX.BackColor;
            this.transparent = false;

        }

        public void DrawCross()
        {
            Bitmap bmp = this.image;
            int xm = bmp.Width;
            int ym = bmp.Height;
            for (int x = 0; x < xm; x += 1)
            {
                for (int y = 0; y < ym; y += 1)
                {
                    if (y - x == 0 || y + x == xm)
                    {
                        bmp.SetPixel(x, y, Color.Black);
                    }
                }
            }
        }

        public void SetLabelTransparency(bool transparent)
        {
            if(transparent)
            {
                this.labelX.BackColor = Color.Transparent;
                this.labelY.BackColor = Color.Transparent;
            } else
            {
                this.labelX.BackColor = this.control;
                this.labelY.BackColor = this.control;
            }
        }

        public void ToggleLabelTransparency()
        {
            this.transparent = !this.transparent;
            this.SetLabelTransparency(this.transparent);
        }
    }
}
