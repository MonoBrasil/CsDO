/*
 * Created by Alexandre Rocha Lima e Marcondes
 * User: Administrator
 * Date: 28/09/2005
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
using System.Data.SqlClient;

using CsDO.Lib;

namespace CsDO.Drivers.SqlServer
{

	public class SqlServerDriver : IDataBase
	{
		private SqlConnection conn = null;
		private string connectionString = null;	

		public SqlServerDriver() {
			this.connectionString = Config.GetDbConectionString(Config.DBMS.MSSQLServer);
		}
		
		public SqlServerDriver(string server, string user, string database, string password) 
		{
			this.connectionString =  "Server="+server+";User Id="+user+";Password="+password+";Database="+database+";";
		}

		protected string getUrl()
		{
			return connectionString;
		}

		protected string getUrlSys()
		{
			return getUrl();
		}

		public IDbCommand getCommand(String sql)
		{

			return new SqlCommand(sql, (SqlConnection) getConnection());
		}

		public IDbCommand getSystemCommand(String sql)
		{
			return new SqlCommand(sql, (SqlConnection) getSystemConnection());
		}

        public IDataAdapter getDataAdapter(IDbCommand command)
        {
            return new SqlDataAdapter((SqlCommand) command);
        }

		protected IDbConnection getConnection()
		{
			open(getUrl());

			return conn;
		}

		protected IDbConnection getSystemConnection()
		{
			open(getUrlSys());

			return conn;
		}
		
		protected void open(string URL) 
	  	{
			conn = new SqlConnection(URL);
				conn.Open();
		}

		protected void close()
		{
			if (conn != null)
				conn.Close();
			//GC();
		}
	}
}

