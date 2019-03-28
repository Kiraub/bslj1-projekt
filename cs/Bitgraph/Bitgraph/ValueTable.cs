using System;
using System.Data;
using System.Windows.Forms;

namespace Bitgraph
{
    /// <summary>
    /// Container Klasse, welche eine interaktive Datentabelle und META-Informationen kapselt
    /// </summary>
    public sealed class ValueTable : IDisposable
    {
        #region public class constants

        /// <summary>
        /// Namen der Tabellenspalten
        /// <para>R_1 = U_0 / I_0</para>
        /// <para>I_1 = U_0 / R_1</para>
        /// <para>U_1 = R_1 * I_1</para>
        /// <para>P_1 = U_1 * I_1 || U_1^2 / R_1 || I_1^2 * R_1</para>
        /// </summary>
        public static readonly string[] ColumnNames = { "R\u2081 in Ohm", "I\u2081 (R\u2081) in Ampere", "U\u2081 (R\u2081) in Volt", "P\u2081 (R\u2081) in Watt" };
        /// <summary>
        /// Spannung U_0 der Stromquelle
        /// </summary>
        public static readonly double VoltageSource = 10.0;

        /// <summary>
        /// Funktion zur Berechnung von I_1 = U_0 / R_1
        /// </summary>
        public static readonly QuadPolynomial AmpereFunction = new QuadPolynomial
        {
            two = (_) => 0f,
            one = (x) => (float) (VoltageSource/Math.Pow(x,2.0)),
            zero = (_) => 0f
        };

        /// <summary>
        /// Funktion zur Berechnung von U_1 = R_1 * I_1
        /// </summary>
        public static readonly QuadPolynomial VoltageFunction = new QuadPolynomial
        {
            two = (_) => 0f,
            one = (x) => AmpereFunction.FunctionValue(x),
            zero = (_) => 0f
        };

        /// <summary>
        /// Funktion zur Berechnung von P_1 = U_1 * I_1
        /// </summary>
        public static readonly QuadPolynomial PowerFunction = new QuadPolynomial
        {
            two = (_) => 0f,
            one = (x) => { return (AmpereFunction.FunctionValue(x) * VoltageFunction.FunctionValue(x)) / x; },
            zero = (_) => 0f
        };

        #endregion

        #region public instance fields

        /// <summary>
        /// Container und graphische Repräsentation der Datentabelle
        /// </summary>
        public DataGridView MyDataGridView { get; set; }

        #endregion

        #region private instance fields

        /// <summary>
        /// Datentabellen-Objekt
        /// </summary>
        private DataTable MyDataTable;

        #endregion

        /// <summary>
        /// Container für die Datentabelle
        /// </summary>
        public ValueTable()
        {
            MyDataTable = new DataTable();
            MyDataGridView = new DataGridView
            {
                DataSource = MyDataTable,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
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

        #region public instance methods

        /// <summary>
        /// Setzt Parent Element des Tabellen-Containers
        /// </summary>
        /// <param name="parent">Neues Parent Element</param>
        public void SetParent(Control parent)
        {
            MyDataGridView.Parent = parent;
            MyDataGridView.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Gibt für das Objekt reservierte Ressourcen wieder frei
        /// </summary>
        public void Dispose()
        {
            MyDataTable.Dispose();
            MyDataGridView.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gibt den momentanen Tabelleninhalt zurück
        /// </summary>
        /// <returns>Matrix mit allen Zell-Werten</returns>
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

        #endregion

        #region private instance methods

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

        private void AddColumn(string name, bool readOnly = true)
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

        #endregion
    }
}
