using System;

namespace CsDO.Lib.Configuration
{
    internal sealed class ConfigurationInformation
    {
        #region Constructors

        public ConfigurationInformation( string driver, string cacheFile, bool testMode, bool debugMode)
        {
            _driver = driver;
            _cacheFile = cacheFile;
            _testMode = testMode;
            _debugMode = debugMode;
        }

        #endregion

        #region Properties

        private string _driver;

        public string Driver
        {
            get
            {
                return _driver;
            }
        }

        private string _cacheFile;

        public string CacheFile
        {
            get
            {
                return _cacheFile;
            }
        }

        private bool _testMode;

        public bool TestMode
        {
            get
            {
                return _testMode;
            }
        }

        private bool _debugMode;

        public bool DebugMode
        {
            get
            {
                return _debugMode;
            }
        }
        #endregion
    }
}