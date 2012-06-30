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
using System.Configuration;
using System.Reflection;
using CsDO.Lib;
using System.Text;
using System.Data.Common;

namespace CsDO.Drivers
{
	/// <summary>
	/// Summary description for CsDO.
	/// </summary>
	public class Generic : IDataBase, IDisposable
	{
        DbConnection conn = null;

        public DbConnection Connection { get { return getConnection(); } }

		public Generic() {}

		public DbCommand getCommand(String sql) {
			DbCommand command = objectCommand();
            if (command != null)
            {
                command.CommandText = sql;
                conn = getConnection();
                try
                {
                    command.Connection = conn;
                    if (command.Connection != null &&
                        (command.Connection.State == ConnectionState.Broken
                        || command.Connection.State == ConnectionState.Closed))
                    {
                        command.Connection.Open();
                    }
                }
                catch (DataException)
                {
                    conn.Dispose();
                }
            }
            else
                command.Connection = getConnection();
			return command;
		}

        public DbDataAdapter getDataAdapter(DbCommand command)
        {
            DbDataAdapter da = objectDataAdapter(command);
            return da;
        }

		public DbCommand getSystemCommand(String sql) {
			return this.getCommand(sql);
		}

        public DbConnection getConnection()
        {
            if (conn == null)
            {
                if (ConfigurationManager.AppSettings["connection"] != null)
                {
                    return getConnection(ConfigurationManager.AppSettings["connection"].ToString());
                }
            }

            return conn;
        }

		protected DbConnection getConnection(string URL) {
            if (conn != null)
                return conn;

			Assembly assembly;
			string prefix;
            if (ConfigurationManager.AppSettings["dbNameSpace"] != null)
            {
                prefix = ConfigurationManager.AppSettings["dbNameSpace"].ToString();
			}
			else {
				prefix = null;
			}

            assembly = Assembly.Load(ConfigurationManager.AppSettings["dbAssembly"].ToString());

			Type[] tipos = assembly.GetTypes();

			foreach (Type tipo in tipos) {
				Type getInterface = tipo.GetInterface("IDbConnection");

				if (getInterface != null) {
					Assembly assCon = Assembly.Load(tipo.Assembly.GetName().FullName);

					if (prefix != null && prefix != string.Empty) {
						string[] namespaces = tipo.Namespace.Split('.');

						if (namespaces[namespaces.Length - 1] == prefix) {
                            StringBuilder obj = new StringBuilder(tipo.Namespace);
                            obj.Append(".");
                            obj.Append(tipo.Name);
                            conn = (DbConnection)assCon.CreateInstance(obj.ToString());
							break;
						}
					}
					else {
                        StringBuilder obj = new StringBuilder(tipo.Namespace);
                        obj.Append(".");
                        obj.Append(tipo.Name);
                        conn = (DbConnection)assCon.CreateInstance(obj.ToString());
						break;
					}
				}
			}
            if (URL != null)
            {
                conn.ConnectionString = URL;
			}

            return conn;

		}

		protected DbCommand objectCommand() {
			Assembly assembly;
			string prefix;
            if (ConfigurationManager.AppSettings["dbNameSpace"] != null)
            {
                prefix = ConfigurationManager.AppSettings["dbNameSpace"].ToString();
			}
			else {
				prefix = null;
			}

            assembly = Assembly.Load(ConfigurationManager.AppSettings["dbAssembly"].ToString());

			Type[] tipos = assembly.GetTypes();
			DbCommand comando = null;
			foreach (Type tipo in tipos) {
				Type getInterface = tipo.GetInterface("IDbCommand");
				if (getInterface != null) {
					Assembly assCon = Assembly.Load(tipo.Assembly.GetName());

					if (prefix != null && prefix != string.Empty) {
						string[] namespaces = tipo.Namespace.Split('.');

						if (namespaces[namespaces.Length - 1] == prefix) {
                            StringBuilder obj = new StringBuilder(tipo.Namespace);
                            obj.Append(".");
                            obj.Append(tipo.Name);
							return comando = (DbCommand) assCon.CreateInstance(obj.ToString());
						}
					}

					else {
                        StringBuilder obj = new StringBuilder(tipo.Namespace);
                        obj.Append(".");
                        obj.Append(tipo.Name);
						return comando = (DbCommand) assCon.CreateInstance(obj.ToString());
					}
				}
			}
			return comando;
		}

		protected DbDataAdapter objectDataAdapter() {
			Assembly assembly;
			string prefix;
            if (ConfigurationManager.AppSettings["dbNameSpace"] != null)
            {
                prefix = ConfigurationManager.AppSettings["dbNameSpace"].ToString();
			}
			else {
				prefix = null;
			}

            assembly = Assembly.Load(ConfigurationManager.AppSettings["dbAssembly"].ToString());

			Type[] tipos = assembly.GetTypes();
			DbDataAdapter dataAdapter = null;
			foreach (Type tipo in tipos) {
				Type getInterface = tipo.GetInterface("IDbDataAdapter");
				if (getInterface != null) {
					Assembly assCon = Assembly.Load(tipo.Assembly.GetName());
					if (prefix != null && prefix != string.Empty) {
						string[] namespaces = tipo.Namespace.Split('.');

						if (namespaces[namespaces.Length - 1] == prefix) {
                            StringBuilder obj = new StringBuilder(tipo.Namespace);
                            obj.Append(".");
                            obj.Append(tipo.Name);
							return dataAdapter = (DbDataAdapter) assCon.CreateInstance(obj.ToString());
						}
					}
					else {
                        StringBuilder obj = new StringBuilder(tipo.Namespace);
                        obj.Append(".");
                        obj.Append(tipo.Name);
						return dataAdapter = (DbDataAdapter) assCon.CreateInstance(obj.ToString());
					}
				}
			}
			return dataAdapter;
		}

        protected DbDataAdapter objectDataAdapter(DbCommand command)
        {
            Assembly assembly;
            string prefix;
            if (ConfigurationManager.AppSettings["dbNameSpace"] != null)
            {
                prefix = ConfigurationManager.AppSettings["dbNameSpace"].ToString();
            }
            else
            {
                prefix = null;
            }

            assembly = Assembly.Load(ConfigurationManager.AppSettings["dbAssembly"].ToString());

            Type[] tipos = assembly.GetTypes();
            DbDataAdapter dataAdapter = null;
            foreach (Type tipo in tipos)
            {
                Type getInterface = tipo.GetInterface("IDbDataAdapter");
                if (getInterface != null)
                {
                    Assembly assCon = Assembly.Load(tipo.Assembly.GetName());
                    if (prefix != null && prefix != string.Empty)
                    {
                        string[] namespaces = tipo.Namespace.Split('.');

                        if (namespaces[namespaces.Length - 1] == prefix)
                        {
                            StringBuilder obj = new StringBuilder(tipo.Namespace);
                            obj.Append(".");
                            obj.Append(tipo.Name);
                            return dataAdapter = (DbDataAdapter)assCon.
                                CreateInstance(obj.ToString(), false, BindingFlags.InvokeMethod,
                                    null, new object[] { command }, null, null);
                        }
                    }
                    else
                    {
                        StringBuilder obj = new StringBuilder(tipo.Namespace);
                        obj.Append(".");
                        obj.Append(tipo.Name);
                        return dataAdapter = (DbDataAdapter)assCon.
                            CreateInstance(obj.ToString(), false, BindingFlags.InvokeMethod,
                                null, new object[] { command }, null, null);
                    }
                }
            }
            return dataAdapter;
        }

        protected DbParameter objectParameter()
        {
            Assembly assembly;
            string prefix;
            if (ConfigurationManager.AppSettings["dbNameSpace"] != null)
            {
                prefix = ConfigurationManager.AppSettings["dbNameSpace"].ToString();
            }
            else
            {
                prefix = null;
            }

            assembly = Assembly.Load(ConfigurationManager.AppSettings["dbAssembly"].ToString());

            Type[] tipos = assembly.GetTypes();
            DbParameter paramter = null;
            foreach (Type tipo in tipos)
            {
                Type getInterface = tipo.GetInterface("IDbParameter");
                if (getInterface != null)
                {
                    Assembly assCon = Assembly.Load(tipo.Assembly.GetName());
                    if (prefix != null && prefix != string.Empty)
                    {
                        string[] namespaces = tipo.Namespace.Split('.');

                        if (namespaces[namespaces.Length - 1] == prefix)
                        {
                            StringBuilder obj = new StringBuilder(tipo.Namespace);
                            obj.Append(".");
                            obj.Append(tipo.Name);
                            return paramter = (DbParameter)assCon.
                                CreateInstance(obj.ToString(), false, BindingFlags.InvokeMethod,
                                    null, new object[] {}, null, null);
                        }
                    }
                    else
                    {
                        StringBuilder obj = new StringBuilder(tipo.Namespace);
                        obj.Append(".");
                        obj.Append(tipo.Name);
                        return paramter = (DbParameter)assCon.
                            CreateInstance(obj.ToString(), false, BindingFlags.InvokeMethod,
                                null, new object[] { }, null, null);
                    }
                }
            }
            return paramter;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (conn != null)
                conn.Dispose();
        }

        #endregion

        #region IDataBase Members

        public DataTable getSchema()
        {
            try
            {
                return (DataTable) conn.GetType().InvokeMember("GetSchema", BindingFlags.InvokeMethod, null, conn, new object[] {});
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        public DataTable getSchema(string collectionName)
        {
            try
            {
                return (DataTable)conn.GetType().InvokeMember("GetSchema", BindingFlags.InvokeMethod, null, conn, new object[] { collectionName });
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        public DataTable getSchema(string collectionName, string[] restrictions)
        {
            try
            {
                return (DataTable)conn.GetType().InvokeMember("GetSchema", BindingFlags.InvokeMethod, null, conn, new object[] { collectionName, restrictions });
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        public void open(string URL)
        {
            conn = getConnection(URL);
            conn.Open();
        }

        public void close()
        {
            if (conn != null)
                conn.Close();
        }

        public DbParameter getParameter()
        {
            return objectParameter();
        }

        public DbParameter getParameter(string name, DbType type, int size)
        {
            DbParameter parameter = objectParameter();

            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Size = size;

            return parameter;
        }

        #endregion
    }
}
