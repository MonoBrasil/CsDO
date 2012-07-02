using System;
using System.Data;

namespace CsDO.Lib
{
	public class ObjectDataTable
    {
        private DataTable dt;

        private int row = 0;

        public int Cursor
        {
            get 
            {
                if (Read())
                    return row++;
                else
                    return row; 
            }
        }

        public bool IsEmpty
        {
            get { return dt.Rows.Count == 0; }
        }

        public DataRowCollection Rows 
        {
            get { return dt.Rows; }
        }

        public DataColumnCollection Columns 
        {
            get { return dt.Columns; }
        }

        public DataRow this[int index]
        {
            get { return dt.Rows[index]; }
        }

        public ObjectDataTable(DataTable table)
        {
            dt = table;
        }

        public bool Read()
        {
            bool result = row < dt.Rows.Count;

            return result;
        }

        public void ResetCursor()
        {
            row = 0;
        }

        public void Clear()
        {
            dt.Clear();
            ResetCursor();
        }

		public static implicit operator ObjectDataTable(DataTable dt)
		{
			return new ObjectDataTable(dt);
		}
    }
}

