/*
 * Created by Alessandro Binhara
 * User: Administrator
 * Date: 29/4/2005
 * Time: 18:13
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
using System.Collections;

namespace CsDO.Lib
{
	public class Conf
	{
		static IDataBase _driver;
        static DataPool _dataPool;
        static bool _dataPooling;

		static Conf()
		{
			_driver = new CsDO.Drivers.Generic();
            _dataPool = DataPool.New();
            _dataPooling = false;
		}

		public static IDataBase Driver
		{
			get { return _driver; }
			set { _driver = value; }
		}

        public static DataPool DataPool
        {
            get { return _dataPool; }
        }

        public static bool DataPooling
        {
            get { return _dataPooling; }
            set { _dataPooling = value; }
        }
	}

	public interface IDataBase
	{
		IDbCommand getCommand(String sql);
		IDbCommand getSystemCommand(String sql);
		IDataAdapter getDataAdapter(IDbCommand command);
	}

	public class DataBase : Singleton
	{
		protected IList dataReaders = new ArrayList();

		new public static DataBase New() { return (DataBase) Instance(typeof(DataBase)); }

		public void disposeDataReaders()
		{
			if (dataReaders.Count > 0)
				foreach(IDataReader dataReader in dataReaders) {
					if (!dataReader.IsClosed)
						dataReader.Close();
				}
				dataReaders.Clear();
		}

		public int Exec(String query)
		{
			disposeDataReaders();
			IDbCommand command = Conf.Driver.getCommand(query);
    		Int32 rowsaffected;
    		
		    try
		    {
		    	rowsaffected = command.ExecuteNonQuery();
		    } finally {
                command.Connection.Close();		   
		    }
		    return rowsaffected;
		}
		
		public int ExecSys(String query)
		{
			disposeDataReaders();
			IDbCommand command = Conf.Driver.getSystemCommand(query);
    		Int32 rowsaffected;
    		
		    try
		    {
		    	rowsaffected = command.ExecuteNonQuery();
		    } finally {
		   
		    }
		    return rowsaffected;
    		}
		
		public IDataReader Query(String query)
		{
			disposeDataReaders();
		    IDbCommand command = Conf.Driver.getCommand(query);
			IDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection);
			
			return dr;
		}

        public IDataReader Query(CommandType cmdType, string query, IDataParameter[] Params) 
		{
            disposeDataReaders();
            IDbCommand command = Conf.Driver.getCommand(query);

            //command.CommandText = query;
            command.CommandType = cmdType;

            if (Params != null)
            {
                foreach (IDataParameter param in Params)
                {
                    if (command.Parameters.Contains(param))
                        command.Parameters[param.ParameterName] = param;
                    else
                        command.Parameters.Add(param);
                }
            }

            IDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection);

            return dr;
        }	

        public DataSet QueryDS(CommandType cmdType, string query, IDataParameter[] Params) 
		{
            disposeDataReaders();
            IDbCommand command = Conf.Driver.getCommand(query);
            IDataAdapter da = Conf.Driver.getDataAdapter(command);
            DataSet ds = new DataSet();
            
            command.CommandType = cmdType;
            try
            {

                if (Params != null)
                {
                    foreach (IDataParameter param in Params)
                    {
                        if (command.Parameters.Contains(param))
                            command.Parameters[param.ParameterName] = param;
                        else
                            command.Parameters.Add(param);
                    }
                }

                da.Fill(ds);

                command.Parameters.Clear();
                command.Connection.Dispose();
            }
            catch (Exception ex)
            {
                command.Connection.Dispose();
                throw (ex);
            }

            return ds;
        }	
	}
}