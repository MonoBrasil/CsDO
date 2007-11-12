using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.ComponentModel;
using CsDO.Lib.Configuration;

namespace CsDO.Lib.MockDriver
{
    public class MockConnection : DbConnection
    {
        #region Private Vars

        private DbConnection _dbConnection;
        private ConnectionState _localResultState = ConnectionState.Closed;

        #endregion

        #region Constructors
        
        public MockConnection()
        {
            _dbConnection = ProviderFactory.New().CreateConnection();
            _dbConnection.StateChange += new StateChangeEventHandler( _dbConnection_StateChange );
        }
        
        public MockConnection(string connectionString) : this()
        {
            this.ConnectionString = connectionString;
        }
        #endregion

        #region Event Handlers
        
        private void _dbConnection_StateChange( object sender, StateChangeEventArgs e )
        {
            base.OnStateChange( e );
        }

        #endregion

        #region Properties

        internal DbConnection RealConnection
        {
            get
            {
                return _dbConnection;
            }
        }
        
        [SettingsBindable( true )]
        [RefreshProperties( RefreshProperties.All )]
        [DefaultValue( "" )]
        public override string ConnectionString
        {
            get
            {
                return _dbConnection.ConnectionString;
            }
            set
            {
                _dbConnection.ConnectionString = value;
            }
        }

        public override string Database
        {
            get
            {
                return _dbConnection.Database;
            }
        }

        public override string DataSource
        {
            get
            {
                return _dbConnection.DataSource;
            }
        }

        [Browsable( false )]
        public override string ServerVersion
        {
            get
            {
                return _dbConnection.ServerVersion;
            }
        }

        [Browsable( false )]
        public override ConnectionState State
        {
            get
            {
                if (ConfigurationHelper.New().TestMode)
                    return _localResultState;
                else
                    return _dbConnection.State;
            }
        }

        #endregion

        #region Public Methods
        
        public override void ChangeDatabase( string databaseName )
        {
            _dbConnection.ChangeDatabase( databaseName );
        }

        public override void Close( )
        {
            if (ConfigurationHelper.New().TestMode)
                _localResultState = ConnectionState.Closed;
            else
                _dbConnection.Close( );
        }

        public override void Open( )
        {
            if (ConfigurationHelper.New().TestMode)
                _localResultState = ConnectionState.Open;
            else
                _dbConnection.Open( );
        }

        #endregion
        
        #region Private Methods

        protected override DbTransaction BeginDbTransaction( IsolationLevel isolationLevel )
        {
            if (ConfigurationHelper.New().TestMode)
                return new MockTransaction( this, isolationLevel );
            else
                return _dbConnection.BeginTransaction( isolationLevel );
        }
    
        protected override DbCommand CreateDbCommand( )
        {
            DbCommand command = MockFactory.Instance.CreateCommand( );
            command.Connection = _dbConnection;

            return command;
        }

	    #endregion    
    }
}
