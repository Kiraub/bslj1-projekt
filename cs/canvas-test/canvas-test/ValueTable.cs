using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace canvas_test
{
    class ValueTable
    {
        public DataGrid addDataGrid()
        {
            DataGrid myDataGrid = new DataGrid();
            myDataGrid.Location = new Point(50, 50);
            myDataGrid.Size = new Size(300, 200);
            myDataGrid.CaptionText = "Das Data Grid";

            return myDataGrid;
        }

        public void addDataSet()
        {
            DataSet myDataSet = new DataSet();
            DataTable dtResistance = new DataTable();
            DataTable dtAmperage = new DataTable();
            DataTable dtVoltage = new DataTable();

            myDataSet.Tables.Add(dtResistance);
            myDataSet.Tables.Add(dtAmperage);
            myDataSet.Tables.Add(dtVoltage);
        }

    }
}
