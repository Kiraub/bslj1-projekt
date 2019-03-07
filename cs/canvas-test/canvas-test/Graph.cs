﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

using ImageCoord = System.Drawing.Point;
using GraphCoord = System.Drawing.PointF;
using SimpleLine = System.Tuple<System.Drawing.PointF, System.Drawing.PointF, System.Drawing.Color>;


namespace canvas_test
{
    /// <summary>
    /// Repräsentiert die visuelle Schnittstelle um Graphen zu zeichnen
    /// <para>Beinhaltet sowohl Bild-/ als auch Geometrieobjekte</para>
    /// </summary>
    public class Graph
    {

        #region public instance fields

        public static int DEFAULT_WIDTH = 100;
        public static int DEFAULT_HEIGHT = 100;
        
        #region graph render options
        //TODO separate render options into own options class
        public bool TransparentLabels { get; set; } = false;
        public bool AutoResize { get; set; } = true;
        public bool RememberDrawing { get; set; } = false;
        #endregion
        public Label LblY { get; set; }
        public Label LblX { get; set; }
        public List<Label> LblLegende { get; set; }

        public Bitmap DrawArea { get; set; }
        public PictureBox DrawAreaContainer { get; set; }

        public Color[] VisualColors { get; set; }
        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }

        #endregion

        #region private instance fields

        private int ImageWidth => DrawArea.Width;
        private int ImageHeight => DrawArea.Height;
        private GraphProperties Geometry { get; set; }
        private List<SimpleLine> LineMemory { get; set; }

        #endregion

        #region constructors

        public Graph()
        {
            DrawAreaContainer = new PictureBox();
            DrawArea = new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            DrawAreaContainer.Image = DrawArea;
            DrawAreaContainer.BorderStyle = BorderStyle.FixedSingle;
            //DrawAreaContainer.BackColor = Color.AliceBlue;

            LblX = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "X Achse",
                Parent = DrawAreaContainer
            };

            LblY = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Y Achse",
                Parent = DrawAreaContainer
            };

            LblLegende = new List<Label>();
            LineMemory = new List<SimpleLine>();

            ForegroundColor = Color.Black;
            BackgroundColor = LblX.BackColor;
            TransparentLabels = false;

            Geometry = new GraphProperties();
            Geometry.SetLimitsX(-10f, 100f);
            Geometry.SetLimitsY(-10f, 100f);
        }

        #endregion

        #region public instance methods

        public void SetParent(Control newParent)
        {
            DrawAreaContainer.Parent = newParent;
            newParent.SizeChanged += ParentResize;
            newParent.SizeChanged += (object s, EventArgs e) => { System.Diagnostics.Debug.Print(System.DateTime.Now.ToString() + " Size of container changed"); };
            FitToParent();
        }

        /// <summary>
        /// Beschriftungshintergründe durchsichtig/undurchsichtig umschalten
        /// </summary>
        public void ToggleLabelTransparency()
        {
            TransparentLabels = !TransparentLabels;
            SetLabelTransparency(TransparentLabels);
        }

        /// <summary>
        /// Zeichne eine Linie in den Graphen
        /// </summary>
        /// <param name="start">Start-Koordinate der Linie</param>
        /// <param name="end">End-Koordinate der Linie</param>
        public void DrawLine(GraphCoord graphStart, GraphCoord graphEnd, Color fillColor, bool redraw=false)
        {
            if ( !redraw && RememberDrawing)
            {
                LineMemory.Add(new SimpleLine(graphStart, graphEnd, fillColor));
            }
            ImageCoord start = Graph2Image(graphStart);
            ImageCoord end = Graph2Image(graphEnd);
            // left and right points rather than start and end
            ImageCoord Left = start.X < end.X ? start : end;
            ImageCoord Right = start.X >= end.X ? start : end;
            //System.Diagnostics.Debug.Print("Drawing Graph-Coords\n\t" + graphStart.ToString() + "\n\t" + graphEnd.ToString());
            //System.Diagnostics.Debug.Print("Eq Pixel-Coords\n\t" + Left.ToString() + "\n\t" + Right.ToString());
            // xdiff is always positive or zero
            float xdiff = Right.X - Left.X;
            if (xdiff == 0f)
            {
                DrawVerticalLine(Left.X);
            }
            // ydiff is positive if up-slope; negative if down-slope; zero if even line
            float ydiff = Right.Y - Left.Y;
            if (ydiff == 0f)
            {
                DrawHorizontalLine(Left.Y);
            }
            // steepness of the slope
            float m = ydiff / xdiff;
            m = (float)Math.Truncate(m * 10000) / 10000;
            float buffer = 0;
            // remember y to count it up/down through x loops
            int y = Left.Y;
            for (int x = Left.X; x < Right.X; x += 1)
            {
                buffer += m;
                if (Math.Abs(buffer) > 1)
                {
                    for (int yc = 1; yc < Math.Abs(buffer); yc += 1)
                    {
                        this.SetPixel(new ImageCoord(x, y + (yc * Math.Sign(m))), fillColor);
                    }
                    y += (int)Math.Truncate(buffer);
                    buffer = (float)Math.Truncate((buffer % 1.0) * 10000) / 10000;
                }
                else
                {
                    this.SetPixel(new ImageCoord(x, y), fillColor);
                }
            }
        }

        /// <summary>
        /// Zeichne mehrere, verbundene Linien in den Graphen
        /// </summary>
        /// <param name="points">Liste der zu verbindenden Koordinaten</param>
        /// <param name="connect_ends">Verbinde erste und letzte Koordinate</param>
        public void DrawLines(GraphCoord[] points, Color fillColor, bool connect_ends = false)
        {
            if (points.Length >= 2)
            {
                GraphCoord current = points.First();
                for (int pc = 1; pc < points.Length; pc += 1)
                {
                    GraphCoord next = points.ElementAt(pc);
                    DrawLine(current, next, fillColor);
                    current = next;
                }
                if (connect_ends)
                {
                    DrawLine(points.Last(), points.First(), fillColor);
                }
            }
        }

        public void DrawMark(GraphCoord coord, float scale = 1.0f)
        {
            float up = coord.Y + scale;
            float down = coord.Y - scale;
            float left = coord.X - scale;
            float right = coord.X + scale;
            DrawLine(new GraphCoord(left, up), new GraphCoord(right, down), ForegroundColor, true);
            DrawLine(new GraphCoord(left, down), new GraphCoord(right, up), ForegroundColor, true);
        }

        public void DrawAxes()
        {
            //System.Diagnostics.Debug.Print("Drawing Axes:");
            DrawLine(new GraphCoord(Geometry.LowX, 0f), new GraphCoord(Geometry.HighX, 0f), ForegroundColor, true);
            DrawLine(new GraphCoord(0f, Geometry.LowY), new GraphCoord(0f, Geometry.HighY), ForegroundColor, true);
            for (float xPositive = Geometry.ScalingX; xPositive < Geometry.HighX; xPositive += Geometry.ScalingX)
            {
                DrawMark(new GraphCoord(xPositive, 0f));
            }
            for (float xNegative = -Geometry.ScalingX; xNegative > Geometry.LowX; xNegative -= Geometry.ScalingX)
            {
                DrawMark(new GraphCoord(xNegative, 0f));
            }
            for (float yPositive = Geometry.ScalingY; yPositive < Geometry.HighY; yPositive += Geometry.ScalingY)
            {
                DrawMark(new GraphCoord(0f, yPositive));
            }
            for (float yNegative = -Geometry.ScalingY; yNegative > Geometry.LowY; yNegative -= Geometry.ScalingY)
            {
                DrawMark(new GraphCoord(0f, yNegative));
            }
        }

        #endregion

        #region private instance methods

        private void ParentResize(object sender, EventArgs e)
        {
            Control parent = (Control)sender;
            System.Diagnostics.Debug.Print("Dimensions of sender: " + parent.Size.ToString());
            System.Diagnostics.Debug.Print("Dimensions of cont:   " + DrawAreaContainer.Parent.Size.ToString());
            FitToParent();
        }

        private void FitToParent()
        {
            if (!AutoResize) return;
            DrawAreaContainer.Dock = DockStyle.Fill;
            int newWidth = DrawAreaContainer.Width;
            int newHeight = DrawAreaContainer.Height;
            DrawArea = new Bitmap(newWidth, newHeight);
            DrawAreaContainer.Image = DrawArea;
            //TODO dynamische labelpositioning nach graph-achsen
            LblY.Parent = DrawAreaContainer;
            LblY.Top = 10;
            LblY.Left = 10;

            LblX.Parent = DrawAreaContainer;
            LblX.Top = newHeight - LblX.Height - 20;
            LblX.Left = newWidth - LblX.Width - 20;

            DrawAxes();
            RedrawGraph();
        }

        private ImageCoord Graph2Image(GraphCoord gc)
        {
            GraphCoord relative = Geometry.GetRelative(gc);
            //System.Diagnostics.Debug.Print(relative.ToString());
            // Die Asurichtung der X-Achse der Bitmap stimmt mit der X-Achse des Graphen überein
            float relX = relative.X;
            // Die Ausrichtung der Y-Achse der Bitmap entgegen der Y-Achse des Graphen
            float relY = 1f - relative.Y;
            int imgX = (int)Math.Floor((double)ImageWidth * relX);
            int imgY = (int)Math.Floor((double)ImageHeight * relY);
            //System.Diagnostics.Debug.Print(imgX.ToString());
            //System.Diagnostics.Debug.Print(imgY.ToString());
            return new ImageCoord(imgX, imgY);
        }

        private GraphCoord Image2Graph(ImageCoord ic)
        {
            return new GraphCoord(0, 0);
        }

        /// <summary>
        /// Setze einen Pixel des Bitmap Objekts
        /// </summary>
        /// <param name="x">Horizontale Bild-Koordinate</param>
        /// <param name="y">Vertikale Bild-Koordinate</param>
        /// <param name="fill">Füllfarbe</param>
        private void SetPixel(ImageCoord imageCoord, Color fill)
        {
            if (imageCoord.X >= 0 && imageCoord.X < ImageWidth && imageCoord.Y >= 0 && imageCoord.Y < ImageHeight)
            {
                DrawArea.SetPixel(imageCoord.X, imageCoord.Y, fill);
            }
        }

        private void SetPoint(GraphCoord gc, Color c) => SetPixel(Graph2Image(gc), c);

        /// <summary>
        /// Setze die Hintergrundfarbe der Label-Objekte
        /// </summary>
        /// <param name="transparent">T: Transparent; F: BackgroundColor</param>
        private void SetLabelTransparency(bool transparent)
        {
            if (transparent)
            {
                LblX.BackColor = Color.Transparent;
                LblY.BackColor = Color.Transparent;
                foreach (Label lbl in LblLegende)
                {
                    lbl.BackColor = Color.Transparent;
                }
            }
            else
            {
                LblX.BackColor = BackgroundColor;
                LblY.BackColor = BackgroundColor;
                foreach (Label lbl in LblLegende)
                {
                    lbl.BackColor = BackgroundColor;
                }
            }
        }

        private void DrawHorizontalLine(int yVal)
        {
            for (int xstep = 0; xstep < ImageWidth; xstep += 1)
            {
                SetPixel(new ImageCoord(xstep, yVal), ForegroundColor);
            }
        }

        private void DrawVerticalLine(int xVal)
        {
            for (int ystep = 0; ystep < ImageHeight; ystep += 1)
            {
                SetPixel(new ImageCoord(xVal, ystep), ForegroundColor);
            }
        }

        private void RedrawGraph()
        {
            foreach(SimpleLine line in LineMemory)
            {
                DrawLine(line.Item1, line.Item2, line.Item3, true);
            }
        }

        #endregion

    }

    /// <summary>
    /// Repräsentiert die Eigenschaften eines geometrischen Koordinatensystems
    /// </summary>
    class GraphProperties
    {
        #region public instance fields
        public float LowX { get; set; }
        public float HighX { get; set; }
        public float LowY { get; set; }
        public float HighY { get; set; }

        /// <summary>
        /// Grapheinteilung X-Achse
        /// </summary>
        public float ScalingX { get; set; }
        /// <summary>
        /// Grapheinteilung Y-Achse
        /// </summary>
        public float ScalingY { get; set; }

        public float Width => HighX - LowX;
        public float Height => HighY - LowY;
        #endregion
        #region private instance fields
        #endregion

        #region constructors

        public GraphProperties()
        {
            LowX = 0.0f;
            LowY = 0.0f;
            HighX = 0.0f;
            HighY = 0.0f;
            ScalingX = 5.0f;
            ScalingY = 5.0f;
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Setzt neue low/high Grenzwerte der Y-Achse
        /// <para>Reihenfolge der Werte hat keine Auswirkung</para>
        /// </summary>
        /// <param name="lim1">Erster Grenzwert</param>
        /// <param name="lim2">Zweiter Grenzwert</param>
        public void SetLimitsY(float lim1, float lim2)
        {
            if (lim1 <= lim2)
            {
                LowY = lim1;
                HighY = lim2;
            }
            else
            {
                LowY = lim2;
                HighY = lim1;
            }
        }

        /// <summary>
        /// Setzt neue low/high Grenzwerte der X-Achse
        /// <para>Reihenfolge der Werte hat keine Auswirkung</para>
        /// </summary>
        /// <param name="lim1">Erster Grenzwert</param>
        /// <param name="lim2">Zweiter Grenzwert</param>
        public void SetLimitsX(float lim1, float lim2)
        {
            if (lim1 <= lim2)
            {
                LowX = lim1;
                HighX = lim2;
            }
            else
            {
                LowX = lim2;
                HighX = lim1;
            }
        }

        /// <summary>
        /// Wandelt absolute Koordinate in relative um
        /// </summary>
        /// <param name="absoluteCoord">Absolute Koordinate</param>
        /// <returns>Relative Koordinate</returns>
        public GraphCoord GetRelative(GraphCoord absoluteCoord)
        {
            // Division durch Null vermeiden
            if (Width == 0.0)
            {
                return new GraphCoord(-1.0f, -1.0f);
            }
            return new GraphCoord(
                (absoluteCoord.X - LowX) / Width,
                (absoluteCoord.Y - LowY) / Height
            );
        }

        #endregion
        #region private instance methods
        #endregion
    }
}
