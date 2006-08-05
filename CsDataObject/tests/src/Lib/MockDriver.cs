/*
 * Created by Alexandre Rocha Lima e Marcondes
 * User: Administrator
 * Date: 13/08/2005
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
using System.Collections;
using System.Data;
using System.Data.OleDb;
using NUnit.Framework;
using CsDO.Lib;

namespace CsDO.Tests {

	public class MockDbConnection : IDbConnection
	{
		private string connectionString;
		private string database;
		private ConnectionState state;

		public string ConnectionString { get { return connectionString; } set { connectionString = value; } }		
		public int ConnectionTimeout  { get { return 95248447; } }
		public string Database { get { return database; } }
		public ConnectionState State { get { return state; } }

		public void Dispose() { }
		public void Open() { state =  ConnectionState.Open;  }
		public void Close() { state =  ConnectionState.Closed;  }
		public IDbCommand CreateCommand() { return new MockDbCommand(); }
		public void ChangeDatabase(string databaseName) { database = databaseName;  }
		public IDbTransaction BeginTransaction() { return new MockDbTransaction(); }
		public IDbTransaction BeginTransaction(IsolationLevel  il) { return BeginTransaction(); }
	}

	public class MockDbCommand : IDbCommand
	{
		private string commandText;
		private string[] commands;
		private int commandTimeout;
		private CommandType commandType;
		private IDbConnection connection;
		private IDataParameterCollection parameters;
		private IDbTransaction transaction;
		private UpdateRowSource updatedRowSource;

		public string CommandText 
		{
			get { return commandText; } 
			set 
			{ 
				commands = commandText.Split(' ');
				commandText = value; 
			} 
		}
		public int CommandTimeout { get { return commandTimeout; } set { commandTimeout = value; } }
		public CommandType CommandType { get { return commandType; } set { commandType = value; } }
		public IDbConnection Connection { get { return connection; } set { connection = value; } }
		public IDataParameterCollection Parameters { get { return parameters; } set { parameters = value; } }
		public IDbTransaction Transaction { get { return transaction; } set { transaction = value; } }
		public UpdateRowSource UpdatedRowSource { get { return updatedRowSource; } set { updatedRowSource = value; } }

		public MockDbCommand() 
		{
			commandText = "";
			connection = null;
		}
		
		public MockDbCommand(string sql, IDbConnection conn) 
		{
			commandText = sql;
			commands = commandText.Split(' ');
			connection = conn;
		}
		
		public void Dispose() { }
		public void Cancel() { }
		public void Prepare() { }
		public object ExecuteScalar() { return (String) "First Column"; }
		public IDataReader ExecuteReader() 
		{ 
			for(int i = 0; i < commands.Length; i++) {
				if (commands[i].ToUpper().Equals("FROM")) {
					return new MockDataReader(commands[i+1]);
				}
			}
			
			return null;
		}
		public IDataReader ExecuteReader(CommandBehavior behavior) { return this.ExecuteReader(); }
		public int ExecuteNonQuery() 
		{ 
			commands = commandText.Split(' ');
			
			if (commandText.ToUpper().StartsWith("INSERT")) {
				return 1;
			} else if (commandText.ToUpper().StartsWith("UPDATE")) {
				if (commandText.ToUpper().IndexOf("WHERE") > -1)
					return 1;
				else
					return 99248446;
			} else if (commandText.ToUpper().StartsWith("DELETE")) {
				if (commandText.ToUpper().IndexOf("WHERE") > -1)
					return 1;
				else
					return 99248446;
			} else if (commandText.ToUpper().StartsWith("CREATE")) {
				return 1;
			} else if (commandText.ToUpper().StartsWith("DROP")) {
				return 1;
			} else
				return 0;
		}
		
		public IDbDataParameter CreateParameter() 
		{ 
			IDbDataParameter result = new OleDbParameter();
			if (parameters != null)
				parameters.Add(result);
			return result;
		}
	}

	public class MockDbTransaction : IDbTransaction
	{
		public IDbConnection Connection { get { return new MockDbConnection(); } }
		public IsolationLevel IsolationLevel  { get { return IsolationLevel.Unspecified; } }

		public void Dispose() { }
		public void Commit() { }
		public void Rollback() { }
	}

	public class MockDataReader : IDataReader
	{
		private MockDriver driver = (MockDriver) Conf.Driver;
		private string tableName = "";
		private DataTable table = null;
		private int row = -1;
		private bool isClosed = true;

		public int Depth { get { return 95248447; } }
		public bool IsClosed {get { return isClosed; } }
		public int RecordsAffected { get { return 95248447; } }
		public int FieldCount { get { return driver.getColumnCount(tableName); } }
		public object this[string s] { get { return GetValue(GetOrdinal(s)); } }
		public object this[int i] { get { return GetValue(i); } }

		public MockDataReader(string table) {
			isClosed = false;
			tableName = table;
			this.table = driver.dataset.Tables[table];
		}
		
		public void Dispose() { }
		public void Close() { isClosed = true; }
		public DataTable GetSchemaTable() { return null; }
		public bool NextResult() 
		{ 
			if (!isClosed && (row < table.Rows.Count)) {
				row++;
				return true;
			}
			
			return false;
		}
		
		public bool Read() { return NextResult(); }
		
		public bool GetBoolean(int i) { return (bool) table.Rows[row].ItemArray[i]; }
		public byte GetByte(int i) { return (byte) table.Rows[row].ItemArray[i]; }
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { return fieldOffset - 1; }
		public char GetChar(int i) { return (char) table.Rows[row].ItemArray[i]; }
		public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length) { return fieldOffset - 1; }
		public IDataReader GetData(int i) { return this; } 
		public string GetDataTypeName(int i) { return (String) table.Columns[i].DataType.Name; }
		public DateTime GetDateTime(int i) { return(DateTime) table.Rows[row].ItemArray[i]; }
		public decimal GetDecimal(int i) { return (decimal) table.Rows[row].ItemArray[i]; }
		public double GetDouble(int i) { return (double) table.Rows[row].ItemArray[i]; }
		public Type GetFieldType(int i) { return  table.Columns[i].DataType; }
		public float GetFloat(int i) { return (float) table.Rows[row].ItemArray[i]; }
		public Guid GetGuid(int i) { return (Guid) table.Rows[row].ItemArray[i]; }
		public short GetInt16(int i) { return (short) table.Rows[row].ItemArray[i]; }
		public int GetInt32(int i) { return (int) table.Rows[row].ItemArray[i]; }
		public long GetInt64(int i) { return (long) table.Rows[row].ItemArray[i]; }
		public string GetName(int i) { return table.Columns[i].ColumnName; }
		public int GetOrdinal(string name) { return table.Columns.IndexOf(name); }
		public string GetString(int i) { return (String) table.Rows[row].ItemArray[i]; }
		public object GetValue(int i) { return table.Rows[row].ItemArray[i]; }	
		public bool IsDBNull(int i) { return table.Rows[row].IsNull(i); }
		public int GetValues(object[] values) 
		{ 
			int length = table.Rows[row].ItemArray.Length;
			table.Rows[row].ItemArray.CopyTo(values, length);
			return length; 
		}
		
	}
	
	public class MockDriver : IDataBase
	{
		private MockDbConnection conn = null;
		private MockDbCommand prevCommand = null;

        public IDataAdapter getDataAdapter(IDbCommand command)
        {
            return null;
        }
		
		protected internal System.Data.DataSet dataset = new DataSet("MockData");
				
		public void addColumn(string table, string name, Type type) {
			addColumn(table, name, type, false, false);
		}
		
		public void addTable(string name) {
			DataTable table = new DataTable(name);
			dataset.Tables.Add(table);
		}
			
		public void clearTables() {
			dataset.Tables.Clear();
		}
		
		public void addColumn(string table, string name, Type type, bool unique, bool readOnly) {
			DataColumn column = new DataColumn();
			
    		column.DataType = type;
    		column.ColumnName = name;
		    column.ReadOnly = readOnly;
    		column.Unique = unique;
    		
    		dataset.Tables[table].Columns.Add(column);
		}
		
		public void addRow(string table, DataRow row) {
			dataset.Tables[table].Rows.Add(row);
		}

		public DataRow newRow(string table) {
			return dataset.Tables[table].NewRow();
		}		
		
		public DataColumn getColumn(string table, int index) {
			return dataset.Tables[table].Columns[index];
		}		
		
		public int getColumnCount(string table) {
			return dataset.Tables[table].Columns.Count;
		}
		
		public int getColumnIndex(string table, string name) {
			return dataset.Tables[table].Columns.IndexOf(name);
		}

		public void clearTable(string table) {
			dataset.Tables[table].Columns.Clear();
			dataset.Tables[table].Rows.Clear();
		}
		
		public void setPrimaryKey(string table, string name) {
			if (name != null) {
				DataColumn[] PrimaryKeyColumns = new DataColumn[1];
    			PrimaryKeyColumns[0] = dataset.Tables[table].Columns["id"];
    			dataset.Tables[table].PrimaryKey = PrimaryKeyColumns;
			} else {
				dataset.Tables[table].PrimaryKey = null;
			}
		}
		
		public MockDriver() { }

		public MockDbCommand getPreviousCommand() {
			return prevCommand;
		}
		
		public MockDbConnection getPreviousConnection() {
			return conn;
		}
		
		public IDbCommand getCommand(String sql) 
		{	
			prevCommand = new MockDbCommand(sql, getConnection());
			return prevCommand;
		}

		public IDbCommand getSystemCommand(String sql) 
		{	
			prevCommand = new MockDbCommand(sql, getSystemConnection());
			return prevCommand;
		}

		protected IDbConnection getConnection() 
		{	
			if (conn == null)
    				conn = new MockDbConnection();
			
		    	return conn;
		}	

		protected IDbConnection getSystemConnection() { return getConnection();	}		
	}		

}
