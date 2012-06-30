using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;
using CsDO.Lib.Configuration;

namespace CsDO.Lib.MockDriver
{
    internal class ProviderFactory : DbProviderFactory
    {
        #region Factory

        public static readonly ProviderFactory Instance = new ProviderFactory( );

        #endregion
        
        #region Private Vars

        private DbProviderFactory _factory = DbProviderFactories.GetFactory(ConfigurationHelper.Instance.Driver);

        #endregion

        #region Constructors

        private ProviderFactory( ) { }

        #endregion

        #region Properties

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return _factory.CanCreateDataSourceEnumerator;
            }
        }

        #endregion
        
        #region Public Methods

        public override DbCommand CreateCommand( )
        {
            return _factory.CreateCommand( );
        }

        public override DbCommandBuilder CreateCommandBuilder( )
        {
            return _factory.CreateCommandBuilder( );
        }

        public override DbConnection CreateConnection( )
        {
            return _factory.CreateConnection( );
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder( )
        {
            return _factory.CreateConnectionStringBuilder( );
        }

        public override DbDataAdapter CreateDataAdapter( )
        {
            return _factory.CreateDataAdapter( );
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator( )
        {
            return _factory.CreateDataSourceEnumerator( );
        }

        public override DbParameter CreateParameter( )
        {
            return _factory.CreateParameter( );
        }

        public override CodeAccessPermission CreatePermission( PermissionState state )
        {
            return _factory.CreatePermission( state );
        }

        #endregion
    }
}