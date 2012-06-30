using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace CsDO.Lib.MockDriver
{
    public class MockTransaction : DbTransaction
    {
        #region Private Vars

        private DbConnection _dbConnection;
        private IsolationLevel _isolationLevel;

        #endregion

        #region Constructors

        internal MockTransaction(DbConnection dbConnection, IsolationLevel isolationLevel)
        {
            _dbConnection = dbConnection;
            _isolationLevel = isolationLevel;
        }

        #endregion

        #region Properties

        public override IsolationLevel IsolationLevel
        {
            get
            {
                return _isolationLevel;
            }
        }

        #endregion

        #region Public Methods

        public override void Commit() { }

        public override void Rollback() { }

        #endregion

        #region Private Methods

        protected override DbConnection DbConnection
        {
            get
            {
                return _dbConnection;
            }
        }

        #endregion
    }
}
