using System;
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


namespace Bitgraph
{
    /// <summary>
    /// Repräsentiert die zusammengefasste Schnittstelle um Graphen zu zeichnen
    /// <para>Beinhaltet hauptsächlich visuelle Eigenschaften mit einer ausgelagerten Geometrie</para>
    /// <para>Das Bildobjekt arbeitet mit Integer-Pixel Koordinaten</para>
    /// <para>Die Geometrie-Berechnung arbeitet mit Float-Genauigkeit</para>
    /// <para>Kantenglättung ist nicht implementiert</para>
    /// </summary>
    public class GraphManager
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

        /// <summary>
        /// Transparente Hintergründe der Graphenbeschriftungen
        /// </summary>
        public bool TransparentLabels { get {return Options.TransparentLabels;} set{Options.TransparentLabels=value;} }
        /// <summary>
        /// Automatisches neu-Zeichnen des Graphen bei geg. Event
        /// </summary>
        public bool AutoResize { get {return Options.AutoResize;} set{Options.AutoResize=value;} }
        /// <summary>
        /// Schaltet Gedächtnis für Linien an/aus
        /// </summary>
        public bool RememberDrawing { get {return Options.RememberDrawing;} set{Options.RememberDrawing=value;} }

        /// <summary>
        /// Hauptachsenbeschriftung der Y-Achse
        /// </summary>
        public Label LblYAxis { get; set; }
        /// <summary>
        /// Hauptachsenbeschriftung der X-Achse
        /// </summary>
        public Label LblXAxis { get; set; }
        /// <summary>
        /// Liste aller Label, welche für Achsenbeschriftung o.ä. dynamisch erstellt werden
        /// </summary>
        public List<Label> LstLblMarkings { get; set; }

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
        /// Repräsentiert die geometrischen X-Achsen Limits des Geometrie-Objekts
        /// <para></para>
        /// <para>Löst bei setzen ein bedingtes Anpassen an das Parent aus</para>
        /// </summary>
        public Boundary HorizontalAxis
        {
            get { return new Boundary(Geometry.LowX, Geometry.HighX); }
            set { Geometry.LowX = value.Item1; Geometry.HighX = value.Item2; FitToParent(); }
        }
        /// <summary>
        /// Repräsentiert die geomtrischen Y-Achsen Limits des Geometrie-Objekts
        /// <para></para>
        /// <para>Löst bei setzen ein bedingtes Anpassen an das Parent aus</para>
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

        private GraphOptions Options { get; set; }
        /// <summary>
        /// Container der geomtrischen Schnittstelle
        /// </summary>
        private GraphGeomtry Geometry { get; set; }
        /// <summary>
        /// Gedächtnis der vorhandenen Linien
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
        public GraphManager()
        {
            Options = new GraphOptions();

            ForegroundColor = Color.Black;
            BackgroundColor = Color.Beige;

            DrawAreaContainer = new PictureBox();
            DrawArea = new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            DrawAreaContainer.Image = DrawArea;
            DrawAreaContainer.BorderStyle = BorderStyle.FixedSingle;
            DrawAreaContainer.BackColor = BackgroundColor;

            LblXAxis = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "X Achse",
                Parent = DrawAreaContainer,
                BackColor = BackgroundColor
            };

            LblYAxis = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Y Achse",
                Parent = DrawAreaContainer,
                BackColor = BackgroundColor
            };

            LstLblMarkings = new List<Label>();
            LineMemory = new List<SimpleLine>();

            TransparentLabels = false;

            Geometry = new GraphGeomtry();
            HorizontalAxis = new Boundary(-10f, 100f);
            VerticalAxis = new Boundary(-10f, 100f);
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Hängt den Graphen an das gegebene Control-Element und fügt einen Event-Listener an dessen SizeChanged-Event
        /// </summary>
        /// <param name="newParent">Neues Parent Control-Element</param>
        public void SetParent(Control newParent)
        {
            DrawAreaContainer.Parent = newParent;
            newParent.SizeChanged += Parent_SizeChanged;
            FitToParent();
        }

        /// <summary>
        /// Löst ein neu berechnen/zeichnen des Graphen aus, unabhängig von derzeitigen Eigenschaften
        /// </summary>
        public void ForceResize()
        {
            bool previousAutoResize = AutoResize;
            bool previousRemember = RememberDrawing;
            AutoResize = true;
            RememberDrawing = true;
            FitToParent();
            AutoResize = previousAutoResize;
            RememberDrawing = previousRemember;
        }

        /// <summary>
        /// Beschriftungshintergründe durchsichtig/undurchsichtig umschalten
        /// </summary>
        public void ToggleLabelTransparency()
        {
            TransparentLabels = !TransparentLabels;
            SetLabelTransparency(TransparentLabels);
        }

        public List<CheckBox> GetOptionsList()
        {
            CheckBox traCheckBox = Options.TransparentLabelsCheckBox;
            traCheckBox.Checked = TransparentLabels;
            traCheckBox.CheckedChanged += (object s, EventArgs e) => { TransparentLabels = traCheckBox.Checked; SetLabelTransparency(TransparentLabels); };
            CheckBox autoCheckBox = Options.AutoResizeCheckBox;
            autoCheckBox.Checked = AutoResize;
            autoCheckBox.CheckedChanged += (object s, EventArgs e) => { AutoResize = autoCheckBox.Checked; FitToParent(); };
            CheckBox remCheckBox = Options.RememberDrawingCheckBox;
            remCheckBox.Checked = RememberDrawing;
            remCheckBox.CheckedChanged += (object s, EventArgs e) => { RememberDrawing = remCheckBox.Checked; };

            List<CheckBox> checkBoxList = new List<CheckBox>
            {
                traCheckBox, autoCheckBox/*, remCheckBox*/
            };
            return checkBoxList;
        }

        public List<Button> GetActionsList(ValueTable actionLinkedTable)
        {
            Button drawPlotButton = new Button
            {
                Text = "Zeichne Graphen"
            };
            drawPlotButton.Click += (object s, EventArgs e) =>
            {
                ClearLineMemory();
                DrawPolynomialFunction(ValueTable.AmpereFunction, Options.PlotColors[0]);
                DrawPolynomialFunction(ValueTable.VoltageFunction, Options.PlotColors[1]);
                DrawPolynomialFunction(ValueTable.PowerFunction, Options.PlotColors[2]);
                ForceResize();
            };
            Button drawDataMarksButton = new Button
            {
                Text = "Zeichne Datenpunkte"
            };
            drawDataMarksButton.Click += (object s, EventArgs e) =>
            {
                double[,] data = actionLinkedTable.GetTableContents();
                int columnCount = data.GetLength(0);
                int rowCount = data.GetLength(1);
                // first column are X-values
                // loop through other columns for Y-values
                for (int column = 1; column < columnCount; column += 1)
                {
                    for (int row = 0; row < rowCount; row += 1)
                    {
                        //debugging output
                        //System.Diagnostics.Debug.Print(row.ToString() + ": " + data[column, row].ToString() + " ");
                        DrawMark(new PointF((float)data[0, row], (float)data[column, row]), Options.PlotColors[column-1], false);
                    }
                    //System.Diagnostics.Debug.Print("----");
                }
                ForceResize();
            };
            Button deleteButton = new Button
            {
                Text = "Lösche Graphen"
            };
            deleteButton.Click += (object s, EventArgs e) =>
            {
                ClearLineMemory();
                ForceResize();
            };
            //TODO other actions

            List<Button> buttonList = new List<Button>
            {
                drawPlotButton, drawDataMarksButton, deleteButton
            };
            return buttonList;
        }

        public List<NumericUpDown> GetScales()
        {
            NumericUpDown Xmin = new NumericUpDown { Text = "X Minimum", Maximum = 500.0m, Minimum = -500.0m, Value = (decimal)Geometry.LowX };
            NumericUpDown Xmax = new NumericUpDown { Text = "X Maximum", Maximum = 500.0m, Minimum = -500.0m, Value = (decimal)Geometry.HighX };
            NumericUpDown Xstep = new NumericUpDown { Text = "X Intervall", Maximum = 500.0m, Minimum = -500.0m, Value = (decimal)Geometry.ScalingX };
            NumericUpDown Ymin = new NumericUpDown { Text = "Y Minimum", Maximum = 500.0m, Minimum = -500.0m, Value = (decimal)Geometry.LowY };
            NumericUpDown Ymax = new NumericUpDown { Text = "Y Maximum", Maximum = 500.0m, Minimum = -500.0m, Value = (decimal)Geometry.HighY };
            NumericUpDown Ystep = new NumericUpDown { Text = "Y Intervall", Maximum = 500.0m, Minimum = -500.0m, Value = (decimal)Geometry.ScalingY };
            List<NumericUpDown> numUpDownList = new List<NumericUpDown>
            {
                Xmin, Xmax, Xstep, Ymin, Ymax, Ystep
            };
            return numUpDownList;
        }

        #region Drawing related methods

        /// <summary>
        /// Zeichne eine Linie in den Graphen
        /// </summary>
        /// <param name="graphStart">Start-Koordinate</param>
        /// <param name="graphEnd">End-Koordinate</param>
        /// <param name="fillColor">Füllfarbe</param>
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
            // xdiff is always positive or zero
            float xdiff = Right.X - Left.X;
            if (xdiff == 0f)
            {
                DrawVerticalLine(Left.X, Left.Y, Right.Y, fillColor);
            }
            // ydiff is positive if up-slope; negative if down-slope; zero if even line
            float ydiff = Right.Y - Left.Y;
            if (ydiff == 0f)
            {
                DrawHorizontalLine(Left.Y, Left.X, Right.X, fillColor);
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
        /// Zeichnet ein '+'-Kreuz in den Graphen
        /// </summary>
        /// <param name="coord">Koordinate des Kreuz-Zentrums</param>
        /// <param name="fillColor">Füllfarbe</param>
        /// <param name="is_redraw">True: Markierung brauch nicht gemerkt zu werden</param>
        /// <param name="scale">+/- Bereich um Koordinate der Kreuz-Linien</param>
        public void DrawMark(GraphCoord coord, Color fillColor, bool is_redraw, float scale = 0.75f)
        {
            float up = coord.Y + scale;
            float down = coord.Y - scale;
            float left = coord.X - scale;
            float right = coord.X + scale;
            DrawLine(new GraphCoord(left, coord.Y), new GraphCoord(right, coord.Y), fillColor, is_redraw);
            DrawLine(new GraphCoord(coord.X, down), new GraphCoord(coord.X, up), fillColor, is_redraw);
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
                DrawMark(new GraphCoord(xPositive, 0f), ForegroundColor, true, 1f);
                DrawMark(new GraphCoord(xPositive - Geometry.ScalingX / 2, 0f), ForegroundColor, true, 0.4f);
            }
            for (float xNegative = -Geometry.ScalingX; xNegative > Geometry.LowX - Geometry.ScalingX; xNegative -= Geometry.ScalingX)
            {
                DrawMark(new GraphCoord(xNegative, 0f), ForegroundColor, true, 1f);
                DrawMark(new GraphCoord(xNegative + Geometry.ScalingX / 2, 0f), ForegroundColor, true, 0.4f);
            }
            for (float yPositive = Geometry.ScalingY; yPositive < Geometry.HighY + Geometry.ScalingY; yPositive += Geometry.ScalingY)
            {
                DrawMark(new GraphCoord(0f, yPositive), ForegroundColor, true, 1f);
                DrawMark(new GraphCoord(0f, yPositive - Geometry.ScalingY / 2), ForegroundColor, true, 0.4f);
            }
            for (float yNegative = -Geometry.ScalingY; yNegative > Geometry.LowY - Geometry.ScalingY; yNegative -= Geometry.ScalingY)
            {
                DrawMark(new GraphCoord(0f, yNegative), ForegroundColor, true, 1f);
                DrawMark(new GraphCoord(0f, yNegative + Geometry.ScalingY / 2), ForegroundColor, true, 0.4f);
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
                float yStep = polynomial.FunctionValue(xStep);
                GraphCoord functionPoint = new GraphCoord(xStep, yStep);
                if ( xStep > xLeft )
                {
                    DrawLine(lastPoint, functionPoint, fillColor, false);
                    //System.Diagnostics.Debug.Print("zeichne linie: " + lastPoint.ToString() + "; " + functionPoint.ToString());
                }
                lastPoint = functionPoint;
            }
        }

        /// <summary>
        /// Entfernt sämtliche Linien bedingungslos aus dem Gedächtnis
        /// </summary>
        public void ClearLineMemory()
        {
            LineMemory.Clear();
        }

        #endregion

        #endregion

        #region private instance methods

        /// <summary>
        /// Listener-Funktion auf das SizeChanged-Event des Parent Control-Elements
        /// <para></para>
        /// <para>Löst ein bedingtes Anpassen an das Parent aus</para>
        /// </summary>
        /// <param name="sender">Parent Control-Element</param>
        /// <param name="_e">Nicht verarbeitete Event-Infos</param>
        private void Parent_SizeChanged(object sender, EventArgs _e)
        {
            Control parent = (Control)sender;
            FitToParent();
        }

        /// <summary>
        /// Bedingtes anpassen an das Parent Control-Element:
        /// <para></para>
        /// <para>- Setzt DockStyle, Breite und Höhe des visuellen Containers</para>
        /// <para>- Generiert neue Darstellungsfläche und fügt diese in den Container ein</para>
        /// <para>- Repositioniert die Graphenbeschriftung</para>
        /// <para>- Zeichnet Achsen und Graphen neu</para>
        /// </summary>
        private void FitToParent()
        {
            if (!AutoResize) return;
            DrawAreaContainer.Dock = DockStyle.Fill;
            int newWidth = DrawAreaContainer.Width;
            int newHeight = DrawAreaContainer.Height;
            DrawArea = new Bitmap(newWidth, newHeight);
            DrawAreaContainer.Image = DrawArea;
            //TODO dynamische labelpositioning nach graph-achsen
            LblYAxis.Parent = DrawAreaContainer;
            LblYAxis.Top = 10;
            LblYAxis.Left = 10;

            LblXAxis.Parent = DrawAreaContainer;
            LblXAxis.Top = newHeight - LblXAxis.Height - 20;
            LblXAxis.Left = newWidth - LblXAxis.Width - 20;

            DrawAxes();
            RedrawGraph();
        }

        /// <summary>
        /// Konvertiert eine geometrische in eine visuelle Koordinate
        /// </summary>
        /// <param name="gc">geometrische Koordinate</param>
        /// <returns>visuelle Koordinate</returns>
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

        /// <summary>
        /// Nicht implementierte Funktion, da visuelle zu geometrische Konvertierung noch nicht benötigt ist.
        /// <para></para>
        /// <para>Möglicherweise wenn Interaktion im Bild möglich werden soll wichtig</para>
        /// </summary>
        /// <param name="ic">visuelle Korrdinate</param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns>geometrische Koordinate</returns>
        private GraphCoord Image2Graph(ImageCoord ic)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Setze einen Pixel des Bitmap Objekts
        /// </summary>
        /// <param name="imageCoord">Bild-Koordinaten des Pixels</param>
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
                LblXAxis.BackColor = Color.Transparent;
                LblYAxis.BackColor = Color.Transparent;
                foreach (Label lbl in LstLblMarkings)
                {
                    lbl.BackColor = Color.Transparent;
                }
            }
            else
            {
                LblXAxis.BackColor = BackgroundColor;
                LblYAxis.BackColor = BackgroundColor;
                foreach (Label lbl in LstLblMarkings)
                {
                    lbl.BackColor = BackgroundColor;
                }
            }
        }

        /// <summary>
        /// Zeichnet eine horizontale Linie in den Graphen
        /// </summary>
        /// <param name="yVal">visueller Y-Wert der Linie</param>
        /// <param name="xLeft">visuell linker X-Wert der Linie</param>
        /// <param name="xRight">visuell rechter X-Wert der Linie</param>
        /// <param name="fillColor">Füllfarbe</param>
        private void DrawHorizontalLine(int yVal, int xLeft, int xRight, Color fillColor)
        {
            for (int xstep = xLeft; xstep < xRight; xstep += 1)
            {
                SetPixel(new ImageCoord(xstep, yVal), fillColor);
            }
        }

        /// <summary>
        /// Zeichnet eine vertikale Linie in den Graphen
        /// </summary>
        /// <param name="xVal">visueller X-Wert der Linie</param>
        /// <param name="yDown">visuell niedrigerer Y-Wert der Linie</param>
        /// <param name="yUp">visuell höherer Y-Wert der Linie</param>
        /// <param name="fillColor">Füllfarbe</param>
        private void DrawVerticalLine(int xVal, int yDown, int yUp, Color fillColor)
        {
            int stepSize = Math.Sign(yUp - yDown);
            for (int ystep = yDown; ystep != yUp; ystep += stepSize)
            {
                SetPixel(new ImageCoord(xVal, ystep), fillColor);
            }
        }

        /// <summary>
        /// Zeichnet sämtliche Linien die derzeit im Gedächtnis sind
        /// </summary>
        private void RedrawGraph()
        {
            foreach (SimpleLine line in LineMemory)
            {
                DrawLine(line.Item1, line.Item2, line.Item3, true);
            }
        }

        #endregion

    }

    /// <summary>
    /// Container-Klasse für Optionen der Graph-Visualisierung
    /// </summary>
    public class GraphOptions
    {
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
        public bool RememberDrawing { get; set; } = true;

        public List<Color> PlotColors = new List<Color>{ Color.Red, Color.Blue, Color.Green };

        public CheckBox TransparentLabelsCheckBox => new CheckBox { Checked = TransparentLabels, Text = "Transparente Beschriftung" };
        public CheckBox AutoResizeCheckBox => new CheckBox { Checked = AutoResize, Text = "Automatische Größenanpassung" };
        public CheckBox RememberDrawingCheckBox => new CheckBox { Checked = RememberDrawing, Text = "Merke gezeichnete Linien" };
    }

    /// <summary>
    /// Repräsentiert die Eigenschaften eines geometrischen Koordinatensystems
    /// </summary>
    class GraphGeomtry
    {
        #region public instance fields
        /// <summary>
        /// Minimaler X-Wert des Definitionsbereichs
        /// </summary>
        public float LowX { get; set; }
        /// <summary>
        /// Maximaler X-Wert des Definitionsbereichs
        /// </summary>
        public float HighX { get; set; }
        /// <summary>
        /// Minimaler Y-Wert des Wertebereichs
        /// </summary>
        public float LowY { get; set; }
        /// <summary>
        /// Maximaler Y-Wert des Wertebereichs
        /// </summary>
        public float HighY { get; set; }

        /// <summary>
        /// Grapheinteilung X-Achse
        /// </summary>
        public float ScalingX { get; set; }
        /// <summary>
        /// Grapheinteilung Y-Achse
        /// </summary>
        public float ScalingY { get; set; }

        /// <summary>
        /// Größe des möglichen Definitionsbereichs
        /// </summary>
        public float Width => HighX - LowX;
        /// <summary>
        /// Größe des möglichen Wertebereichs
        /// </summary>
        public float Height => HighY - LowY;
        #endregion

        #region private instance fields
        #endregion

        #region constructors

        /// <summary>
        /// Erstellt eine neue Geometrie-Instanz mit '0f'-instanzierten Definitions- und Wertebereich Limits
        /// <para></para>
        /// <para>Es ist erforderlich die Limits nachträglich anzupassen</para>
        /// </summary>
        public GraphGeomtry()
        {
            LowX = -6.0f;
            LowY = -6.0f;
            HighX = 51.0f;
            HighY = 51.0f;
            ScalingX = 5.0f;
            ScalingY = 5.0f;
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Setzt neue Min/Max Grenzwerte des Wertebereichs
        /// <para>Reihenfolge der Werte ist irrelevant, es wird immer der niedrigere als Min und der höhere als Max gesetzt</para>
        /// <para>Bei Gleichheit beider Werte ist der zweite das neue Min und der erste das neue Max</para>
        /// </summary>
        /// <param name="limits">Grenzwert-Tupel</param>
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
        /// Setzt neue Min/Max Grenzwerte des Definitionsbereichs
        /// <para>Reihenfolge der Werte ist irrelevant, es wird immer der niedrigere als Min und der höhere als Max gesetzt</para>
        /// <para>Bei Gleichheit beider Werte ist der zweite das neue Min und der erste das neue Max</para>
        /// </summary>
        /// <param name="limits">Grenzwert-Tupel</param>
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
        /// <returns>Zu (MinX,MinY) Relative Koordinate mit Werten 0&lt;=(x|y)&lt;=1</returns>
        public GraphCoord GetRelative(GraphCoord absoluteCoord)
        {
            // Division durch Null vermeiden
            if (Width == 0.0f || Height == 0.0f)
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

    /// <summary>
    /// Repräsentiert eine polynomiale Funktion zweiten Grades der Form f(x) = ax^2 + bx + c
    /// </summary>
    public struct QuadPolynomial
    {
        /// <summary>
        /// Faktor a von x^2
        /// </summary>
        public Func<float,float> two;
        /// <summary>
        /// Faktor b von x
        /// </summary>
        public Func<float,float> one;
        /// <summary>
        /// Summand c
        /// </summary>
        public Func<float,float> zero;
        /// <summary>
        /// Funktionswert für geg. Eingabe
        /// </summary>
        /// <param name="xValue">Eingabewert</param>
        /// <returns></returns>
        public float FunctionValue(float xValue)
        {
            return (float)Math.Pow(xValue, 2.0) * two(xValue) + one(xValue) * xValue + zero(xValue);
        }
    }
}
