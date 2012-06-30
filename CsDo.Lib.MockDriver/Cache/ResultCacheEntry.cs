using System;
using System.Data;

namespace CsDO.Lib.MockDriver
{
    [Serializable]
    public class ResultCacheEntry
    {
        #region Constructors

        public ResultCacheEntry( ) { }

        public ResultCacheEntry( int hashedKey, object scalarResult )
        {
            this.HashedKey = hashedKey;
            this.ScalarResult = scalarResult;
        }

        public ResultCacheEntry( int hashedKey, object scalarResult, DataSet dataSetResult )
        {
            this.HashedKey = hashedKey;
            this.ScalarResult = scalarResult;
            this.DataSetResult = dataSetResult;
        }

        #endregion

        #region Properties

        private int _hashedKey;

        public int HashedKey
        {
            get
            {
                return _hashedKey;
            }
            set
            {
                _hashedKey = value;
            }
        }

        private object _scalarResult;

        public object ScalarResult
        {
            get
            {
                return _scalarResult;
            }
            set
            {
                _scalarResult = value;
            }
        }

        private DataSet _dataSetResult;

        public DataSet DataSetResult
        {
            get
            {
                return _dataSetResult;
            }
            set
            {
                _dataSetResult = value;
            }
        }

        #endregion
    }
}