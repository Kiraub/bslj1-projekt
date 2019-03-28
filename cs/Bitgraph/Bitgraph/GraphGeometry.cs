using GraphCoord = System.Drawing.PointF;
using Boundary = System.Tuple<float, float>;

namespace Bitgraph.Graph
{
    sealed class GraphGeomtry
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

        #region constructors

        /// <summary>
        /// Schnittstelle für die Geometrie des Graphen
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
            // avoid divide by zero
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
    }
}
