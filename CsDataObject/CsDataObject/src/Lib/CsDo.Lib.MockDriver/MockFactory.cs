using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Security.Permissions;
using System.Security;

namespace CsDO.Lib.MockDriver
{
    public class MockFactory : DbProviderFactory
    {
        #region Factory

        public static readonly MockFactory Instance = new MockFactory();

        #endregion

        #region Constructors

        public MockFactory() { }

        #endregion

        #region Factory

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Methods

        public override DbCommand CreateCommand()
        {
            return new MockCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotImplementedException();
        }

        public override DbConnection CreateConnection()
        {
            return new MockConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            throw new NotImplementedException();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new MockDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            throw new NotImplementedException();
        }

        public override DbParameter CreateParameter()
        {
            return new MockParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
