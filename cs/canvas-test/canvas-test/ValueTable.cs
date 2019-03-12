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

        public void addDataSet(int numColumns)
        {
            DataSet myDataSet = new DataSet();
            DataTable dtResistance = new DataTable();
            DataTable dtAmperage = new DataTable();
            DataTable dtVoltage = new DataTable();

            DataColumn headRes = new DataColumn();
            headRes.ColumnName = "headResistance";
            headRes.Caption = "R(L)";
            headRes.ReadOnly = true;
            dtResistance.Columns.Add(headRes);

            DataColumn headAmp = new DataColumn();
            headAmp.ColumnName = "headAmperage";
            headAmp.Caption = "I";
            headAmp.ReadOnly = true;
            dtAmperage.Columns.Add(headAmp);

            DataColumn headVol = new DataColumn();
            headVol.ColumnName = "headVoltage";
            headVol.Caption = "U";
            headVol.ReadOnly = true;
            dtVoltage.Columns.Add(headVol);

            for(int i = 0; i < (numColumns - 1); i++)
            {
                DataColumn cln = new DataColumn();
                cln.ColumnName = "column" + i.ToString();
                cln.Caption = i.ToString();

                dtResistance.Columns.Add(cln);
                dtAmperage.Columns.Add(cln);
                dtVoltage.Columns.Add(cln);
            }

            myDataSet.Tables.Add(dtResistance);
            myDataSet.Tables.Add(dtAmperage);
            myDataSet.Tables.Add(dtVoltage);
        }

    }
}
