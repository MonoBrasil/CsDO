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
using System.Data.Common;

namespace CsDO.Lib
{
    #region Conf

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

    #endregion

    #region IDatabase

    public interface IDataBase
    {
        DbCommand getCommand(String sql);
        DbCommand getSystemCommand(String sql);
        DbDataAdapter getDataAdapter(DbCommand command);
        DbConnection getConnection();
        void open(string URL);
        void close();
        DbConnection Connection { get; }
        DataTable getSchema();
        DataTable getSchema(string collectionName);
        DataTable getSchema(string collectionName, string[] restrictions);
    } 

    #endregion

	public class DataBase : Singleton, IDisposable
	{
		protected IList dataReaders = new ArrayList();

		new public static DataBase New() { return (DataBase) Instance(typeof(DataBase)); }

        #region IDisposable Members

        public void Dispose()
        {
            DisposeDataReaders();
        }

        private void DisposeDataReaders()
        {
            if (dataReaders.Count > 0)
            {
                foreach (IDataReader dataReader in dataReaders)
                {
                    dataReader.Dispose();
                }
            }
            dataReaders.Clear();
        }

        #endregion

		public int Exec(String query)
		{
            if (query.ToLower().Contains("update"))
                Conf.DataPool.Clear();

			DisposeDataReaders();
			IDbCommand command = Conf.Driver.getCommand(query);
    		Int32 rowsaffected;
    		
	    	rowsaffected = command.ExecuteNonQuery();

		    return rowsaffected;
		}

        public int Exec(string query, IDataParameter[] Params)
        {
            if (query.ToLower().Contains("update"))
                Conf.DataPool.Clear();

            DisposeDataReaders();
            IDbCommand command = Conf.Driver.getCommand(query);

            if (Params != null)
            {
                foreach (IDataParameter param in Params)
                {
                    if (param.Value == null)
                        param.Value = DBNull.Value;

                    if (command.Parameters.Contains(param))
                        command.Parameters[param.ParameterName] = param;
                    else
                        command.Parameters.Add(param);
                }
            }

            int result = command.ExecuteNonQuery();

            return result;
        }

		public int ExecSys(String query)
		{
			IDbCommand command = Conf.Driver.getSystemCommand(query);
    		Int32 rowsaffected;
    		
        	rowsaffected = command.ExecuteNonQuery();
		    
		    return rowsaffected;
    	}
		
		public IDataReader Query(String query)
		{
			DisposeDataReaders();
		    IDbCommand command = Conf.Driver.getCommand(query);
			IDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            dataReaders.Add(dr);
			return dr;
		}

        public IDataReader Query(CommandType cmdType, string query, IDataParameter[] Params) 
		{
            DisposeDataReaders();
            IDbCommand command = Conf.Driver.getCommand(query);

            if (cmdType == CommandType.StoredProcedure)
                command.CommandText = query;
            command.CommandType = cmdType;

            if (Params != null)
            {
                foreach (IDataParameter param in Params)
                {
                    if (param.Value == null)
                        param.Value = DBNull.Value;

                    if (command.Parameters.Contains(param))
                        command.Parameters[param.ParameterName] = param;
                    else
                        command.Parameters.Add(param);
                }
            }

            IDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            dataReaders.Add(dr);
            return dr;
        }	

        public DataSet QueryDS(CommandType cmdType, string query, DbParameter[] Params) 
		{
            DbCommand command = Conf.Driver.getCommand(query);
            IDataAdapter da = Conf.Driver.getDataAdapter(command);
            DataSet ds = new DataSet();
            command.CommandTimeout = 90;
            command.CommandType = cmdType;
            try
            {

                if (Params != null)
                {
                    foreach (DbParameter param in Params)
                    {
                        if (command.Parameters.Contains(param))
                            command.Parameters[param.ParameterName] = param;
                        else
                            command.Parameters.Add(param);
                    }
                }

                da.Fill(ds);

                command.Parameters.Clear();
            }
            catch (Exception ex)
            {
                da = null;
                ds.Clear();
                ds = null;
                throw (ex);
            }

            return ds;
        }
    }
}
