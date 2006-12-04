using System;
using System.Configuration;

namespace CsDO.Lib
{
    /// <summary>
    /// Summary description for CsDO.
    /// "Server=localhost;port=5432;User Id=postgres;Password=senhafacil;
    /// Database=geleira;Pooling=false;Encoding=LATIN1"
    /// </summary>
    public class Config
    {
        protected static DBMS driverConnection = DBMS.None;

        public enum DBMS { None, PostgreSQL, OleDB, MSSQLServer };

        /// <summary>
        /// Get connection string of App.config 
        /// </summary>
        /// <returns>database connection string</returns>
        public static string GetDbConectionString(DBMS driver)
        {
            driverConnection = driver;

            switch (driver)
            {
                case DBMS.PostgreSQL:
                    return ConfigurationManager.AppSettings["PostgreSQL"];
                case DBMS.OleDB:
                    return ConfigurationManager.AppSettings["OleDB"];
                case DBMS.MSSQLServer:
                    return ConfigurationManager.AppSettings["MSSQLServer"];
                default:
                    return null;
            }
        }

        public static string CreateDbConectionString(DBMS driver)
        {
            driverConnection = driver;

            return "Server = " + server
                + "; port = " + port
                + "; User Id = " + user
                + "; Password = " + password
                + "; Database = " + database + ";";
        }

        public static string SGBD
        {
            get
            {
                switch (driverConnection)
                {
                    case DBMS.PostgreSQL:
                        return "PostgreSQL";
                    case DBMS.OleDB:
                        return "OleDB";
                    case DBMS.MSSQLServer:
                        return "MSSQLServer";
                    default:
                        return null;
                }
            }
        }

        protected static string GetProperty(string property)
        {
            string[] connectionString;

            if (driverConnection == DBMS.None)
                return null;

            if (GetDbConectionString(driverConnection) != null)
                connectionString = GetDbConectionString(driverConnection).Split(';');
            else
                return null;

            for (int i = 0; i < connectionString.Length; i++)
            {
                if (connectionString[i].Trim().ToLower().StartsWith(property.ToLower()))
                    return connectionString[i].Substring(connectionString[i].LastIndexOf('=') + 1);
            }

            return null;
        }

        private static string server = null;
        public static string Server
        {
            get
            {
                if (server == null || server.Trim().Equals(String.Empty))
                    return GetProperty("Server");
                else
                    return server;
            }
            set { server = value; }
        }

        private static string port = null;
        public static string Port
        {
            get
            {
                if (port == null || port.Trim().Equals(String.Empty))
                    return GetProperty("Port");
                else
                    return port;
            }
            set { port = value; }
        }

        private static string user = null;
        public static string User
        {
            get
            {
                if (user == null || user.Trim().Equals(String.Empty))
                    return GetProperty("User");
                else
                    return user;
            }
            set { user = value; }
        }

        private static string database = null;
        public static string Database
        {
            get
            {
                if (database == null || database.Trim().Equals(String.Empty))
                    return GetProperty("Database");
                else
                    return database;
            }
            set { database = value; }
        }

        private static string password = null;
        public static string Password
        {
            get
            {
                if (password == null || password.Trim().Equals(String.Empty))
                    return GetProperty("Password");
                else
                    return password;
            }
            set { password = value; }
        }
    }
}
