using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.ComponentModel;
using CsDO.Lib;
using CsDO.Lib.Configuration;

namespace CsDO.Lib.MockDriver
{
    public class MockCommand : DbCommand
    {
        #region Private Vars

        private DbCommand _dbCommand = ProviderFactory.Instance.CreateCommand();
        private DbTransaction _dbTransaction;

        #endregion

        #region Constructors

        public MockCommand() { }

        public MockCommand(string cmdText, MockConnection connection)
            : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
        }

        #endregion

        #region Private Vars

        [DefaultValue("")]
        [RefreshProperties(RefreshProperties.All)]
        public override string CommandText
        {
            get
            {
                return _dbCommand.CommandText;
            }
            set
            {
                _dbCommand.CommandText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                return _dbCommand.CommandTimeout;
            }
            set
            {
                _dbCommand.CommandTimeout = value;
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public override CommandType CommandType
        {
            get
            {
                return _dbCommand.CommandType;
            }
            set
            {
                _dbCommand.CommandType = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [DefaultValue(true)]
        [DesignOnly(true)]
        public override bool DesignTimeVisible
        {
            get
            {
                return _dbCommand.DesignTimeVisible;
            }
            set
            {
                _dbCommand.DesignTimeVisible = value;
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return _dbCommand.UpdatedRowSource;
            }
            set
            {
                _dbCommand.UpdatedRowSource = value;
            }
        }

        #endregion

        #region Public Methods

        public override void Cancel()
        {
            _dbCommand.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            int result;

            int hashedKey = ResultCache.GetHashedKey(ResultCache.CacheEntryType.ExecuteNonQuery.ToString(), _dbCommand);
            ResultCacheEntry entry = ResultCache.Instance[hashedKey];
            if (entry == null)
            {
                result = _dbCommand.ExecuteNonQuery();
                ResultCache.Instance.Add(hashedKey, result);
            }
            else
            {
                result = (int)entry.ScalarResult;
            }

            return result;
        }

        public override object ExecuteScalar()
        {
            object result;

            int hashedKey = ResultCache.GetHashedKey(ResultCache.CacheEntryType.ExecuteScalar.ToString(), _dbCommand);
            ResultCacheEntry entry = ResultCache.Instance[hashedKey];
            if (entry == null)
            {
                result = _dbCommand.ExecuteScalar();
                ResultCache.Instance.Add(hashedKey, result);
            }
            else
            {
                result = entry.ScalarResult;
            }

            return result;
        }

        public override void Prepare()
        {
            _dbCommand.Prepare();
        }

        #endregion

        #region Private Methods

        protected override DbConnection DbConnection
        {
            get
            {
                if (ConfigurationHelper.Instance.TestMode)
                    return new MockConnection();
                else
                    return _dbCommand.Connection;
            }
            set
            {
                MockConnection castValue = value as MockConnection;
                if (castValue == null)
                    _dbCommand.Connection = value;
                else
                    _dbCommand.Connection = castValue.RealConnection;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return _dbCommand.Parameters;
            }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                if (ConfigurationHelper.Instance.TestMode)
                    return _dbTransaction;
                else
                    return _dbCommand.Transaction;
            }
            set
            {
                if (ConfigurationHelper.Instance.TestMode)
                    _dbTransaction = value;
                else
                    _dbCommand.Transaction = value;
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            return _dbCommand.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            DbDataReader result = null;

            int hashedKey = ResultCache.GetHashedKey(ResultCache.CacheEntryType.ExecuteDbDataReader.ToString(), _dbCommand);
            ResultCacheEntry entry = ResultCache.Instance[hashedKey];
            if (entry == null)
            {
                result = _dbCommand.ExecuteReader(behavior);
                int recordsAffected = result.RecordsAffected;

                DataSet ds = new DataSet();
                DataReaderConverter.FillDataSetFromReader(ds, result);

                ResultCache.Instance.Add(hashedKey, recordsAffected, ds);

                result.Close();
                result.Dispose();
                result = null;

                result = new MockDataReader(ds, recordsAffected);
            }
            else
            {
                result = new MockDataReader(entry.DataSetResult, (int)entry.ScalarResult);
            }

            return result;
        }

        #endregion
    }
}
