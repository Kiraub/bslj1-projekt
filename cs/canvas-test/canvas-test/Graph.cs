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
using Boundary = System.Tuple<float, float>;


namespace canvas_test
{
    /// <summary>
    /// Repräsentiert die visuelle Schnittstelle um Graphen zu zeichnen
    /// <para>Beinhaltet sowohl Bild-/ als auch Geometrieobjekte</para>
    /// <para>Das Bildobjekt arbeitet mit Integer-Pixel Koordinaten</para>
    /// <para>Daher reicht für die Geometrie-Berechnung Float-Genauigkeit</para>
    /// <para>Kantenglättung ist nicht implementiert</para>
    /// </summary>
    public class Graph
    {
        #region public class fields
        /// <summary>
        /// Default-Breite der visuellen Darstellungsfläche in Pixel
        /// </summary>
        public static int DEFAULT_WIDTH = 100;
        /// <summary>
        /// Default-Höhe der visuellen Darstellungsfläche in Pixel
        /// </summary>
        public static int DEFAULT_HEIGHT = 100;
        #endregion

        #region public instance fields

        #region graph render options
        //TODO separate render options into own options class
        /// <summary>
        /// Transparente Hintergründe der Graphenbeschriftungen
        /// </summary>
        public bool TransparentLabels { get; set; } = false;
        /// <summary>
        /// Automatisches neu-Zeichnen des Graphen bei geg. Event
        /// </summary>
        public bool AutoResize { get; set; } = true;
        /// <summary>
        /// Schaltet Gedächtnis für Linien an/aus
        /// </summary>
        public bool RememberDrawing { get; set; } = false;
        #endregion
        /// <summary>
        /// Hauptachsenbeschriftung der Y-Achse
        /// </summary>
        public Label LblY { get; set; }
        /// <summary>
        /// Hauptachsenbeschriftung der X-Achse
        /// </summary>
        public Label LblX { get; set; }
        /// <summary>
        /// List aller Label, welche für Achsenbeschriftung o.ä. dynamisch erstellt sind
        /// </summary>
        public List<Label> LblLegende { get; set; }

        /// <summary>
        /// Repräsentiert die visuelle Darstellungsfläche
        /// </summary>
        public Bitmap DrawArea { get; set; }
        /// <summary>
        /// Container der visuellen Darstellungsfläche
        /// </summary>
        public PictureBox DrawAreaContainer { get; set; }

        /// <summary>
        /// Vordergrundfarbe der visuellen Darstellungsfläche
        /// </summary>
        public Color ForegroundColor { get; set; }
        /// <summary>
        /// Hintergrundfarbe der visuellen Darstellungsfläche
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Repräsentiert die X-Achsen Limits
        /// </summary>
        public Boundary HorizontalAxis
        {
            get { return new Boundary(Geometry.LowX, Geometry.HighX); }
            set { Geometry.LowX = value.Item1; Geometry.HighX = value.Item2; FitToParent(); }
        }
        /// <summary>
        /// Repräsentiert die Y-Achsen Limits
        /// </summary>
        public Boundary VerticalAxis
        {
            get { return new Boundary(Geometry.LowY, Geometry.HighY); }
            set { Geometry.LowY = value.Item1; Geometry.HighY = value.Item2; FitToParent(); }
        }

        #endregion

        #region private instance fields
        /// <summary>
        /// Breite der visuellen Darstellungsfläche
        /// </summary>
        private int ImageWidth => DrawArea.Width;
        /// <summary>
        /// Höhe der visuellen Darstellungsfläche
        /// </summary>
        private int ImageHeight => DrawArea.Height;
        /// <summary>
        /// Container der geomtrischen Schnittstelle
        /// </summary>
        private GraphProperties Geometry { get; set; }
        /// <summary>
        /// Gedächtnis der zu vorhandenen Linien
        /// </summary>
        private List<SimpleLine> LineMemory { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Erstelle einen neuen Graphen mit visuellem und geometrischem Container
        /// <para></para>
        /// <para>Default-Einstellungen</para>
        /// <para>Vordergrund-Farbe: Schwarz</para>
        /// <para>Hintergrund-Farbe: Beige</para>
        /// </summary>
        public Graph()
        {
            ForegroundColor = Color.Black;
            BackgroundColor = Color.Beige;

            DrawAreaContainer = new PictureBox();
            DrawArea = new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            DrawAreaContainer.Image = DrawArea;
            DrawAreaContainer.BorderStyle = BorderStyle.FixedSingle;
            DrawAreaContainer.BackColor = BackgroundColor;

            LblX = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "X Achse",
                Parent = DrawAreaContainer,
                BackColor = BackgroundColor
            };

            LblY = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Y Achse",
                Parent = DrawAreaContainer,
                BackColor = BackgroundColor
            };

            LblLegende = new List<Label>();
            LineMemory = new List<SimpleLine>();

            TransparentLabels = false;

            Geometry = new GraphProperties();
            HorizontalAxis = new Boundary(-10f, 100f);
            VerticalAxis = new Boundary(-10f, 100f);
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Hängt den Graphen an ein Control-Element und fügt einen Event-Listener an dessen SizeChanged-Event
        /// </summary>
        /// <param name="newParent">Neues Parent Control-Element</param>
        public void SetParent(Control newParent)
        {
            DrawAreaContainer.Parent = newParent;
            newParent.SizeChanged += ParentResize;
            FitToParent();
        }

        /// <summary>
        /// Löst ein neu berechnen/zeichnen des Graphen aus, egal wie die derzeitigen Eigenschaften sind
        /// </summary>
        public void ForceResize()
        {
            bool previous = AutoResize;
            AutoResize = true;
            FitToParent();
            AutoResize = previous;
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
        /// <param name="fillColor">Füllfarbe der Linie</param>
        /// <param name="is_redraw"><para>True: Linie wird nicht gespeichert</para><para></para><para>False: Linie wird gespeichert</para></param>
        public void DrawLine(GraphCoord graphStart, GraphCoord graphEnd, Color fillColor, bool is_redraw = false)
        {
            if (!is_redraw && RememberDrawing)
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
                DrawVerticalLine(Left.X, Left.Y, Right.Y);
            }
            // ydiff is positive if up-slope; negative if down-slope; zero if even line
            float ydiff = Right.Y - Left.Y;
            if (ydiff == 0f)
            {
                DrawHorizontalLine(Left.Y, Left.X, Right.X);
            }
            // steepness of the slope
            float m = ydiff / xdiff;
            m = (float)Math.Truncate(m * 10000) / 10000;
            // buffer to even out difference of pixel coord and graph coord
            float buffer = 0;
            // remember y to count it up/down through x loops
            int y = Left.Y;
            for (int x = Left.X; x < Right.X; x += 1)
            {
                // remember vertical distance
                buffer += m;
                if (Math.Abs(buffer) > 1)
                {
                    // draw vertical pixels
                    for (int yc = 1; yc < Math.Abs(buffer); yc += 1)
                    {
                        this.SetPixel(new ImageCoord(x, y + (yc * Math.Sign(m))), fillColor);
                    }
                    y += (int)Math.Truncate(buffer);
                    buffer = (float)Math.Truncate((buffer % 1.0) * 10000) / 10000;
                }
                else
                {
                    // draw horizontal pixel
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

        /// <summary>
        /// Zeichnet ein '+'-Kreuz in den Graphen
        /// </summary>
        /// <param name="coord">Koordinate des Kreuz-Zentrums</param>
        /// <param name="scale">+/- Bereich um Koordinate der Kreuz-Linien</param>
        public void DrawMark(GraphCoord coord, float scale = 0.75f)
        {
            float up = coord.Y + scale;
            float down = coord.Y - scale;
            float left = coord.X - scale;
            float right = coord.X + scale;
            DrawLine(new GraphCoord(left, coord.Y), new GraphCoord(right, coord.Y), ForegroundColor, true);
            DrawLine(new GraphCoord(coord.X, down), new GraphCoord(coord.X, up), ForegroundColor, true);
        }

        /// <summary>
        /// Zeichnet die Achsen des Graphen, diese werden nicht im LineMemory gespeichert
        /// </summary>
        public void DrawAxes()
        {
            DrawLine(new GraphCoord(Geometry.LowX, 0f), new GraphCoord(Geometry.HighX, 0f), ForegroundColor, true);
            DrawLine(new GraphCoord(0f, Geometry.LowY), new GraphCoord(0f, Geometry.HighY), ForegroundColor, true);
            for (float xPositive = Geometry.ScalingX; xPositive < Geometry.HighX + Geometry.ScalingX; xPositive += Geometry.ScalingX)
            {
                DrawMark(new GraphCoord(xPositive, 0f));
            }
            for (float xNegative = -Geometry.ScalingX; xNegative > Geometry.LowX - Geometry.ScalingX; xNegative -= Geometry.ScalingX)
            {
                DrawMark(new GraphCoord(xNegative, 0f));
            }
            for (float yPositive = Geometry.ScalingY; yPositive < Geometry.HighY + Geometry.ScalingY; yPositive += Geometry.ScalingY)
            {
                DrawMark(new GraphCoord(0f, yPositive));
            }
            for (float yNegative = -Geometry.ScalingY; yNegative > Geometry.LowY - Geometry.ScalingY; yNegative -= Geometry.ScalingY)
            {
                DrawMark(new GraphCoord(0f, yNegative));
            }
        }

        /// <summary>
        /// Verbindet Punkte der Reihe nach mit Näherungskurven
        /// <para>Da dies durch zeichnen vielen Geraden realisiert wird, muss die Genauigkeit angegeben werden</para>
        /// </summary>
        /// <param name="startPoint">Anfangspunkt der ersten Kurve</param>
        /// <param name="curvePoints">Restpunkte</param>
        /// <param name="fillColor">Füllfarbe der Kurven</param>
        /// <param name="approxCount">Anzahl Annäherungen, höher=runder</param>
        /// <param name="approxLevel">Richtung &amp; Härte der Annäherungen, zwischen -1 und 1</param>
        public void DrawCurve(GraphCoord startPoint, List<GraphCoord> curvePoints, Color fillColor, int approxCount = 10, float approxLevel = 0.5f)
        {
            int count = curvePoints.Count;
            if (count == 0)
            {
                // cannot draw with zero curve points
                return;
            }
            else if (count == 1)
            {
                ApproxCurve(startPoint, curvePoints.First(), fillColor, approxCount, approxLevel);
            }
            else
            {
                float yNow = startPoint.Y;
                float yN1 = curvePoints[0].Y;
                float yN2 = curvePoints[1].Y;
                float nextApproxLevel = -approxLevel;
                if (yNow > yN1 && yN1 < yN2)
                {
                    approxLevel = -Math.Abs(approxLevel);
                    nextApproxLevel = approxLevel;
                }
                else if (yNow < yN1 && yN1 > yN2)
                {
                    approxLevel = Math.Abs(approxLevel);
                    nextApproxLevel = approxLevel;
                }
                DrawCurve(curvePoints.First(), curvePoints.GetRange(1, count - 1), fillColor, approxCount, nextApproxLevel);
                ApproxCurve(startPoint, curvePoints.First(), fillColor, approxCount, approxLevel);
            }
        }

        /// <summary>
        /// Zeichnet eine polynomielle Funktion zweiten Grades f(x)=ax^2+bx+c
        /// </summary>
        /// <param name="polynomial">Quadratisches Polynom</param>
        /// <param name="fillColor">Füllfarbe der Linie</param>
        /// <param name="stepAccuracy"><para>Genauigkeits-Multiplikator mit der f(x) gezeichnet wird</para><para></para><para>1.0 entspricht 1/200 Schrittweite</para></param>
        public void DrawPolynomialFunction(QuadPolynomial polynomial, Color fillColor, float stepAccuracy=10.0f)
        {
            // draw inside x-axis bounds
            float xLeft = Geometry.LowX;
            float xRight = Geometry.HighX;
            GraphCoord lastPoint = new GraphCoord(-1f, -1f);
            float stepIncrement = Geometry.Width / (200f * stepAccuracy);
            for( float xStep = xLeft; xStep <= xRight; xStep += stepIncrement)
            {
                // calculate y = f(x) = P(x) / Q(x)
                float yStep = (float) Math.Pow(polynomial.two, 2.0)*xStep + polynomial.one*xStep + polynomial.zero;
                GraphCoord functionPoint = new GraphCoord(xStep, yStep);
                if ( xStep > xLeft )
                {
                    DrawLine(lastPoint, functionPoint, fillColor, false);
                }
                lastPoint = functionPoint;
            }
        }

        /// <summary>
        /// Zeichnet eine rationale Funktion f(x)=P(x)/Q(x)
        /// </summary>
        /// <param name="pFunc">Das Zähler-Polynom</param>
        /// <param name="qFunc">Das Nenner-Polynom</param>
        /// <param name="fillColor">Füllfarbe der Linie</param>
        /// <param name="stepAccuracy"><para>Genauigkeits-Multiplikator mit der f(x) gezeichnet wird</para><para></para><para>1.0 entspricht 1/200 Schrittweite</para></param>
        public void DrawRationalFunction(QuadPolynomial pFunc, QuadPolynomial qFunc, Color fillColor, float stepAccuracy=10.0f)
        {
            // draw inside x-axis bounds
            float xLeft = Geometry.LowX;
            float xRight = Geometry.HighX;
            GraphCoord lastPoint = new GraphCoord(-1f, -1f);
            float stepIncrement = Geometry.Width / (200f * stepAccuracy);
            for( float xStep = xLeft; xStep <= xRight; xStep += stepIncrement)
            {
                // calculate y = f(x) = P(x) / Q(x)
                double pStep = Math.Pow(pFunc.two, 2.0)*xStep + pFunc.one*xStep + pFunc.zero;
                double qStep = Math.Pow(qFunc.two, 2.0)*xStep + qFunc.one*xStep + qFunc.zero;
                if (qStep == 0.0)
                {
                    continue;
                }
                float yStep = (float) (pStep/qStep);
                GraphCoord functionPoint = new GraphCoord(xStep, yStep);
                if ( xStep > xLeft )
                {
                    DrawLine(lastPoint, functionPoint, fillColor, false);
                }
                lastPoint = functionPoint;
            }
        }

        #endregion

        #region private instance methods

        private void ParentResize(object sender, EventArgs e)
        {
            Control parent = (Control)sender;
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

        private void DrawHorizontalLine(int yVal, int xLeft, int xRight)
        {
            for (int xstep = xLeft; xstep < xRight; xstep += 1)
            {
                SetPixel(new ImageCoord(xstep, yVal), ForegroundColor);
            }
        }

        private void DrawVerticalLine(int xVal, int yDown, int yUp)
        {
            int stepSize = Math.Sign(yUp - yDown);
            for (int ystep = yDown; ystep != yUp; ystep += stepSize)
            {
                SetPixel(new ImageCoord(xVal, ystep), ForegroundColor);
            }
        }

        private void RedrawGraph()
        {
            foreach (SimpleLine line in LineMemory)
            {
                DrawLine(line.Item1, line.Item2, line.Item3, true);
            }
        }

        private void ApproxCurve(GraphCoord startPoint, GraphCoord endPoint, Color fillColor, int approxCount, float level)
        {
            ImageCoord start = Graph2Image(startPoint);
            ImageCoord end = Graph2Image(endPoint);
            if (0 < approxCount)
            {
                GraphCoord intermediate = Geometry.GetIntermediate(startPoint, endPoint, level);
                ApproxCurve(startPoint, intermediate, fillColor, approxCount - 1, level / 2);
                ApproxCurve(intermediate, endPoint, fillColor, approxCount - 1, level / 2);
            }
            else
            {
                DrawLine(startPoint, endPoint, fillColor);
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
        public void SetLimitsY(Boundary limits)
        {
            if (limits.Item1 <= limits.Item2)
            {
                LowY = limits.Item1;
                HighY = limits.Item2;
            }
            else
            {
                LowY = limits.Item2;
                HighY = limits.Item1;
            }
        }

        /// <summary>
        /// Setzt neue low/high Grenzwerte der X-Achse
        /// <para>Reihenfolge der Werte hat keine Auswirkung</para>
        /// </summary>
        /// <param name="lim1">Erster Grenzwert</param>
        /// <param name="lim2">Zweiter Grenzwert</param>
        public void SetLimitsX(Boundary limits)
        {
            if (limits.Item1 <= limits.Item2)
            {
                LowX = limits.Item1;
                HighX = limits.Item2;
            }
            else
            {
                LowX = limits.Item2;
                HighX = limits.Item1;
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
            if (Width == 0.0f)
            {
                return new GraphCoord(-1.0f, -1.0f);
            }
            return new GraphCoord(
                (absoluteCoord.X - LowX) / Width,
                (absoluteCoord.Y - LowY) / Height
            );
        }

        public GraphCoord GetIntermediate(GraphCoord start, GraphCoord end, float level)
        {
            float xdiff = end.X - start.X;
            float ydiff = end.Y - start.Y;
            float xMed = start.X + xdiff / 2 - Math.Sign(ydiff) * xdiff * level * 0.5f;
            float yMed = start.Y + ydiff / 2 + Math.Sign(ydiff) * ydiff * level * 0.5f;
            return new GraphCoord(xMed, yMed);
        }

        #endregion
        #region private instance methods
        #endregion
    }

    /// <summary>
    /// Repräsentiert eine polynomiale Funktion zweiten Grades der Form f(x) = ax^2 + bx + c
    /// </summary>
    public struct QuadPolynomial
    {
        /// <summary>
        /// Faktor a von x^2
        /// </summary>
        public float two;
        /// <summary>
        /// Faktor b von x
        /// </summary>
        public float one;
        /// <summary>
        /// Summand c
        /// </summary>
        public float zero;
    }

    /* If needed more polyniomal structs should be created like the one above.
     * This separation is needed, since drawing functions need to know how to handle a polynom's degree.
     * Examples:
        public struct LinearPolynomial
        {
            public float one;
            public float zero;
        }
        public struct QubicPolynomial
        {
            public float three;
            public float two;
            public float one;
            public float zero;
        }
     * These could also be created nested, although this is not necessary for current feature requirements.
     * Example:
        public struct QubicPolynomial
        {
            public float three;
            public QuadPolynomial quadPolynomial;
        }
        public struct QuadPolynomial
        {
            public float two;
            public LinearPolynomial linearPolynomial;
        }
        public struct LinearPolynomial
        {
            public float one;
            public float zero;
        }
     * Note that neither of these examples are tested and should be expected to require modifications upon implementation.
     */

}
