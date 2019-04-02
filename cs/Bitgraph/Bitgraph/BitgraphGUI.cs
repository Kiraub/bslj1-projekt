using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bitgraph
{
    /// <summary>
    /// Klasse zur Kapselung und Darstellung der Programm Komponenten
    /// </summary>
    public partial class BitgraphGUI : Form
    {
        #region private class fields

        private static string HelpText =>
            "\n" +
            "Begriffe:\n" +
            "\n" +
            "# Oberfläche:\n" +
            "    \u2192 Links: Graph\n" +
            "    \u2192 Rechts:\n" +
            "        \u21d2 Oben:  Menü\n" +
            "        \u21d2 Unten: Tabelle\n" +
            "\n" +
            "Nutzungshinweise:\n" +
            "\n" +
            "# Graph:\n" +
            "\n" +
            "    \u2192 Legende:\n" +
            "\n" +
            "An den Graphenachsen finden sich Farblegenden und Beschriftungen\n" +
            "\n" +
            "# Menü:\n" +
            "\n" +
            "    \u2192 Optionen:\n" +
            "\n" +
            "        \u21d2 Ankreuzfeld: <Transparente Beschriftung>\n" +
            "Steuert Hintergrund-Transparenz der Beschriftung des Graphen\n" +
            "\n" +
            "        \u21d2 Ankreuzfeld: <Echtzeit Größenanpassung>\n" +
            "Steuert Echtzeit-Neuzeichnen des Graphen bei Veränderung der Fenstergröße\n" +
            "\n" +
            "        \u21d2 Knopf: <Zeichne Graphen>\n" +
            "Plottet die Funktionslinien der physikalischen Zusammenhänge\n" +
            "\n" +
            "        \u21d2 Knopf: <Zeichne Datenpunkte>\n" +
            "Plottet die Werte, welche in die Tabelle eingetragen wurden\n" +
            "\n" +
            "        \u21d2 Knopf: <Lösche Graphen>\n" +
            "Löscht sämtliche bisher geplottete Elemente\n" +
            "\n" +
            "        \u21d2 Zahlfelder: <X/Y Max-/ Minimum>\n" +
            "Steuert den dargestellten Bereich der X/Y-Achse\n" +
            "Limits sind \u00b1 500\n" +
            "\n" +
            "        \u21d2 Zahlfelder: <X/Y Intervall>\n" +
            "Steuert die dargestellte Graphskala der X/Y-Achse\n" +
            "Limits sind +0.5 bis +100\n" +
            "\n" +
            "    \u2192 Schaltung:\n" +
            "\n" +
            "Schemata der Schaltung, welche den Berechnungen zugrunde liegt\n" +
            "\n" +
            "    \u2192 Hilfe:\n" +
            "\n" +
            "Diese Hilfeseite\n" +
            "\n" +
            "    \u2192 Über:\n" +
            "\n" +
            "Meta-Informationen über das Programm und die Entwickler\n" +
            "\n" +
            "# Tabelle:\n" +
            "\n" +
            "    \u2192 Allgemein geg. Wert für Spannung U\u2080 = 10V\n" +
            "\n" +
            "    \u2192 Spalte "+ValueTable.ColumnNames[0]+":\n" +
            "In diese Spalte können Gleitkommazahlen eingetragen werden, es kann jeder Wert jedoch nur einmal existieren\n" +
            "\n" +
            "    \u2192 Spalte "+ValueTable.ColumnNames[1]+"\n" +
            "In dieser Spalte erscheinen Werte Anhand der Formel:\n" +
            "I\u2081 = U\u2080 \u00f7 R\u2081\n" +
            "\n" +
            "    \u2192 Spalte "+ValueTable.ColumnNames[2]+"\n" +
            "In dieser Spalte erscheinen Werte Anhand der Formel:\n" +
            "U\u2081 = R\u2081 \u00d7 I\u2081\n" +
            "\n" +
            "    \u2192 Spalte "+ValueTable.ColumnNames[3]+"\n" +
            "In dieser Spalte erscheinen Werte Anhand der Formel:\n" +
            "P\u2081 = U\u2081\u00b2 \u00d7 R\u2081\n" +
            "\n" +
            "    \u2192 Löschen von Eingaben\n" +
            "Auswahl von Zeilen durch Linksklick auf die leere linke Spalte und betätigen der 'Entf'-Taste\n" +
            "\n";
        private static string AboutText =>
            "\n" +
            "Bitgraph ist ein Programm zum Darstellen von mathematischen Zusammenhängen grundlegender physikalischer Größen der Elektrotechnik.\n" +
            "\n" +
            "  Es wurde entwickelt von:\n" +
            "\n" +
            "    Erik Bauer\n" +
            "    Henning Meyer\n" +
            "\n" +
            "  im Rahmen des Projektes im 1. Lehrjahr:\n" +
            "\n" +
            "    Leistungs-, Spannungs- und Stromanpassung\n" +
            "\n" +
            "  an der Einrichtung:\n" +
            "\n" +
            "    Berufliche Schule der Hansestadt Rostock -Technik-\n" +
            "\n" +
            "    Anschrift:\n" +
            "    Fritz-Triddelfitz-Weg 1F\n" +
            "    18069 Rostock\n" +
            "\n" +
            "    Kontakt:\n" +
            "    Tel.:   0381 38141100\n" +
            "    Fax:    0381 38141103\n" +
            "    E-Mail: info @bs-technik-rostock.de\n" +
            "    Web:    www.bs-technik-rostock.de\n" +
            "\n";

        #endregion

        #region private instance fields

        private bool Maximised = false;

        private SplitContainer SplitVert { get; set; }
        private SplitContainer SplitHori { get; set; }

        private TabControl TabCtrl { get; set; }
        private TabPage PageOptions { get; set; }
        private TabPage PageSchema { get; set; }
        private TabPage PageHelp { get; set; }
        private TabPage PageAbout { get; set; }
        private string[] PageTitles = { "Optionen", "Schaltung", "Hilfe", "Über" };

        private Graph.GraphManager GraphMan { get; set; }
        private ValueTable ValueTbl { get; set; }

        private List<CheckBox> OptionValues { get; set; }
        private List<Button> ActionButtons { get; set; }
        private List<NumericUpDown> GraphScales { get; set; }
        private List<Label> GraphScalesText { get; set; }

        private PictureBox SchemaContainer { get; set; }

        private RichTextBox HelpTextBox { get; set; }
        private RichTextBox AboutTextBox { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Windows-Forms Objekt, welches als Haupt-Container sämtlicher GUI Elemente dient
        /// </summary>
        public BitgraphGUI()
        {
            InitializeComponent();

            SplitVert = new SplitContainer
            {
                Parent = this,
                IsSplitterFixed = true,
                Dock = DockStyle.Fill
            };
            SplitHori = new SplitContainer
            {
                Parent = SplitVert.Panel2,
                Orientation = Orientation.Horizontal,
                IsSplitterFixed = true,
                Dock = DockStyle.Fill
            };

            TabCtrl = new TabControl
            {
                Parent = SplitHori.Panel1,
                Dock = DockStyle.Fill
            };
            PageOptions = new TabPage(PageTitles[0]);
            PageSchema = new TabPage(PageTitles[1]);
            PageHelp = new TabPage(PageTitles[2]);
            PageAbout = new TabPage(PageTitles[3]);

            GraphMan = new Graph.GraphManager(SplitHori.Panel1);
            ValueTbl = new ValueTable();

            GraphScalesText = new List<Label>();

            SchemaContainer = new PictureBox
            {
                Parent = PageSchema,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Image = Properties.Resources.SchemaResource,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            HelpTextBox = new RichTextBox
            {
                Text = HelpText,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Parent = PageHelp,
                Font = new Font(FontFamily.GenericSansSerif, 8f),
                ShortcutsEnabled = false
            };
            AboutTextBox = new RichTextBox
            {
                Text = AboutText,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Parent = PageAbout,
                ShortcutsEnabled = false
            };
        }

        #endregion

        #region private instance methods

        private void BitgraphGUI_Load(object sender, EventArgs e)
        {
            Size = new Size(960, 540);
            Maximised = WindowState == FormWindowState.Maximized;

            SplitVert.SplitterDistance = Convert.ToInt32(Math.Round(Width-320f, 0));
            SplitHori.SplitterDistance = Convert.ToInt32(Math.Round(Height*0.5f, 0));

            TabCtrl.TabPages.Add(PageOptions);
            TabCtrl.TabPages.Add(PageSchema);
            TabCtrl.TabPages.Add(PageHelp);
            TabCtrl.TabPages.Add(PageAbout);

            GraphMan.LblXAxis.Text = ValueTable.ColumnNames[0];
            GraphMan.LblYAxis.Text =
                ValueTable.ColumnNames[1] + ",  Farbe Rot\n" +
                ValueTable.ColumnNames[2] + ",  Farbe Blau\n"+
                ValueTable.ColumnNames[3] + ",  Farbe Grün";

            GraphMan.SetParent(SplitVert.Panel1);
            ValueTbl.SetParent(SplitHori.Panel2);

            OptionValues = GraphMan.GetOptionsList();
            int top = 10;
            int left = 5;
            int width = PageOptions.Width - 10;
            foreach ( CheckBox cbx in OptionValues)
            {
                cbx.Parent = PageOptions;
                cbx.Top = top;
                cbx.Left = left;
                cbx.Width = width;
                top += 30;
            }

            ActionButtons = GraphMan.GetActionsList(ValueTbl);
            foreach ( Button btn in ActionButtons)
            {
                btn.Parent = PageOptions;
                btn.Top = top;
                btn.Left = left;
                btn.Width = width;
                top += 30;
            }

            top += 20;
            GraphScales = GraphMan.GetScales();
            // Half is the break between X and Y relevant values
            int element = 1;
            width = (width / (GraphScales.Count / 2))-20;
            foreach ( NumericUpDown numUpDown in GraphScales)
            {
                numUpDown.Parent = PageOptions;
                numUpDown.Top = top;
                numUpDown.Left = left+(width+20)*(2-(element % (GraphScales.Count / 2)));
                numUpDown.Width = width;
                Label lbl = new Label { Parent=PageOptions, BackColor=Color.Transparent, Text = numUpDown.Name, Top = numUpDown.Top - 20, Left = numUpDown.Left, Width=numUpDown.Width };
                GraphScalesText.Add(lbl);
                if(element % (GraphScales.Count/2) == 0)
                {
                    top += 45;
                }
                element += 1;
            }
        }

        private void BitgraphGUI_SizeChanged(object sender, EventArgs e)
        {
            SplitVert.SplitterDistance = Convert.ToInt32(Math.Round(Width - 320f, 0));
            SplitHori.SplitterDistance = Convert.ToInt32(Math.Round(Height * 0.5f, 0));
        }

        private void BitgraphGUI_ResizeEnd(object sender, EventArgs e)
        {
            GraphMan.ForceResize();
        }

        private void BitgraphGUI_ClientSizeChanged(object sender, EventArgs e)
        {
            if (Maximised != (WindowState == FormWindowState.Maximized))
            {
                GraphMan.ForceResize();
                Maximised =! Maximised;
            }
        }

        #endregion
    }
}
