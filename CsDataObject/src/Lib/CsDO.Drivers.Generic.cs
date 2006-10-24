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

namespace CsDO.Drivers
{
	/// <summary>
	/// Summary description for CsDO.
	/// </summary>
	public class Generic : IDataBase
	{
		public Generic() {}

		public IDbCommand getCommand(String sql) {
			IDbCommand command = objectCommand();
            if (command != null)
            {
                command.CommandText = sql;
                command.Connection = objectConnection();
                if (command.Connection != null)
                    command.Connection.Open();
            }
			return command;
		}

        public IDataAdapter getDataAdapter(IDbCommand command)
        {
            IDataAdapter da = objectDataAdapter(command);
            return da;
        }
		
		public IDbCommand getSystemCommand(String sql) {
			return this.getCommand(sql);
		}

		protected IDbConnection objectConnection() {
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
			IDbConnection conexao = null;

			foreach (Type tipo in tipos) {
				Type getInterface = tipo.GetInterface("IDbConnection");

				if (getInterface != null) {
					Assembly assCon = Assembly.Load(tipo.Assembly.GetName().FullName);

					if (prefix != null && prefix != string.Empty) {
						string[] namespaces = tipo.Namespace.Split('.');

						if (namespaces[namespaces.Length - 1] == prefix) {
							string obj = tipo.Namespace + "." + tipo.Name;
							conexao = (IDbConnection) assCon.CreateInstance(obj);
							break;
						}
					}
					else {
						string obj = tipo.Namespace + "." + tipo.Name;
						conexao = (IDbConnection) assCon.CreateInstance(obj);
						break;
					}
				}
			}
			if (conexao != null) {
                if (ConfigurationManager.AppSettings["connection"] != null)
                {
                    conexao.ConnectionString = ConfigurationManager.AppSettings["connection"].ToString();
				}
			}

			return conexao;

		}


		protected IDbCommand objectCommand() {
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
			IDbCommand comando = null;
			foreach (Type tipo in tipos) {
				Type getInterface = tipo.GetInterface("IDbCommand");
				if (getInterface != null) {
					Assembly assCon = Assembly.Load(tipo.Assembly.GetName());

					if (prefix != null && prefix != string.Empty) {
						string[] namespaces = tipo.Namespace.Split('.');

						if (namespaces[namespaces.Length - 1] == prefix) {
							string obj = tipo.Namespace + "." + tipo.Name;
							return comando = (IDbCommand) assCon.CreateInstance(obj);
						}
					}

					else {
						string obj = tipo.Namespace + "." + tipo.Name;
						return comando = (IDbCommand) assCon.CreateInstance(obj);
					}
				}
			}
			return comando;
		}

		protected IDbDataAdapter objectDataAdapter() {
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
			IDbDataAdapter dataAdapter = null;
			foreach (Type tipo in tipos) {
				Type getInterface = tipo.GetInterface("IDbDataAdapter");
				if (getInterface != null) {
					Assembly assCon = Assembly.Load(tipo.Assembly.GetName());
					if (prefix != null && prefix != string.Empty) {
						string[] namespaces = tipo.Namespace.Split('.');

						if (namespaces[namespaces.Length - 1] == prefix) {
							string obj = tipo.Namespace + "." + tipo.Name;
							return dataAdapter = (IDbDataAdapter) assCon.CreateInstance(obj);
						}
					}
					else {
						string obj = tipo.Namespace + "." + tipo.Name;
						return dataAdapter = (IDbDataAdapter) assCon.CreateInstance(obj);
					}
				}
			}
			return dataAdapter;
		}


        protected IDbDataAdapter objectDataAdapter(IDbCommand command)
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
            IDbDataAdapter dataAdapter = null;
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
                            string obj = tipo.Namespace + "." + tipo.Name;
                            return dataAdapter = (IDbDataAdapter)assCon.CreateInstance(obj,
                                                                                false,
                                                                                BindingFlags.InvokeMethod,
                                                                                null,
                                                                                new object[] { command },
                                                                                null,
                                                                                null
                                                                                );
                        }
                    }
                    else
                    {
                        string obj = tipo.Namespace + "." + tipo.Name;
                        return dataAdapter = (IDbDataAdapter)assCon.CreateInstance(obj,
                                                                            false,
                                                                            BindingFlags.InvokeMethod,
                                                                            null,
                                                                            new object[] {command},
                                                                            null,
                                                                            null
                                                                            );
                    }
                }
            }
            return dataAdapter;
        }

	}
}