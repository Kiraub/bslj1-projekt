using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bitgraph
{
    public partial class BitgraphGUI : Form
    {

        private SplitContainer SplitVert { get; set; }
        private SplitContainer SplitHori { get; set; }

        private TabControl TabCtrl { get; set; }
        private TabPage PageOptions { get; set; }
        private TabPage PageImage { get; set; }
        private TabPage PageHelp { get; set; }
        private TabPage PageAbout { get; set; }
        private string[] PageTitles = { "Optionen", "Schaltung", "Hilfe", "Über" };

        private GraphManager GraphMan { get; set; }
        private ValueTable ValueTbl { get; set; }

        private List<CheckBox> OptionValues { get; set; }
        private List<Button> ActionButtons { get; set; }
        private List<NumericUpDown> GraphScales { get; set; }

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
            PageImage = new TabPage(PageTitles[1]);
            PageHelp = new TabPage(PageTitles[2]);
            PageAbout = new TabPage(PageTitles[3]);

            GraphMan = new GraphManager();
            ValueTbl = new ValueTable();
        }

        private void BitgraphGUI_Load(object sender, EventArgs e)
        {
            SplitVert.SplitterDistance = Convert.ToInt32(Math.Round(Width * 0.6, 0));
            SplitHori.SplitterDistance = Convert.ToInt32(Math.Round(Height * 0.5, 0));

            TabCtrl.TabPages.Add(PageOptions);
            TabCtrl.TabPages.Add(PageImage);
            TabCtrl.TabPages.Add(PageHelp);
            TabCtrl.TabPages.Add(PageAbout);

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
                //cbx.Dock = DockStyle.Top;
                top += 30;
            }

            ActionButtons = GraphMan.GetActionsList(ValueTbl);
            foreach ( Button btn in ActionButtons)
            {
                btn.Parent = PageOptions;
                btn.Top = top;
                btn.Left = left;
                btn.Width = width;
                //btn.Dock = DockStyle.Top;
                top += 30;
            }

            top += 30;
            left += 20;
            GraphScales = GraphMan.GetScales();
            // Half should be the break between X and Y relevant values
            int element = 1;
            width = (width / (GraphScales.Count / 2))-20;
            foreach ( NumericUpDown numUpDown in GraphScales)
            {
                numUpDown.Parent = PageOptions;
                numUpDown.Top = top;
                numUpDown.Left = left+(width+20)*(2-(element % (GraphScales.Count / 2)));
                numUpDown.Width = width;
                if(element % (GraphScales.Count/2) == 0)
                {
                    top += 30;
                }
                element += 1;
            }
        }
    }
}
