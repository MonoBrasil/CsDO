using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;
using CsDO.Lib.Configuration;
using System.Collections;

namespace CsDO.Lib.MockDriver
{
    internal class ProviderFactory : DbProviderFactory
    {      
        #region Private Vars

        private static readonly Hashtable instances = new Hashtable();
        private static object syncRoot = new object();
        private static volatile int numOfReferences = 0;
        private static bool innerCall = false;

        private DbProviderFactory _factory = DbProviderFactories.GetFactory(ConfigurationHelper.New().Driver);

        #endregion

        #region Constructors

		public ProviderFactory() { 
			if (!innerCall)
				throw new TypeInitializationException("This class is Singleton, use New() instead of the class constructor.", new Exception(""));
		}

        protected static ProviderFactory Instance(Type self)
        {
            ProviderFactory instance = null;

            innerCall = true;

            if (self.IsSubclassOf(typeof(ProviderFactory)) || self == typeof(ProviderFactory))
            {
                instance = (ProviderFactory)instances[self];
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        instance = (ProviderFactory)instances[self];
                        if (instance == null)
                        {
                            instance = (ProviderFactory)Activator.CreateInstance(self);
                            instances.Add(self, instance);
                        }
                    }
                }

                if (instance == null)
                    throw new TypeInitializationException("Default constructor not called.", new Exception(""));
            }

            innerCall = false;

            numOfReferences++;

            return instance;
        }

        public static ProviderFactory New() { return (ProviderFactory)Instance(typeof(ProviderFactory)); }

        #endregion

        #region Properties

        public override bool CanCreateDataSourceEnumerator
        {
            get { return _factory.CanCreateDataSourceEnumerator; }
        }

        public static int References
        {
            get { return numOfReferences; }
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