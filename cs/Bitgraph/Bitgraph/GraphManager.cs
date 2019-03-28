using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using ImageCoord = System.Drawing.Point;
using GraphCoord = System.Drawing.PointF;
using SimpleLine = System.Tuple<System.Drawing.PointF, System.Drawing.PointF, System.Drawing.Color>;
using Boundary = System.Tuple<float, float>;

namespace Bitgraph.Graph
{
    /// <summary>
    /// Repräsentiert die zusammengefasste Schnittstelle um Graphen zu zeichnen
    /// <para>Beinhaltet hauptsächlich visuelle Eigenschaften mit einer ausgelagerten Geometrie</para>
    /// <para>Das Bildobjekt arbeitet mit Integer-Pixel Koordinaten</para>
    /// <para>Die Geometrie-Berechnung arbeitet mit Float-Genauigkeit</para>
    /// <para>Kantenglättung ist nicht implementiert</para>
    /// </summary>
    public sealed class GraphManager
    {
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
        /// Vordergrundfarbe der visuellen Darstellungsfläche
        /// </summary>
        public Color ForegroundColor => Color.Black;
        /// <summary>
        /// Hintergrundfarbe der visuellen Darstellungsfläche
        /// </summary>
        public Color BackgroundColor => Color.Beige;
        /// <summary>
        /// Transparente "Farbe" der visuellen Darstellungsfläche
        /// </summary>
        public Color TransparentColor => Color.Transparent;

        /// <summary>
        /// Repräsentiert die geometrischen X-Achsen Limits des Geometrie-Objekts
        /// <para></para>
        /// <para>Löst bei setzen ein bedingtes Anpassen an das Parent aus</para>
        /// </summary>
        public Boundary HorizontalAxis
        {
            get { return new Boundary(Geometry.LowX, Geometry.HighX); }
            set { Geometry.SetLimitsX(value); FitToParent(); }
        }
        /// <summary>
        /// Repräsentiert die geomtrischen Y-Achsen Limits des Geometrie-Objekts
        /// <para></para>
        /// <para>Löst bei setzen ein bedingtes Anpassen an das Parent aus</para>
        /// </summary>
        public Boundary VerticalAxis
        {
            get { return new Boundary(Geometry.LowY, Geometry.HighY); }
            set { Geometry.SetLimitsY(value); FitToParent(); }
        }

        #endregion

        #region private instance fields

        /// <summary>
        /// Breite der visuellen Darstellungsfläche
        /// </summary>
        private int ImageWidth => Visualization.Width;
        /// <summary>
        /// Höhe der visuellen Darstellungsfläche
        /// </summary>
        private int ImageHeight => Visualization.Height;

        /// <summary>
        /// Container der Einstellungen des Graphen
        /// </summary>
        private GraphOptions Options { get; set; }
        /// <summary>
        /// Container der visuellen Schnittstelle
        /// </summary>
        private GraphVisualization Visualization { get; set; }
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
        /// </summary>
        public GraphManager(Control parent)
        {
            Options = new GraphOptions();
            Visualization = new GraphVisualization(BackgroundColor);
            Geometry = new GraphGeomtry();

            LblXAxis = new Label
            {
                BorderStyle = BorderStyle.None,
                Text = "X Achse",
                Parent = Visualization.Container,
                BackColor = BackgroundColor,
                AutoSize = true
            };
            LblYAxis = new Label
            {
                BorderStyle = BorderStyle.None,
                Text = "Y Achse",
                Parent = Visualization.Container,
                BackColor = BackgroundColor,
                AutoSize = true
            };

            LstLblMarkings = new List<Label>();
            LineMemory = new List<SimpleLine>();

            SetParent(parent);
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Hängt den Graphen an das gegebene Control-Element und fügt einen Event-Listener an dessen SizeChanged-Event
        /// </summary>
        /// <param name="newParent">Neues Parent Control-Element</param>
        public void SetParent(Control newParent)
        {
            Visualization.Parent = newParent;
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
        
        /// <summary>
        /// Zeichne eine Linie in den Graphen
        /// </summary>
        /// <param name="graphStart">Start-Koordinate</param>
        /// <param name="graphEnd">End-Koordinate</param>
        /// <param name="fillColor">Füllfarbe</param>
        /// <param name="is_redraw"><para>True: Linie wird nicht gespeichert</para><para></para><para>False: Linie wird gespeichert</para></param>
        public void PlotLine(GraphCoord graphStart, GraphCoord graphEnd, Color fillColor, bool is_redraw = false)
        {
            if (!is_redraw && RememberDrawing)
            {
                LineMemory.Add(new SimpleLine(graphStart, graphEnd, fillColor));
            }
            ImageCoord start = Graph2Image(graphStart);
            ImageCoord end = Graph2Image(graphEnd);
            Visualization.DrawLine(start, end, fillColor);
        }
        
        /// <summary>
        /// Zeichnet ein '+'-Kreuz in den Graphen
        /// </summary>
        /// <param name="coord">Koordinate des Kreuz-Zentrums</param>
        /// <param name="fillColor">Füllfarbe</param>
        /// <param name="is_redraw">True: Markierung brauch nicht gemerkt zu werden</param>
        /// <param name="scale">+/- Bereich um Koordinate der Kreuz-Linien</param>
        public void PlotMark(GraphCoord coord, Color fillColor, bool is_redraw, float scale = 0.75f)
        {
            float up = coord.Y + scale;
            float down = coord.Y - scale;
            float left = coord.X - scale;
            float right = coord.X + scale;
            PlotLine(new GraphCoord(left, coord.Y), new GraphCoord(right, coord.Y), fillColor, is_redraw);
            PlotLine(new GraphCoord(coord.X, down), new GraphCoord(coord.X, up), fillColor, is_redraw);
        }

        /// <summary>
        /// Zeichnet die Achsen des Graphen, diese werden nicht im LineMemory gespeichert
        /// </summary>
        public void PlotAxes()
        {
            foreach(Label lbl in LstLblMarkings)
            {
                lbl.Dispose();
            }
            LstLblMarkings.Clear();
            BorderStyle lblBorder = BorderStyle.None;
            Color lblBackground = TransparentLabels ? TransparentColor : BackgroundColor;
            int freeHoriSteps = 0;
            int freeVertiSteps = 0;
            int travelSteps = 0;
            int lblHeight = (int)Math.Floor(ImageHeight / (Geometry.Height / (Geometry.ScalingY + freeVertiSteps * Geometry.ScalingY)));
            int lblWidth = (int) Math.Floor(ImageWidth / (Geometry.Width / (Geometry.ScalingX + freeHoriSteps*Geometry.ScalingX)));
            while (lblWidth < 23)
            {
                freeHoriSteps += 1;
                lblWidth = (int)Math.Floor(ImageWidth / (Geometry.Width / (Geometry.ScalingX + freeHoriSteps * Geometry.ScalingX)));
            }
            lblWidth = lblWidth > 28 ? 28 : lblWidth;
            while (lblHeight < 15)
            {
                freeVertiSteps += 1;
                lblHeight = (int)Math.Floor(ImageHeight / (Geometry.Height / (Geometry.ScalingY + freeVertiSteps * Geometry.ScalingY)));
            }
            lblHeight = lblHeight > 20 ? 20 : lblHeight;
            PlotLine(new GraphCoord(Geometry.LowX, 0f), new GraphCoord(Geometry.HighX, 0f), ForegroundColor, true);
            PlotLine(new GraphCoord(0f, Geometry.LowY), new GraphCoord(0f, Geometry.HighY), ForegroundColor, true);
            for (float xPositive = Geometry.ScalingX; xPositive < Geometry.HighX + Geometry.ScalingX; xPositive += Geometry.ScalingX)
            {
                if(travelSteps==0)
                {
                    ImageCoord lblPos = Graph2Image(new GraphCoord(xPositive, -1.5f));
                    LstLblMarkings.Add(new Label { Text = xPositive.ToString(), BorderStyle = lblBorder, Parent = Visualization.Container, Top = lblPos.Y, Left = lblPos.X, Width = lblWidth, Height = lblHeight, Margin = new Padding(0), Padding = new Padding(0), BackColor = lblBackground });
                }
                travelSteps += 1;
                if(travelSteps>freeHoriSteps)
                {
                    travelSteps = 0;
                }
                PlotMark(new GraphCoord(xPositive, 0f), ForegroundColor, true, 1f);
                PlotMark(new GraphCoord(xPositive - Geometry.ScalingX / 2, 0f), ForegroundColor, true, 0.4f);
            }
            travelSteps = 0;
            for (float xNegative = -Geometry.ScalingX; xNegative > Geometry.LowX - Geometry.ScalingX; xNegative -= Geometry.ScalingX)
            {
                if (travelSteps == 0)
                {
                    ImageCoord lblPos = Graph2Image(new GraphCoord(xNegative, -1.5f));
                    LstLblMarkings.Add(new Label { Text = xNegative.ToString(), BorderStyle = lblBorder, Parent = Visualization.Container, Top = lblPos.Y, Left = lblPos.X, Width = lblWidth, Height = lblHeight, Margin = new Padding(0), Padding = new Padding(0), BackColor = lblBackground });
                }
                travelSteps += 1;
                if (travelSteps > freeHoriSteps)
                {
                    travelSteps = 0;
                }
                PlotMark(new GraphCoord(xNegative, 0f), ForegroundColor, true, 1f);
                PlotMark(new GraphCoord(xNegative + Geometry.ScalingX / 2, 0f), ForegroundColor, true, 0.4f);
            }
            travelSteps = 0;
            for (float yPositive = Geometry.ScalingY; yPositive < Geometry.HighY + Geometry.ScalingY; yPositive += Geometry.ScalingY)
            {
                if (travelSteps == 0)
                {
                    ImageCoord lblPos = Graph2Image(new GraphCoord(1.5f, yPositive));
                    LstLblMarkings.Add(new Label { Text = yPositive.ToString(), BorderStyle = lblBorder, Parent = Visualization.Container, Top = lblPos.Y, Left = lblPos.X, Width = 30, Height = lblHeight, Margin = new Padding(0), Padding = new Padding(0), BackColor = lblBackground });
                }
                travelSteps += 1;
                if (travelSteps > freeVertiSteps)
                {
                    travelSteps = 0;
                }
            PlotMark(new GraphCoord(0f, yPositive), ForegroundColor, true, 1f);
                PlotMark(new GraphCoord(0f, yPositive - Geometry.ScalingY / 2), ForegroundColor, true, 0.4f);
            }
            travelSteps = 0;
            for (float yNegative = -Geometry.ScalingY; yNegative > Geometry.LowY - Geometry.ScalingY; yNegative -= Geometry.ScalingY)
            {
                if (travelSteps == 0)
                {
                    ImageCoord lblPos = Graph2Image(new GraphCoord(1.5f, yNegative));
                    LstLblMarkings.Add(new Label { Text = yNegative.ToString(), BorderStyle = lblBorder, Parent = Visualization.Container, Top = lblPos.Y, Left = lblPos.X, Width = 30, Height = lblHeight, Margin = new Padding(0), Padding = new Padding(0), BackColor = lblBackground });
                }
                travelSteps += 1;
                if (travelSteps > freeVertiSteps)
                {
                    travelSteps = 0;
                }
                PlotMark(new GraphCoord(0f, yNegative), ForegroundColor, true, 1f);
                PlotMark(new GraphCoord(0f, yNegative + Geometry.ScalingY / 2), ForegroundColor, true, 0.4f);
            }
        }
        
        /// <summary>
        /// Zeichnet eine polynomielle Funktion zweiten Grades f(x)=ax^2+bx+c
        /// </summary>
        /// <param name="polynomial">Quadratisches Polynom</param>
        /// <param name="fillColor">Füllfarbe der Linie</param>
        /// <param name="stepAccuracy"><para>Genauigkeits-Multiplikator mit der f(x) gezeichnet wird</para><para></para><para>1.0 entspricht 1/200 Schrittweite</para></param>
        public void PlotPolynomialFunction(QuadPolynomial polynomial, Color fillColor, float stepAccuracy=10.0f)
        {
            float xLeft = Geometry.LowX;
            float xRight = Geometry.HighX;
            GraphCoord lastPoint = new GraphCoord(-1f, -1f);
            float stepIncrement = Geometry.Width / (200f * stepAccuracy);
            for( float xStep = xLeft; xStep <= xRight; xStep += stepIncrement)
            {
                float yStep = polynomial.FunctionValue(xStep);
                GraphCoord functionPoint = new GraphCoord(xStep, yStep);
                if ( xStep > xLeft )
                {
                    PlotLine(lastPoint, functionPoint, fillColor, false);
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
        
        /// <summary>
        /// Erstellt eine Liste von Ankreuzfeldern für vorhandene Graph-Einstellungen
        /// </summary>
        /// <returns>Dynamische Liste von Ankreuzfeldern</returns>
        public List<CheckBox> GetOptionsList()
        {
            CheckBox traCheckBox = Options.TransparentLabelsCheckBox;
            traCheckBox.Checked = TransparentLabels;
            traCheckBox.CheckedChanged += (object s, EventArgs e) => { TransparentLabels = traCheckBox.Checked; SetLabelTransparency(TransparentLabels); };
            CheckBox autoCheckBox = Options.AutoResizeCheckBox;
            autoCheckBox.Checked = AutoResize;
            autoCheckBox.CheckedChanged += (object s, EventArgs e) => { AutoResize = autoCheckBox.Checked; FitToParent(); };
            List<CheckBox> checkBoxList = new List<CheckBox>
            {
                traCheckBox, autoCheckBox
            };
            return checkBoxList;
        }

        /// <summary>
        /// Erstellt eine Liste von Knöpfen für vorhandene Graph-Aktionen im Bezug auf eine gegebene Datentabelle
        /// </summary>
        /// <param name="actionLinkedTable"></param>
        /// <returns>Dynamische Liste von Knöpfen</returns>
        public List<Button> GetActionsList(ValueTable actionLinkedTable)
        {
            Button drawPlotButton = new Button
            {
                Text = "Zeichne Graphen"
            };
            drawPlotButton.Click += (object s, EventArgs e) =>
            {
                ClearLineMemory();
                ForceResize();
                PlotPolynomialFunction(ValueTable.AmpereFunction, Options.PlotColors[0]);
                PlotPolynomialFunction(ValueTable.VoltageFunction, Options.PlotColors[1]);
                PlotPolynomialFunction(ValueTable.PowerFunction, Options.PlotColors[2]);
                ReplotGraph();
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
                        PlotMark(new GraphCoord((float)data[0, row], (float)data[column, row]), Options.PlotColors[column - 1], false);
                    }
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
            List<Button> buttonList = new List<Button>
            {
                drawPlotButton, drawDataMarksButton, deleteButton
            };
            return buttonList;
        }

        /// <summary>
        /// Erstellt eine Liste von Zahlfeldern für die Einstellung von geometrischen Limits und der Skalierung
        /// </summary>
        /// <returns>Dynamische List von Zahlfeldern</returns>
        public List<NumericUpDown> GetScales()
        {
            NumericUpDown Xmin = new NumericUpDown { Name = "X Minimum", Maximum = 500.0m, Minimum = -500.0m, DecimalPlaces = 1, Value = (decimal)Geometry.LowX };
            NumericUpDown Xmax = new NumericUpDown { Name = "X Maximum", Maximum = 500.0m, Minimum = -500.0m, DecimalPlaces = 1, Value = (decimal)Geometry.HighX };
            NumericUpDown Xstep = new NumericUpDown { Name = "X Intervall", Maximum = 100.0m, Minimum = 0.5m, DecimalPlaces = 1, Increment = 0.1m, Value = (decimal)Geometry.ScalingX };
            NumericUpDown Ymin = new NumericUpDown { Name = "Y Minimum", Maximum = 500.0m, Minimum = -500.0m, DecimalPlaces = 1, Value = (decimal)Geometry.LowY };
            NumericUpDown Ymax = new NumericUpDown { Name = "Y Maximum", Maximum = 500.0m, Minimum = -500.0m, DecimalPlaces = 1, Value = (decimal)Geometry.HighY };
            NumericUpDown Ystep = new NumericUpDown { Name = "Y Intervall", Maximum = 100.0m, Minimum = 0.5m, DecimalPlaces = 1, Increment = 0.1m, Value = (decimal)Geometry.ScalingY };

            Xmin.ValueChanged += (object sender, EventArgs e) => { HorizontalAxis = new Boundary((float)Xmin.Value, (float)Xmax.Value); Xmin.Value = (decimal)Geometry.LowX; };
            Xmax.ValueChanged += (object sender, EventArgs e) => { HorizontalAxis = new Boundary((float)Xmin.Value, (float)Xmax.Value); Xmax.Value = (decimal)Geometry.HighX; };
            Xstep.ValueChanged += (object sender, EventArgs e) => { Geometry.ScalingX = (float)Xstep.Value; ForceResize(); };
            Ymin.ValueChanged += (object sender, EventArgs e) => { VerticalAxis = new Boundary((float)Ymin.Value, (float)Ymax.Value); Ymin.Value = (decimal)Geometry.LowY; };
            Ymax.ValueChanged += (object sender, EventArgs e) => { VerticalAxis = new Boundary((float)Ymin.Value, (float)Ymax.Value); Ymax.Value = (decimal)Geometry.HighY; };
            Ystep.ValueChanged += (object sender, EventArgs e) => { Geometry.ScalingY = (float)Ystep.Value; ForceResize(); };

            List<NumericUpDown> numUpDownList = new List<NumericUpDown>
            {
                Xmin, Xmax, Xstep, Ymin, Ymax, Ystep
            };
            return numUpDownList;
        }

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
            Visualization.ResetAndResize();

            ImageCoord xLblPos = Graph2Image(new GraphCoord(Geometry.HighX, Geometry.ScalingY));
            ImageCoord yLblPos = Graph2Image(new GraphCoord(Geometry.ScalingX, Geometry.HighY));
            
            LblYAxis.Top = yLblPos.Y;
            LblYAxis.Left = yLblPos.X;// +LblYAxis.Width;
            
            LblXAxis.Top = xLblPos.Y;
            LblXAxis.Left = xLblPos.X-LblXAxis.Width;

            PlotAxes();
            ReplotGraph();
        }

        /// <summary>
        /// Konvertiert eine geometrische in eine visuelle Koordinate
        /// </summary>
        /// <param name="gc">geometrische Koordinate</param>
        /// <returns>visuelle Koordinate</returns>
        private ImageCoord Graph2Image(GraphCoord gc)
        {
            GraphCoord relative = Geometry.GetRelative(gc);
            // Die Ausrichtung der X-Achse der Bitmap stimmt mit der X-Achse des Graphen überein
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
        /// Zeichnet sämtliche Linien die derzeit im Gedächtnis sind
        /// </summary>
        private void ReplotGraph()
        {
            foreach (SimpleLine line in LineMemory)
            {
                PlotLine(line.Item1, line.Item2, line.Item3, true);
            }
        }

        #endregion
    }
}
