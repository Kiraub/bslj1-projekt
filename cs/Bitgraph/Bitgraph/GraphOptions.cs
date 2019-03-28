using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bitgraph.Graph
{
    /// <summary>
    /// Container-Klasse für Optionen des GraphMangers
    /// </summary>
    sealed class GraphOptions
    {
        /// <summary>
        /// Transparente Hintergründe der Graphenbeschriftungen
        /// </summary>
        public bool TransparentLabels { get; set; } = true;

        /// <summary>
        /// Automatisches neu-Zeichnen des Graphen bei geg. Event
        /// </summary>
        public bool AutoResize { get; set; } = true;

        /// <summary>
        /// Schaltet Gedächtnis für Linien an/aus
        /// <para></para>
        /// <para>Momentan als nicht vom Nutzer änderbar vorgesehen</para>
        /// </summary>
        public bool RememberDrawing { get; set; } = true;

        /// <summary>
        /// Farben der Graph-Plots
        /// </summary>
        public List<Color> PlotColors = new List<Color> { Color.Red, Color.Blue, Color.Green };

        /// <summary>
        /// Ankreuzfeld für die TransparentLabels Einstellung
        /// </summary>
        public CheckBox TransparentLabelsCheckBox => new CheckBox { Checked = TransparentLabels, Text = "Transparente Beschriftung" };
        /// <summary>
        /// Ankreuzfeld für die AutoResize Einstellung
        /// </summary>
        public CheckBox AutoResizeCheckBox => new CheckBox { Checked = AutoResize, Text = "Echtzeit Größenanpassung" };
        /// <summary>
        /// Ankreuzfeld für die RememberDrawing Einstellung
        /// </summary>
        public CheckBox RememberDrawingCheckBox => new CheckBox { Checked = RememberDrawing, Text = "Merke gezeichnete Linien" };
    }
}
