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

        Graph TestGraph { get; set; }
        ValueTable ResultTable { get; set; }

        /// <summary>
        /// Form-Objekt, welches die zu testenden Programm-Bausteine beinhaltet
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            mainCon = new SplitContainer
            {
                Parent = this,
                IsSplitterFixed = true,
                Dock = DockStyle.Fill,

                //Feste Max/Min Werte, um Anzeige des Inhalts auf der rechten Seite zu gewährleisten
                //Panel1MinSize = Convert.ToInt32(Math.Round(Size.Width * 0.3, 0)),
                //Panel2MinSize = Convert.ToInt32(Math.Round(Size.Width * 0.3, 0))
            };
            subCon = new SplitContainer
            {
                Parent = mainCon.Panel2,
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            TestGraph = new Graph();

            Button btnTransparency = new Button
            {
                Text = "Toggle transparent Labels",
                Width = 200
            };
            btnTransparency.Click += Click_btnTransparency;
            Button btnRenderTable = new Button
            {
                Text = "Render data",
                Width = 150
            };
            btnRenderTable.Click += Click_btnRenderTable;

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

            xLow.Value = (decimal)TestGraph.HorizontalAxis.Item1;
            xHigh.Value = (decimal)TestGraph.HorizontalAxis.Item2;
            yLow.Value = (decimal)TestGraph.VerticalAxis.Item1;
            yHigh.Value = (decimal)TestGraph.VerticalAxis.Item2;

            xLow.ValueChanged += NumUpDown_ValueChanged;
            xHigh.ValueChanged += NumUpDown_ValueChanged;
            yLow.ValueChanged += NumUpDown_ValueChanged;
            yHigh.ValueChanged += NumUpDown_ValueChanged;

            AddChild(subCon.Panel1, btnTransparency, 10, 10);
            AddChild(subCon.Panel1, btnRenderTable, 10, (10 + btnTransparency.Width) + 5);

            AddChild(subCon.Panel1, xLabel, 50, 10);
            AddChild(subCon.Panel1, xLow, 80, 10);
            AddChild(subCon.Panel1, xHigh, 80, 150);

            AddChild(subCon.Panel1, yLabel, 120, 10);
            AddChild(subCon.Panel1, yLow, 150, 10);
            AddChild(subCon.Panel1, yHigh, 150, 150);
            ResizeBegin += (object s, EventArgs e) => { TestGraph.AutoResize = false; };
            ResizeEnd += (object s, EventArgs e) => { TestGraph.AutoResize = true; TestGraph.ForceResize(); };

            ClientSizeChanged += (object s, EventArgs e) => { if (previousWindowState != WindowState) { TestGraph.ForceResize(); } };

            previousWindowState = WindowState;

            //Integrate DataGridView, DataSet including DataTables
            ResultTable = new ValueTable();
            AddChild(subCon.Panel2, ResultTable.MyDataGridView);
        }

        private void Click_btnRenderTable(object sender, EventArgs e)
        {
            TestGraph.ClearLineMemory();
            double[,] data = ResultTable.GetTableContents();
            int columnCount = data.GetLength(0);
            int rowCount = data.GetLength(1);
            // first column are X-values
            // loop through other columns for Y-values
            for (int column=1; column<columnCount; column+=1)
            {
                for (int row = 0; row < rowCount; row += 1)
                {
                    //debugging output
                    //System.Diagnostics.Debug.Print(row.ToString() + ": " + data[column, row].ToString() + " ");
                    TestGraph.DrawMark(new PointF((float)data[0, row], (float)data[column, row]), ResultTable.ColumnColors[column], false);
                }
                //System.Diagnostics.Debug.Print("----");
            }
            TestGraph.ForceResize();
            TestGraph.DrawPolynomialFunction(ValueTable.AmpereFunction, ResultTable.ColumnColors[1]);
            TestGraph.DrawPolynomialFunction(ValueTable.VoltageFunction, ResultTable.ColumnColors[2]);
            TestGraph.DrawPolynomialFunction(ValueTable.PowerFunction, ResultTable.ColumnColors[3]);
        }

        private void NumUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestGraph.HorizontalAxis = new Tuple<float, float>((float)xLow.Value, (float)xHigh.Value);
            TestGraph.VerticalAxis = new Tuple<float, float>((float)yLow.Value, (float)yHigh.Value);
        }

        private void Click_btnTransparency(object sender, EventArgs e)
        {
            TestGraph.ToggleLabelTransparency();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Bei Start Splitter in die Mitte setzen
            mainCon.SplitterDistance = Convert.ToInt32(Math.Round(Width * 0.5, 0));
            TestGraph.SetParent(mainCon.Panel1);
            TestGraph.RememberDrawing = true;
            
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

        private void DrawTestImage()
        {
            PointF[] ps = new PointF[5];
            ps[0] = new PointF(10f, 50f);
            ps[1] = new PointF(80f, 70f);
            ps[2] = new PointF(25f, 80f);
            ps[3] = new PointF(50f, 35f);
            ps[4] = new PointF(65f, 95f);
            TestGraph.DrawLines(ps, TestGraph.ForegroundColor, true);

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
                TestGraph.DrawCurve(lps[0], lps.GetRange(1, lps.Count - 1), nextC[(int)counter % 3], 10, lc);
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
                TestGraph.DrawCurve(pie[0], pie.GetRange(1, pie.Count - 1), nextC[(int)counter % 3], 10, lc);
                counter += 1;
            }

            QuadPolynomial simpleQuadratic = new QuadPolynomial
            {
                two = (_) => 2.0f,
                one = (_) => 0.0f,
                zero = (_) => 3.0f
            };
            QuadPolynomial simpleLinear = new QuadPolynomial
            {
                two = (_) => 0.0f,
                one = (_) => 0.5f,
                zero = (_) => 4.0f
            };

            TestGraph.DrawRationalFunction(simpleLinear, simpleQuadratic, Color.Black);
        }

    }
}
