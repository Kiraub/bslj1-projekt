using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace canvas_test
{
    class ValueTable
    {

        public DataGrid myDataGrid;
        public DataTable myDataTable;

        public ValueTable()
        {
            myDataGrid = new DataGrid
            {
                CaptionText = "Das Data Grid",
                Dock = DockStyle.Fill
            };
            myDataTable = new DataTable();
            addColumn("R in Ohm");
            addColumn("U in V");
            addColumn("I in A");
            myDataGrid.DataSource = myDataTable;
        }

        public void addColumn(string name)
        {
            myDataTable.Columns.Add(new DataColumn(name));
        }

        public void addRow(string name)
        {
            myDataTable.NewRow();
        }

        public DataGrid addDataGrid()
        {
            //Create DataGrid
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

            //Create non-head columns
            for (int i = 0; i < (numColumns - 1); i++)
            {
                DataColumn clnRes = new DataColumn();
                clnRes.ColumnName = "column" + i.ToString();
                clnRes.Caption = i.ToString();

                dtResistance.Columns.Add(clnRes);

                DataColumn clnAmp = new DataColumn();
                clnAmp.ColumnName = "column" + i.ToString();
                clnAmp.Caption = i.ToString();

                dtAmperage.Columns.Add(clnAmp);

                DataColumn clnVol = new DataColumn();
                clnVol.ColumnName = "column" + i.ToString();
                clnVol.Caption = i.ToString();

                dtVoltage.Columns.Add(clnVol);
            }

            //Add Tables to DataSet
            myDataSet.Tables.Add(dtResistance);
            myDataSet.Tables.Add(dtAmperage);
            myDataSet.Tables.Add(dtVoltage);

        }

    }
}
