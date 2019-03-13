using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace canvas_test
{
    class ValueTable
    {

        public DataGrid myDataGrid;
        public DataTable myDataTable;
        public List<DataColumn> myDataColumns;

        public ValueTable()
        {
            myDataGrid = new DataGrid
            {
                CaptionText = "Das Data Grid",
                Dock = DockStyle.Fill
            };
            myDataTable = new DataTable();
            myDataColumns = new List<DataColumn>();
            AddColumn("R in Ohm", false);
            AddColumn("U in V");
            AddColumn("I in A");
            myDataGrid.DataSource = myDataTable;
        }

        public void AddColumn(string name, bool readOnly=true)
        {
            DataColumn dc = new DataColumn
            {
                ColumnName = name,
                AllowDBNull = false,
                DataType = Type.GetType("System.Double"),
                ReadOnly = readOnly,
                DefaultValue = 0f
            };
            myDataColumns.Add(dc);
            myDataTable.Columns.Add(dc);
        }
    }
}
