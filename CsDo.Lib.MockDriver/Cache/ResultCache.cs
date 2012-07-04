using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using CsDO.Lib;
using CsDO.Lib.Configuration;

namespace CsDO.Lib.MockDriver
{
    internal class ResultCache
    {
        #region Factory

        public static readonly ResultCache Instance = new ResultCache( );
        
        #endregion

        #region Enums

        public enum CacheEntryType
        {
            ExecuteDbDataReader = 0,
            ExecuteScalar = 1,
            ExecuteNonQuery = 2
        }

        #endregion

        #region Constants

        private const string CacheKeySeparator = "|";

        #endregion
        
        #region Private vars

        private Dictionary<int, ResultCacheEntry> _cache = new Dictionary<int, ResultCacheEntry>( );

        private object syncRoot = new object( );
        
        #endregion

        #region Properties

        public ResultCacheEntry this[ int hashedKey ]
        {
            get
            {
                return Get( hashedKey );
            }
        }

        #endregion

        #region Constructors

        private ResultCache( )
        {
            Load( );
        }

        #endregion
        
        #region Public Methods

        public ResultCacheEntry Get( int hashedKey )
        {
            if ( !ConfigurationHelper.Instance.TestMode )
                return null;

            if ( _cache.ContainsKey( hashedKey ) )
                return _cache[ hashedKey ];
			else
				return new ResultCacheEntry(hashedKey, 1, ((MockDriver)Conf.Driver).ds);
        }

        public void Add( int hashedKey, object scalarResult )
        {
            if ( ConfigurationHelper.Instance.TestMode )
                return;

            if ( !_cache.ContainsKey( hashedKey ) )
                _cache.Add( hashedKey, new ResultCacheEntry( hashedKey, scalarResult ) );
            else
                _cache[ hashedKey ] = new ResultCacheEntry( hashedKey, scalarResult );

            Save( );
        }

        public void Add( int hashedKey, object scalarResult, DataSet dataSetResult )
        {
            if ( ConfigurationHelper.Instance.TestMode )
                return;
            if ( !_cache.ContainsKey( hashedKey ) )
                _cache.Add( hashedKey, new ResultCacheEntry( hashedKey, scalarResult, dataSetResult ) );
            else
                _cache[ hashedKey ] = new ResultCacheEntry( hashedKey, scalarResult, dataSetResult );

            Save( );
        }

        public static int GetHashedKey( string cacheEntryType, DbCommand command )
        {
            StringBuilder sb = new StringBuilder( );

            GetParameterDataString( sb, command );

            return ( String.Format( CultureInfo.InvariantCulture, "{0}{1}", cacheEntryType, sb.ToString( ) ) ).GetHashCode( );
        }

        #endregion

        #region Private Methods

        private void Load( )
        {
            lock ( syncRoot )
            {
                TextReader reader = null;
                try
                {
                    if ( File.Exists( ConfigurationHelper.Instance.CacheFile ) )
                    {
                        reader = new StreamReader( ConfigurationHelper.Instance.CacheFile );
                        XmlSerializer serializer = new XmlSerializer( typeof( ResultCacheEntry[ ] ) );
                        ResultCacheEntry[ ] cacheEntrys = ( ResultCacheEntry[ ] )serializer.Deserialize( reader );
                        reader.Close( );

                        _cache.Clear( );

                        foreach ( ResultCacheEntry entry in cacheEntrys )
                            _cache.Add( entry.HashedKey, entry );
                    }
                }
                catch
                {
                    if ( reader != null )
                        reader.Close( );

                    if ( File.Exists( ConfigurationHelper.Instance.CacheFile ) )
                        File.Delete( ConfigurationHelper.Instance.CacheFile );
                }
            }
        }
        
        private void Save( )
        {
            ResultCacheEntry[ ] cacheEntrys = new ResultCacheEntry[ _cache.Count ];

            int i = 0;
            foreach ( KeyValuePair<int, ResultCacheEntry> kvp in _cache )
            {
                cacheEntrys[ i ] = kvp.Value;
                i++;
            }

            lock ( syncRoot )
            {
                TextWriter writer = new StreamWriter( ConfigurationHelper.Instance.CacheFile, false );
                XmlSerializer serializer = new XmlSerializer( typeof( ResultCacheEntry[ ] ) );
                serializer.Serialize( writer, cacheEntrys );
                writer.Close( );
            }
        }

        private static void GetParameterDataString( StringBuilder sb, DbCommand parameter )
        {
            if ( parameter != null && !String.IsNullOrEmpty( parameter.CommandText ) )
            {
                sb.Append( CacheKeySeparator );
                sb.Append( parameter.CommandText );
                if ( parameter.Parameters != null && parameter.Parameters.Count != 0 )
                {
                    foreach ( DbParameter param in parameter.Parameters )
                    {
                        if ( param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput )
                        {
                            if ( !String.IsNullOrEmpty( param.ParameterName ) )
                            {
                                sb.Append( CacheKeySeparator );
                                sb.Append( param.ParameterName );
                            }

                            try
                            {
                                if ( param.Value != null )
                                {
                                    sb.Append( CacheKeySeparator );
                                    sb.Append( param.Value.ToString( ) );
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        #endregion
    }
}