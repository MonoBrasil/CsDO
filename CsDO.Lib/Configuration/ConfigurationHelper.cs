using System;
using System.Configuration;

namespace CsDO.Lib.Configuration
{
    public class ConfigurationHelper
    {
        #region Factory

        public static readonly ConfigurationHelper Instance = new ConfigurationHelper();

        #endregion

        #region Private Vars

        private readonly ConfigurationInformation _config;

        #endregion

        #region Constructors

        private ConfigurationHelper( )
        {
            try
            {
                _config = ConfigurationManager.GetSection("CsDO") as ConfigurationInformation;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Properties

        public string Driver
        {
            get
            {
                return _config == null ? null : _config.Driver;
            }
        }

        public string CacheFile
        {
            get
            {
                return _config.CacheFile;
            }
        }

        public bool TestMode
        {
            get
            {
                return _config.TestMode;
            }
        }

        public bool DebugMode
        {
            get
            {
                return _config.DebugMode;
            }
        }
        #endregion
    }
}