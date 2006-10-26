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
   	
		public enum DBMS {None, PostgreSQL, OleDB, MSSQLServer};

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
      
		public static string SGBD {
			get {
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
		      	
		      	for (int i = 0; i < connectionString.Length; i++) {
		      		if (connectionString[i].Trim().ToLower().StartsWith(property.ToLower()))
		      			return connectionString[i].Substring(connectionString[i].LastIndexOf('=') + 1);
		      	}

		      	return null;
		}
		
		public static string Server {
			get {
				return GetProperty("Server");
			}
		}		
		
		public static string Port {
			get {
				return GetProperty("Port");
			}
		}		

		public static string User {
			get {
				return GetProperty("User");
			}
		}		

		public static string Database {
			get {
				return GetProperty("Database");
			}
		}		

		public static string Password {
			get {
				return GetProperty("PassWord");
			}
		}
   }
}
