using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace CsDO.Lib.MockDriver
{
    public class MockParameter : DbParameter
    {
        public override DbType DbType
        {
            get
            {
                throw new Exception("2 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("3 The method or operation is not implemented.");
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                throw new Exception("4 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("5 The method or operation is not implemented.");
            }
        }

        public override bool IsNullable
        {
            get
            {
                throw new Exception("6 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("7 The method or operation is not implemented.");
            }
        }

        public override string ParameterName
        {
            get
            {
                throw new Exception("8 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("9 The method or operation is not implemented.");
            }
        }

        public override void ResetDbType()
        {
            throw new Exception("11 The method or operation is not implemented.");
        }

        public override int Size
        {
            get
            {
                throw new Exception("12 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("14 The method or operation is not implemented.");
            }
        }

        public override string SourceColumn
        {
            get
            {
                throw new Exception("15 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("16 The method or operation is not implemented.");
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                throw new Exception("17 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("18 The method or operation is not implemented.");
            }
        }

        public override DataRowVersion SourceVersion
        {
            get
            {
                throw new Exception("19 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("20 The method or operation is not implemented.");
            }
        }

        public override object Value
        {
            get
            {
                throw new Exception("21 The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("22 The method or operation is not implemented.");
            }
        }
    }

    public class MockDbParameterCollection : DbParameterCollection
    {
        List<object> values = new List<object>();
        List<string> keys = new List<string>();

        public override int Add(object value)
        {
            values.Add(value);
            keys.Add(value.ToString());
            return values.Count - 1;
        }

        public override void AddRange(Array values)
        {
            this.values.AddRange(values as IEnumerable<object>);
        }

        public override void Clear()
        {
            values.Clear();
            keys.Clear();
        }

        public override bool Contains(string value)
        {
            return keys.Contains(value);
        }

        public override bool Contains(object value)
        {
            return values.Contains(value);
        }

        public override void CopyTo(Array array, int index)
        {
            for (int i = index; i < array.Length; i++)
                Add(array.GetValue(i));
        }

        public override int Count
        {
            get { return keys.Count; }
        }

        public override IEnumerator GetEnumerator()
        {
            throw new Exception("23 The method or operation is not implemented.");
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            throw new Exception("24 The method or operation is not implemented.");
        }

        protected override DbParameter GetParameter(int index)
        {
            throw new Exception("25 The method or operation is not implemented.");
        }

        public override int IndexOf(string parameterName)
        {
            return keys.IndexOf(parameterName);
        }

        public override int IndexOf(object value)
        {
            return values.IndexOf(value);
        }

        public override void Insert(int index, object value)
        {
            throw new Exception("26 The method or operation is not implemented.");
        }

        public override bool IsFixedSize
        {
            get { return false; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsSynchronized
        {
            get { return false; }
        }

        public override void Remove(object value)
        {
            int index = values.IndexOf(value);
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            int index = keys.IndexOf(parameterName);
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        public override void RemoveAt(int index)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            throw new Exception("27 The method or operation is not implemented.");
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            throw new Exception("28 The method or operation is not implemented.");
        }

        public override object SyncRoot
        {
            get { throw new Exception("29 The method or operation is not implemented."); }
        }
    }
}
