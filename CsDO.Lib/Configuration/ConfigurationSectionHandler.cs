using System;
using System.Configuration;
using System.Xml;
using System.Globalization;

namespace CsDO.Lib.Configuration
{
    public sealed class ConfigurationSectionHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, XmlNode section)
        {
            if (section == null)
                throw new ConfigurationErrorsException("CsDO configuration section not found");

            #region General Configuration

            string driver = String.Empty;
            if (section.Attributes["driver"] != null && !String.IsNullOrEmpty(
                section.Attributes["driver"].Value))
                driver = section.Attributes["driver"].Value;

            string cacheFile = String.Empty;
            if (section.Attributes["cacheFile"] != null && !String.IsNullOrEmpty(
                section.Attributes["cacheFile"].Value))
                cacheFile = section.Attributes["cacheFile"].Value;

            bool testMode = true;
            if (section.Attributes["testMode"] != null)
                testMode = Convert.ToBoolean(section.Attributes["testMode"].Value,
                    CultureInfo.InvariantCulture);

            bool debugMode = true;
            if (section.Attributes["debugMode"] != null)
                debugMode = Convert.ToBoolean(section.Attributes["debugMode"].Value,
                    CultureInfo.InvariantCulture);

            #endregion

            return new ConfigurationInformation(driver, cacheFile, testMode, debugMode);
        }

        #endregion
    }
}