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

        Graph g;
        
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

            g = new Graph(surface, canvas);
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

        public int GetWidth()
        {
            return this.image.Width;
        }

        public int GetHeight()
        {
            return this.image.Height;
        }

        public void DrawCross()
        {
            Bitmap bmp = this.image;
            int xm = bmp.Width;
            int ym = bmp.Height;
            this.DrawLine(new Point(0, ym), new Point(xm, 0));
            this.DrawLine(new Point(0, 0), new Point(xm, ym));
            /*
            for (int x = 0; x < xm; x += 1)
            {
                for (int y = 0; y < ym; y += 1)
                {
                    if (y - x == 0 || y + x == xm)
                    {
                        bmp.SetPixel(x, y, Color.Black);
                    }
                }
            }/* */
        }

        public void DrawLine(Point start, Point end)
        {
            // left and right points rather than start and end
            Point Left = start.X < end.X ? start : end;
            Point Right = start.X > end.X ? start : end;
            // xdiff is always positive or zero
            double xdiff = Right.X - Left.X;
            // ydiff is positive if up-slope; negative if down-slope; zero if even line
            double ydiff = Right.Y - Left.Y;
            // steepness of the slope
            double m = ydiff / xdiff;
            m = Math.Truncate(m * 10000) / 10000;
            double buffer = 0;
            // remember y to count it up/down through x loops
            int y = Left.Y;
            for(int x=Left.X; x < Right.X; x+=1)
            {
                buffer += m;
                if(Math.Abs(buffer) > 1)
                {
                    for(int yc=1; yc<Math.Abs(buffer); yc+=1)
                    {
                        this.SetPixel(x, y+(yc*Math.Sign(m)), Color.Black);
                    }
                    y += (int)Math.Truncate(buffer);
                    buffer = Math.Truncate((buffer % 1.0) * 10000) / 10000;
                } else
                {
                    this.SetPixel(x, y, Color.Black);
                }
            }
        }

        public void DrawLines(Point[] points, bool connect_ends=false)
        {
            if(points.Length >= 2)
            {
                Point current = points.First();
                for(int pc=1; pc < points.Length; pc+=1)
                {
                    Point next = points.ElementAt(pc);
                    this.DrawLine(current, next);
                    current = next;
                }
                if(connect_ends)
                {
                    this.DrawLine(points.Last(), points.First());
                }
            }
        }

        public void SetPixel(int x, int y, Color fill)
        {
            this.image.SetPixel(x, y, fill);
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
