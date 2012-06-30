using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace CsDO.Lib.MockDriver
{
    public class MockDataReader : DbDataReader
    {

        #region Private Vars
        private DataTable[] _data = null;
        private int[] _row = null;
        private int _current = 0;
        private int _recordsAffected = -1;

        private bool _readerClosed = false;

        private IDbConnection _connectionToClose = null;
        #endregion

        #region Constructors
        public MockDataReader(DataTable data, int recordsAffected)
        {
            DataTable[] param = { data };
            SetInternalState(param, recordsAffected);
        }

        public MockDataReader(DataSet data, int recordsAffected)
        {
            DataTable[] param = new DataTable[data.Tables.Count];

            int i = 0;
            foreach (DataTable table in data.Tables)
            {
                param[i] = table;
                i++;
            }

            SetInternalState(param, recordsAffected);
        }

        public MockDataReader(DataTable[] data, int recordsAffected)
        {
            SetInternalState(data, recordsAffected);
        }
        #endregion

        #region Properties
        public IDbConnection ConnectionToClose
        {
            get
            {
                return _connectionToClose;
            }
            set
            {
                _connectionToClose = value;
            }
        }

        public override int FieldCount
        {
            get
            {
                return this.Data.Columns.Count;
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                return GetValue(ordinal);
            }
        }

        public override object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public override int Depth
        {
            get
            {
                return 0;
            }
        }

        public override bool IsClosed
        {
            get
            {
                return _readerClosed;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return _recordsAffected;
            }
        }

        public override bool HasRows
        {
            get
            {
                return (!(this.Row + 1 > this.Data.Rows.Count - 1));
            }
        }

        private DataTable Data
        {
            get
            {
                return _data[_current];
            }
        }

        private int Row
        {
            get
            {
                return _row[_current];
            }
            set
            {
                _row[_current] = value;
            }
        }
        #endregion

        #region Public Methods

        public override bool GetBoolean(int ordinal)
        {
            return Convert.ToBoolean(this.Data.Rows[this.Row][ordinal]);
        }

        public override byte GetByte(int ordinal)
        {
            return Convert.ToByte(this.Data.Rows[this.Row][ordinal]);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            return Convert.ToChar(this.Data.Rows[this.Row][ordinal]);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            return GetFieldType(ordinal).FullName;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return Convert.ToDateTime(this.Data.Rows[this.Row][ordinal]);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return Convert.ToDecimal(this.Data.Rows[this.Row][ordinal]);
        }

        public override double GetDouble(int ordinal)
        {
            return Convert.ToDouble(this.Data.Rows[this.Row][ordinal]);
        }

        public override Type GetFieldType(int ordinal)
        {
            return this.Data.Columns[ordinal].DataType;
        }

        public override float GetFloat(int ordinal)
        {
            return (float)GetDouble(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return new Guid(GetString(ordinal));
        }

        public override short GetInt16(int ordinal)
        {
            return Convert.ToInt16(this.Data.Rows[this.Row][ordinal]);
        }

        public override int GetInt32(int ordinal)
        {
            return Convert.ToInt32(this.Data.Rows[this.Row][ordinal]);
        }

        public override long GetInt64(int ordinal)
        {
            return Convert.ToInt64(this.Data.Rows[this.Row][ordinal]);
        }

        public override string GetName(int ordinal)
        {
            return this.Data.Columns[ordinal].ColumnName;
        }

        public override int GetOrdinal(string name)
        {
            return this.Data.Columns[name].Ordinal;
        }

        public override string GetString(int ordinal)
        {
            return Convert.ToString(this.Data.Rows[this.Row][ordinal]);
        }

        public override object GetValue(int ordinal)
        {
            return this.Data.Rows[this.Row][ordinal];
        }

        public override int GetValues(object[] values)
        {
            object[] rowVals = this.Data.Rows[this.Row].ItemArray;

            int c = Math.Min(values.Length, rowVals.Length);

            int i;
            for (i = 0; i < c; i++)
                values[i] = rowVals[i];

            return i;
        }

        public override bool IsDBNull(int ordinal)
        {
            return Convert.IsDBNull(this.Data.Rows[this.Row][ordinal]);
        }

        public override void Close()
        {
            if (null != _connectionToClose)
                _connectionToClose.Close();

            _readerClosed = true;
        }

        public override DataTable GetSchemaTable()
        {
            return this.Data.Clone();
        }

        public override bool NextResult()
        {
            if ((_current + 1) > _data.Length - 1)
                return false;

            _current++;

            return true;
        }

        public override bool Read()
        {
            if (!this.HasRows)
                return false;

            this.Row++;

            return true;
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private void SetInternalState(DataTable[] data, int recordsAffected)
        {
            _data = new DataTable[data.Length];
            _row = new int[data.Length];
            _recordsAffected = recordsAffected;

            for (int i = 0; i < data.Length; ++i)
            {
                _data[i] = data[i];
                _row[i] = -1;
            }
        }

        #endregion
    }

    internal class DataReaderConverter : DbDataAdapter
    {
        public int FillFromReader(DataTable data, IDataReader dataReader)
        {
            return this.Fill(data, dataReader);
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return null;
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return null;
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
        }

        public static void FillDataSetFromReader(DataSet data, IDataReader dataReader)
        {
            DataReaderConverter converter = new DataReaderConverter();

            do
            {
                DataTable table = new DataTable();
                converter.FillFromReader(table, dataReader);
                data.Tables.Add(table);
            }
            while (dataReader.NextResult());
        }
    }
}
