using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

using ImageCoord = System.Drawing.Point;
using GraphCoord = System.Drawing.PointF;


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

        public Label LblY { get; set; }
        public Label LblX { get; set; }
        public Label[] LblLegende { get; set; }
        public bool TransparentLabels { get; set; }

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

        #endregion

        #region constructors

        public Graph()
        {
            DrawAreaContainer = new PictureBox();
            DrawArea = new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            DrawAreaContainer.Image = DrawArea;

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

            //WIP labelpositioning
            LblX.Top = 10;
            LblX.Left = 10;
            LblY.Top = DrawAreaContainer.Height - LblY.Height - 20;
            LblY.Left = DrawAreaContainer.Width - LblY.Width - 20;

            BackgroundColor = LblX.BackColor;
            TransparentLabels = false;

            Geometry = new GraphProperties();
            Geometry.SetLimitsX(0, DEFAULT_WIDTH);
            Geometry.SetLimitsY(0, DEFAULT_HEIGHT);
        }

        #endregion

        #region public instance methods

        public void SetParent(Control newParent)
        {
            DrawAreaContainer.Parent = newParent;
            DrawAreaContainer.Dock = DockStyle.Fill;
            DrawAreaContainer.BorderStyle = BorderStyle.FixedSingle;
            DrawArea = new Bitmap( DrawAreaContainer.Width, DrawAreaContainer.Height);
            DrawAreaContainer.Image = DrawArea;
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
        public void DrawLine(GraphCoord graphStart, GraphCoord graphEnd)
        {
            ImageCoord start = Graph2Image(graphStart);
            ImageCoord end = Graph2Image(graphEnd);
            // left and right points rather than start and end
            ImageCoord Left = start.X < end.X ? start : end;
            ImageCoord Right = start.X > end.X ? start : end;
            // xdiff is always positive or zero
            float xdiff = Right.X - Left.X;
            // ydiff is positive if up-slope; negative if down-slope; zero if even line
            float ydiff = Right.Y - Left.Y;
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
                        this.SetPixel(new ImageCoord(x, y + (yc * Math.Sign(m))), Color.Black);
                    }
                    y += (int)Math.Truncate(buffer);
                    buffer = (float)Math.Truncate((buffer % 1.0) * 10000) / 10000;
                }
                else
                {
                    this.SetPixel(new ImageCoord(x, y), Color.Black);
                }
            }
        }

        /// <summary>
        /// Zeichne mehrere, verbundene Linien in den Graphen
        /// </summary>
        /// <param name="points">Liste der zu verbindenden Koordinaten</param>
        /// <param name="connect_ends">Verbinde erste und letzte Koordinate</param>
        public void DrawLines(GraphCoord[] points, bool connect_ends = false)
        {
            if (points.Length >= 2)
            {
                GraphCoord current = points.First();
                for (int pc = 1; pc < points.Length; pc += 1)
                {
                    GraphCoord next = points.ElementAt(pc);
                    DrawLine(current, next);
                    current = next;
                }
                if (connect_ends)
                {
                    DrawLine(points.Last(), points.First());
                }
            }
        }

        public void DrawPoint(Point coord, int scale = 10)
        {
            //TODO
        }

        #endregion

        #region private instance methods

        private ImageCoord Graph2Image(GraphCoord gc)
        {
            GraphCoord relative = Geometry.GetRelative(gc);
            // Die Asurichtung der X-Achse der Bitmap stimmt mit der X-Achse des Graphen überein
            float relX = relative.X;
            // Die Ausrichtung der Y-Achse der Bitmap entgegen der Y-Achse des Graphen
            float relY = 1f - relative.Y;
            int imgX = (int)Math.Floor((double)ImageWidth * relX);
            int imgY = (int)Math.Floor((double)ImageHeight * relY);
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
