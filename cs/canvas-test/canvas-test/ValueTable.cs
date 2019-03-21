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
        /// <summary>
        /// <para>R_L = U_S / I</para>
        /// <para>I   = U_S / R_L</para>
        /// <para>U_L = R_L * I</para>
        /// <para>P_L = U_L * I || U_L^2 / R_L || I^2 * R_L</para>
        /// </summary>
        public static readonly string[] ColumnNames = { "R_L in Ohm",  "I in A", "U_L in V", "P_L in Watt" };
        public static readonly double VoltageSource = 10.0;
        
        public static readonly QuadPolynomial AmpereFunction = new QuadPolynomial
        {
            two = (_) => 0f,
            one = (x) => (float) (VoltageSource/Math.Pow(x,2.0)),
            zero = (_) => 0f
        };

        public static readonly QuadPolynomial VoltageFunction = new QuadPolynomial
        {
            two = (_) => 0f,
            one = (x) => AmpereFunction.FunctionValue(x),
            zero = (_) => 0f
        };

        public static readonly QuadPolynomial PowerFunction = new QuadPolynomial
        {
            two = (_) => 0f,
            one = (x) => { return (AmpereFunction.FunctionValue(x) * VoltageFunction.FunctionValue(x)) / x; },
            zero = (_) => 0f
        };

        public Color[] ColumnColors = { Color.Black, Color.Red, Color.Blue, Color.Green };
        public DataGridView MyDataGridView { get; set; }
        private DataTable MyDataTable;

        public ValueTable()
        {
            MyDataTable = new DataTable();
            MyDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = MyDataTable,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };
            AddColumn(ColumnNames[0], false);
            AddColumn(ColumnNames[1]);
            AddColumn(ColumnNames[2]);
            AddColumn(ColumnNames[3]);

            MyDataTable.DefaultView.Sort = ColumnNames[0] + " " + "ASC";
            MyDataTable.ColumnChanged += MyDataTable_ColumnChanged;

            MyDataGridView.DataSource = MyDataTable;
            MyDataGridView.DataError += MyDataGridView_DataError;

        }

        private void MyDataTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            //System.Diagnostics.Debug.Print("Row_Changed Event: row:{0}; column={1}", e.Row, e.Column);
            if(e.Column.ColumnName.Equals(ColumnNames[0]) )
            {
                //System.Diagnostics.Debug.Print("Change other values");
                double ohm = (double)e.Row[ColumnNames[0]];
                double ampere = VoltageSource / ohm;
                double voltage = ohm * ampere;
                double power = voltage * ampere;
                SetReadOnly(false);
                e.Row[ColumnNames[1]] = ampere;
                e.Row[ColumnNames[2]] = voltage;
                e.Row[ColumnNames[3]] = power;
                SetReadOnly(true);
                MyDataGridView.Invalidate();
            }
        }

        private void MyDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // do nothing on a data error since the illegal row will be automatically deleted
            //System.Diagnostics.Debug.Print("Data error:\n" + e.ToString());
            return;
        }

        private void SetReadOnly(bool readOnly)
        {
            foreach (DataColumn dc in MyDataTable.Columns)
            {
                if (dc.ColumnName.Equals(ColumnNames[0])) continue;
                dc.ReadOnly = readOnly;
            }
        }

        public void AddColumn(string name, bool readOnly = true)
        {
            DataColumn dc = new DataColumn
            {
                ColumnName = name,
                AllowDBNull = false,
                DataType = Type.GetType("System.Double"),
                ReadOnly = readOnly,
                DefaultValue = 0f,
                Unique = !readOnly // if user writes data, ensure uniqueness
            };
            MyDataTable.Columns.Add(dc);
        }

        public void Dispose()
        {
            MyDataTable.Dispose();
            MyDataGridView.Dispose();
        }

        public double[,] GetTableContents()
        {
            int columnCount = MyDataTable.Columns.Count;
            int rowCount = MyDataTable.Rows.Count;
            double[,] contents = new double[columnCount, rowCount];
            for (int cc = 0; cc < columnCount; cc += 1)
            {
                for (int rr = 0; rr < rowCount; rr += 1)
                {
                    contents[cc, rr] = (double)MyDataTable.Rows[rr][ColumnNames[cc]];
                }
            }
            return contents;
        }
    }
}
