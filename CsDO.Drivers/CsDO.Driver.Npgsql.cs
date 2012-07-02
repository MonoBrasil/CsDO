/*
 * Created by Alexandre Rocha Lima e Marcondes
 * User: Administrator
 * Date: 12/08/2005
 * Time: 16:13
 * 
 * Description: An SQL Builder, Object Interface to Database Tables
 * Its based on DataObjects from php PEAR
 *  1. Builds SQL statements based on the objects vars and the builder methods.
 *  2. acts as a datastore for a table row.
 *  The core class is designed to be extended for each of your tables so that you put the
 *  data logic inside the data classes.
 *  included is a Generator to make your configuration files and your base classes.
 * 
 * CSharp DataObject 
 * Copyright (c) 2005, Alessandro de Oliveira Binhara
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are 
 * permitted provided that the following conditions are met:
 * 
 * - Redistributions of source code must retain the above copyright notice, this list 
 * of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above copyright notice, this list
 * of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 *
 * - Neither the name of the <ORGANIZATION> nor the names of its contributors may be used to 
 * endorse or promote products derived from this software without specific prior written 
 * permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS 
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
 * AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Data;
using Npgsql;

using CsDO.Lib;
using System.Data.Common;

namespace CsDO.Drivers.Npgsql
{
	[Obsolete("Use CsDO configuration files instead")]
	public class NpgsqlDriver : IDataBase
	{
		private NpgsqlConnection conn = null;
		private string connectionString = null;

        public DbConnection Connection { get { return conn; } }

		public NpgsqlDriver() {
			this.connectionString = Config.GetDbConectionString(Config.DBMS.PostgreSQL);
		}
		
		public NpgsqlDriver(string server, string port, string user, string database, string password) 
		{
				connectionString = "Server="+server+";Port="+port+";User Id="+user+";Password="+password+";Database="+database+";";
		}
			
		protected string getUrl()
		{
			return this.connectionString;
		}
		
		protected string getUrlSys()
		{
			return getUrl();
		}

		public DbCommand getCommand(String sql) 
		{	
			return new NpgsqlCommand(sql, (NpgsqlConnection) getConnection());
		}

		public DbCommand getSystemCommand(String sql) 
		{	
			return new NpgsqlCommand(sql, (NpgsqlConnection) getSystemConnection());
		}

        public DataTable getSchema()
        {
            return conn.GetSchema();
        }

        public DataTable getSchema(string collectionName)
        {
            return conn.GetSchema(collectionName);
        }

        public DataTable getSchema(string collectionName, string[] restrictions)
        {
            return conn.GetSchema(collectionName, restrictions);
        }

		public DbConnection getConnection() 
		{	
			if (conn == null)
    			open(getUrl());
			
		    	return conn;
		}

        public DbConnection getSystemConnection() 
		{	
			if (conn == null)
    			open(getUrlSys());

			return conn;
		}

        public DbDataAdapter getDataAdapter(DbCommand command)
        {
            return new NpgsqlDataAdapter((NpgsqlCommand)command);
        }

		public DbParameter getParameter ()
		{
			throw new System.NotImplementedException ();
		}

		public DbParameter getParameter (string name, DbType type, int size)
		{
			throw new System.NotImplementedException ();
		}

		public void open(string URL) 
	  	{
			conn = new NpgsqlConnection(URL);
	  		conn.Open();
		}

        public void close()
		{
			if (conn != null)
				conn.Close();
		}
	}
}

