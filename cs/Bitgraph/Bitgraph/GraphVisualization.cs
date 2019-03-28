using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ImageCoord = System.Drawing.Point;
using Image = System.Drawing.Bitmap;
using ImageContainer = System.Windows.Forms.PictureBox;

namespace Bitgraph.Graph
{
    sealed class GraphVisualization
    {
        #region private class constants

        /// <summary>
        /// Default-Breite der visuellen Darstellungsfläche in Pixel
        /// </summary>
        private static readonly int DEFAULT_WIDTH = 100;

        /// <summary>
        /// Default-Höhe der visuellen Darstellungsfläche in Pixel
        /// </summary>
        private static readonly int DEFAULT_HEIGHT = 100;

        #endregion

        #region public instance fields

        /// <summary>
        /// Stellvertreter Control-Objekt für den Container
        /// </summary>
        public Control Container { get { return DrawAreaContainer; } }
        /// <summary>
        /// Stellvertreter Parent-Objekt für den Container
        /// </summary>
        public Control Parent { get { return DrawAreaContainer.Parent; } set { DrawAreaContainer.Parent = value; } }
        
        /// <summary>
        /// Breite der visuellen Darstellungsfläche
        /// </summary>
        public int Width => DrawArea.Width;

        /// <summary>
        /// Höhe der visuellen Darstellungsfläche
        /// </summary>
        public int Height => DrawArea.Height;

        #endregion

        #region private instance fields

        /// <summary>
        /// Repräsentiert die visuelle Darstellungsfläche
        /// </summary>
        private Image DrawArea { get; set; }

        /// <summary>
        /// Container der visuellen Darstellungsfläche
        /// </summary>
        private ImageContainer DrawAreaContainer { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Schnittstelle für die Visualisierung des Graphen
        /// </summary>
        /// <param name="background"></param>
        public GraphVisualization(Color background)
        {
            DrawArea = new Image(DEFAULT_WIDTH, DEFAULT_HEIGHT);

            DrawAreaContainer = new ImageContainer
            {
                Image = DrawArea,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = background
            };
        }

        #endregion

        #region public instance methods

        /// <summary>
        /// Zeichne eine Linie in das Bild
        /// </summary>
        /// <param name="start">Start-Koordinate</param>
        /// <param name="end">End-Koordinate</param>
        /// <param name="fillColor">Füllfarbe</param>
        public void DrawLine(ImageCoord start, ImageCoord end, Color fillColor)
        {
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
        /// Passt Container und Darstellungsfläche an Parent an und setzt die Darstellungsfläche zurück
        /// </summary>
        public void ResetAndResize()
        {
            DrawAreaContainer.Dock = DockStyle.Fill;
            int newWidth = DrawAreaContainer.Width;
            int newHeight = DrawAreaContainer.Height;
            DrawArea = new Image(newWidth, newHeight);
            DrawAreaContainer.Image = DrawArea;
        }

        #endregion

        #region private instance methods

        /// <summary>
        /// Setze einen Bildpunkt der Darstellungsfläche
        /// </summary>
        /// <param name="imageCoord">Bild-Koordinaten des Punktes</param>
        /// <param name="fill">Füllfarbe</param>
        private void SetPixel(ImageCoord imageCoord, Color fill)
        {
            if (imageCoord.X >= 0 && imageCoord.X < Width && imageCoord.Y >= 0 && imageCoord.Y < Height)
            {
                DrawArea.SetPixel(imageCoord.X, imageCoord.Y, fill);
            }
        }

        /// <summary>
        /// Zeichnet eine horizontale Linie in das Bild
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
        /// Zeichnet eine vertikale Linie in das Bild
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

        #endregion

    }
}
