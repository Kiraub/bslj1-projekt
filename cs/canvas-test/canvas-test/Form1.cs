using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace canvas_test
{

    public partial class Form1 : Form
    {

        SplitContainer mainCon;
        SplitContainer subCon;

        NumericUpDown xLow, xHigh, yLow, yHigh;

        FormWindowState previousWindowState;

        public Graph g;

        public Form1()
        {
            InitializeComponent();
            mainCon = new SplitContainer
            {
                Parent = this,
                Dock = DockStyle.Fill,

                //Feste Max/Min Werte, um Anzeige des Inhalts auf der rechten Seite zu gewährleisten
                Panel1MinSize = Convert.ToInt32(Math.Round(Size.Width * 0.3, 0)),
                Panel2MinSize = Convert.ToInt32(Math.Round(Size.Width * 0.3, 0))
            };
            subCon = new SplitContainer
            {
                Parent = mainCon.Panel2,
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            g = new Graph();

            Button btnTr = new Button
            {
                Text = "Toggle transparent Labels",
                Width = 200
            };
            btnTr.Click += Click_btnTr;

            // experimental UI for testing purposes

            Label xLabel = new Label
            {
                Text = "X-Axis Limits",
                Width = 150,
                BackColor = Color.Transparent
            };
            Label yLabel = new Label
            {
                Text = "Y-Axis Limits",
                Width = 150,
                BackColor = Color.Transparent
            };
            xLow = new NumericUpDown();
            IncreaseRange(xLow);
            xHigh = new NumericUpDown();
            IncreaseRange(xHigh);
            yLow = new NumericUpDown();
            IncreaseRange(yLow);
            yHigh = new NumericUpDown();
            IncreaseRange(yHigh);

            xLow.Value = (decimal)g.HorizontalAxis.Item1;
            xHigh.Value = (decimal)g.HorizontalAxis.Item2;
            yLow.Value = (decimal)g.VerticalAxis.Item1;
            yHigh.Value = (decimal)g.VerticalAxis.Item2;

            xLow.ValueChanged += NumUpDown_ValueChanged;
            xHigh.ValueChanged += NumUpDown_ValueChanged;
            yLow.ValueChanged += NumUpDown_ValueChanged;
            yHigh.ValueChanged += NumUpDown_ValueChanged;

            AddChild(subCon.Panel1, btnTr, 10, 10);

            AddChild(subCon.Panel1, xLabel, 50, 10);
            AddChild(subCon.Panel1, xLow, 80, 10);
            AddChild(subCon.Panel1, xHigh, 80, 150);

            AddChild(subCon.Panel1, yLabel, 120, 10);
            AddChild(subCon.Panel1, yLow, 150, 10);
            AddChild(subCon.Panel1, yHigh, 150, 150);
            ResizeBegin += (object s, EventArgs e) => { g.AutoResize = false; };
            ResizeEnd += (object s, EventArgs e) => { g.AutoResize = true; g.ForceResize(); };
            ClientSizeChanged += (object s, EventArgs e) => { if (previousWindowState != WindowState) { g.ForceResize(); } };

            previousWindowState = WindowState;

            //Integrate DataGrid, DataSet including DataTables
            ValueTable ResultTable = new ValueTable();
            AddChild(subCon.Panel2, ResultTable.myDataGrid);
            //Controls.Add(ResultTable.addDataGrid());
            ResultTable.addDataSet(6);
        }

        private void NumUpDown_ValueChanged(object sender, EventArgs e)
        {
            g.HorizontalAxis = new Tuple<float, float>((float)xLow.Value, (float)xHigh.Value);
            g.VerticalAxis = new Tuple<float, float>((float)yLow.Value, (float)yHigh.Value);
        }

        private void Click_btnTr(object sender, EventArgs e)
        {
            GetGraph().ToggleLabelTransparency();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Bei Start Splitter in die Mitte setzen
            mainCon.SplitterDistance = Convert.ToInt32(Math.Round(this.Width * 0.5, 0));
            g.SetParent(mainCon.Panel1);
            g.RememberDrawing = true;
            PointF[] ps = new PointF[5];
            ps[0] = new PointF(10f, 50f);
            ps[1] = new PointF(80f, 70f);
            ps[2] = new PointF(25f, 80f);
            ps[3] = new PointF(50f, 35f);
            ps[4] = new PointF(65f, 95f);
            g.DrawLines(ps, g.ForegroundColor, true);

            List<PointF> lps = new List<PointF>();
            lps.Add(new PointF(0f, 100f));
            lps.Add(new PointF(10f, 50f));
            lps.Add(new PointF(20f, 25f));
            lps.Add(new PointF(30f, 12.5f));
            lps.Add(new PointF(40f, 6.25f));
            lps.Add(new PointF(50f, 3.125f));
            lps.Add(new PointF(60f, 1.5625f));
            lps.Add(new PointF(70f, 0.78625f));
            Color[] nextC = { Color.Red, Color.Blue, Color.Green };
            int counter = 0;
            for (float lc = -0.8f; lc < 0.9f; lc += 0.2f)
            {
                g.DrawCurve(lps[0], lps.GetRange(1, lps.Count - 1), nextC[(int)counter % 3], 10, lc);
                counter += 1;
            }
            float pi = (float)Math.PI;
            List<PointF> pie = new List<PointF>();
            float[] nextY = { 0f, 1f, 0f, -1f };
            counter = 0;
            for (float pic = 0f; pic < 100f; pic += pi)
            {
                pie.Add(new PointF(pic, nextY[counter % 4]));
                counter += 1;
            }
            for (float lc = 0.5f; lc < 0.7f; lc += 0.2f)
            {
                g.DrawCurve(pie[0], pie.GetRange(1, pie.Count - 1), nextC[(int)counter % 3], 10, lc);
                counter += 1;
            }
        }

        private void AddChild(Control parent, Control child, int top = 10, int left = 10)
        {
            child.Parent = parent;
            child.Top = top;
            child.Left = left;
        }

        private void IncreaseRange(NumericUpDown numUpDown)
        {
            numUpDown.Maximum = 500.0m;
            numUpDown.Minimum = -500.0m;
        }

        public Graph GetGraph()
        {
            return g;
        }

    }
}
