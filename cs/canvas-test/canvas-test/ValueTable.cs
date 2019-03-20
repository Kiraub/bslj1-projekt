using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace canvas_test
{
    class ValueTable : IDisposable
    {

        public DataGridView myDataGridView;
        public DataTable myDataTable;
        public List<DataColumn> myDataColumns;

        public ValueTable()
        {
            myDataTable = new DataTable();
            myDataColumns = new List<DataColumn>();
            myDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = myDataTable
            };
            AddColumn("R in Ohm", false);
            AddColumn("U in V");
            AddColumn("I in A");
            myDataGridView.DataSource = myDataTable;
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

        public void Dispose()
        {
            foreach( DataColumn dc in myDataColumns)
            {
                dc.Dispose();
            }
            myDataTable.Dispose();
            myDataGridView.Dispose();
        }
    }
}
