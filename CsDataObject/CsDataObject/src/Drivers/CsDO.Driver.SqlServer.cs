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
using System.Data.Common;

namespace CsDO.Drivers.SqlServer
{

	public class SqlServerDriver : IDataBase
	{
		private SqlConnection conn = null;
		private string connectionString = null;

        public DbConnection Connection { get { return conn; } }

		public SqlServerDriver() {
			this.connectionString = Config.GetDbConectionString(Config.DBMS.MSSQLServer);
		}
		
		public SqlServerDriver(string server, string user, string database, string password) 
		{
            this.connectionString = "Server=" + server + ";User Id=" + user + ";Password=" + password + ";Database=" + database + ";MultipleActiveResultSets=true;Max Pool Size=10;Pooling=true";
		}

		protected string getUrl()
		{
			return connectionString;
		}

		protected string getUrlSys()
		{
			return getUrl();
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

		public DbCommand getCommand(String sql)
		{

			return new SqlCommand(sql, (SqlConnection) getConnection());
		}

		public DbCommand getSystemCommand(String sql)
		{
			return new SqlCommand(sql, (SqlConnection) getSystemConnection());
		}

        public DbDataAdapter getDataAdapter(DbCommand command)
        {
            return new SqlDataAdapter((SqlCommand) command);
        }

		public DbConnection getConnection()
		{
			open(getUrl());

			return conn;
		}

		public DbConnection getSystemConnection()
		{
			open(getUrlSys());

			return conn;
		}
		
		public void open(string URL) 
	  	{
            try
            {

                if (conn == null)
                {
                    conn = new SqlConnection(URL);
                    conn.Open();
                }
                else
                {
                    if (conn.State == ConnectionState.Broken
                        || conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                }
            }
            catch (DataException)
            {
                conn.Dispose();
            }
		}

		public void close()
		{
			if (conn != null)
				conn.Close();
		}

        #region IDataBase Members
        public DbParameter getParameter()
        {
            return new SqlParameter();
        }

        public DbParameter getParameter(string name, DbType type, int size)
        {
            SqlDbType dbType = SqlDbType.Variant;

            switch (type)
            {
                case DbType.AnsiString:
                    dbType = SqlDbType.VarChar;
                    break;
                case DbType.AnsiStringFixedLength:
                    dbType = SqlDbType.Char;
                    break;
                case DbType.Binary:
                    dbType = SqlDbType.Binary;
                    break;
                case DbType.Boolean:
                    dbType = SqlDbType.Bit;
                    break;
                case DbType.Byte:
                    dbType = SqlDbType.TinyInt;
                    break;
                case DbType.Currency:
                    dbType = SqlDbType.Money;
                    break;
                case DbType.Date:
                    dbType = SqlDbType.DateTime;
                    break;
                case DbType.DateTime:
                    dbType = SqlDbType.DateTime;
                    break;
                case DbType.Decimal:
                    dbType = SqlDbType.Decimal;
                    break;
                case DbType.Double:
                    dbType = SqlDbType.Float;
                    break;
                case DbType.Guid:
                    dbType = SqlDbType.UniqueIdentifier;
                    break;
                case DbType.Int16:
                    dbType = SqlDbType.SmallInt;
                    break;
                case DbType.Int32:
                    dbType = SqlDbType.Int;
                    break;
                case DbType.Int64:
                    dbType = SqlDbType.BigInt;
                    break;
                case DbType.Object:
                    dbType = SqlDbType.VarBinary;
                    break;
                case DbType.SByte:
                    dbType = SqlDbType.SmallInt;
                    break;
                case DbType.Single:
                    dbType = SqlDbType.Real;
                    break;
                case DbType.String:
                    dbType = SqlDbType.VarChar;
                    break;
                case DbType.StringFixedLength:
                    break;
                case DbType.Time:
                    dbType = SqlDbType.DateTime;
                    break;
                case DbType.UInt16:
                    dbType = SqlDbType.Decimal;
                    break;
                case DbType.UInt32:
                    dbType = SqlDbType.Decimal;
                    break;
                case DbType.UInt64:
                    dbType = SqlDbType.Decimal;
                    break;
                case DbType.VarNumeric:
                    dbType = SqlDbType.Decimal;
                    break;
                case DbType.Xml:
                    dbType = SqlDbType.Xml;
                    break;
                default:
                    break;
            }

            return new SqlParameter(name, dbType, size);
        }
        #endregion

	}
}

