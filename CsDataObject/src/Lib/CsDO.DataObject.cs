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
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace CsDO.Lib
{
	public delegate int AutoIncrement(DataObject table);
    //public delegate int Identity(DataObject table);

    /// <summary>
    /// Represents current state of disposable object.
    /// </summary>
    public enum DisposableState
    {
        /// <summary>
        /// Disposable object is not disposed.
        /// </summary>
        Alive = 0,
        /// <summary>
        /// Disposing aquired and is in progress.
        /// </summary>
        Disposing,
        /// <summary>
        /// Disposable object is disposed.
        /// </summary>
        Disposed,
    }

	[Serializable()]
    public class DataObject: IDisposable, ICloneable
	{
		#region Properties
		private bool debug = false;

		private IDataReader dr;

		private bool _persisted = false;
		private string _table = null;
		private string _fields = null;
		private string _where = null;
		private string _limit = null;
		private string _orderBy = null;
		private string _groupBy = null;
		private IList _primaryKeys = null;
		private IList _foreignKeys = null;
        private int depth = 3;

        protected event AutoIncrement autoIncrement = null;
      //  protected event Identity identity = null; //for SQL SERVER


		[Column(false)]
		protected string Table
		{
			get
			{
				if (_table == null) {
					_table = getTableProperties();
					
					if (_table == null) {
						string[] fullName = GetType().FullName.Split('.');
						return fullName[fullName.Length-1].ToString();
					}
				}
				
				return _table;
			}
		}
		
		protected string Fields {
			get {
				if (_fields == null) {
					_fields = "";
					PropertyInfo [] props = GetType().GetProperties();
					
					foreach (PropertyInfo propriedade in props)
					{
						string name = null;
						bool persist = true;
						
						getColumnProperties(propriedade, ref name, ref persist);
						
                        if (!persist)
                            continue;
						
						if (name == null)
							name = propriedade.Name;
						
						_fields += name + ",";
					}
					_fields = _fields.Substring(0, _fields.Length -1);
				}
				
				return _fields;
			}
		}

		protected string ActiveFields {
			get 
            {
                string _active_fields = "";
				PropertyInfo [] props = GetType().GetProperties();
				
				
				foreach (PropertyInfo propriedade in props)
				{
					string name = null;
					bool persist = true;

					getColumnProperties(propriedade, ref name, ref persist);

					if (!persist)
						continue;

                    if (!propriedade.PropertyType.IsSubclassOf(typeof(DataObject)))
                    {
                        string temp = (propriedade.GetValue(this, null) != null) ?
                            propriedade.GetValue(this, null).ToString() : null;

                        if (temp == null)
                            continue;
                        if (temp.Equals("0"))
                            continue;
                        if (propriedade.GetValue(this, null).GetType() == typeof(System.DateTime))
                            temp = ((DateTime)propriedade.GetValue(this, null)).ToString("yyyyMMdd HH:mm:ss");
                        if (temp.Equals("00010101 00:00:00"))
                            continue;
                        if (temp.Equals("NULL"))
                            continue;
                    }
                    else
                    {
                        string temp = (propriedade.GetValue(this, null) != null) ?
                            propriedade.GetValue(this, null).ToString() : null;
                        if (temp == null)
                            continue;
                    }
					
					if (name == null)
						name = propriedade.Name;

                    _active_fields += name + ",";
				}
                if (_active_fields.Length > 0)
                    _active_fields = _active_fields.Substring(0, _active_fields.Length - 1);

                return _active_fields;
			}
		}

		[Column(false)]
		protected IList PrimaryKeys {
			get {
				if (_primaryKeys == null) {
					PropertyInfo [] props =  GetType().GetProperties();
					_primaryKeys = new ArrayList();
					
					foreach (PropertyInfo propriedade in props)
					{
						object[] attributes = propriedade.GetCustomAttributes(typeof(PrimaryKey), true);
						
						if (attributes.Length > 0)
							_primaryKeys.Add(propriedade);
						
					}
				}
				
				return _primaryKeys;
			}
		}
		
		[Column(false)]
		protected IList ForeignKeys {
			get {
				if (_foreignKeys == null) {
					PropertyInfo [] props =  GetType().GetProperties();
					_foreignKeys = new ArrayList();
					
					foreach (PropertyInfo propriedade in props)
					{
						if (propriedade.PropertyType.IsSubclassOf(typeof(DataObject)))
							_foreignKeys.Add(propriedade);
					}
				}
				
				return _foreignKeys;
			}
		}
		
		[Column(false)]
		protected bool Debug
		{
			get { return debug; }
			set { debug = value; }
		}
						
		[Column(false)]
		protected string Where
		{
			get { return _where; }
			set { _where = value; }
		}		
		
		[Column(false)]
		protected string Limit
		{
			get { return _limit; }
			set { _limit = value; }
		}
		
		[Column(false)]
		protected string OrderBy
		{
			get { return _orderBy; }
			set { _orderBy = value; }
		}
		
		[Column(false)]
		protected string GroupBy
		{
			get { return _groupBy; }
			set { _groupBy = value; }
		}	

		[Column(false)]
		protected bool Persisted
		{
			get { return _persisted; }
			set { _persisted = value; }
		}
		#endregion
		
		#region Constructors
		public DataObject() {}
		
		public DataObject(string column, object whereValue)
		{
			//TODO: Identificar o que foi alterado
			//setField(column, whereValue);
			
			//find();
			//fetch();
		}
		#endregion
		
		#region Finders
        public bool retrieve(string column, object whereValue)
        {
            if (dr != null && !dr.IsClosed)
                dr.Close();

            string sql = "SELECT " + Fields + " FROM " + Table;
            bool found = false;

            PropertyInfo propriedade = findColumn(column);

            if (propriedade != null)
            {
                string name = null;
                getColumnProperties(propriedade, ref name);

                sql += " WHERE " + name;

                if (propriedade.PropertyType == typeof(System.String))
                    sql += " LIKE ";
                else
                    sql += " = ";

                sql += formatObject(whereValue);

                found = true;
            }

            if (!found)
            {
                throw new CsDOException("Field '" + column + "' not found!");
            }

            AddModifiers(ref sql);

            try
            {
                if (debug)
                    Console.WriteLine("Query: \"" + sql + "\"");

                dr = DataBase.New().Query(sql);
            }
            catch (Exception e)
            {
                if (debug)
                    Console.WriteLine("Exception: \"" + e.Message + "\"");
                throw (e);
            }

            return true;
        }

        protected bool find(object keyValue)
        {
            string name = getPrimaryKeyName();

            setField(name, keyValue);
            return find();
        }

		public bool Get(object keyValue)
		{
            find(keyValue);
			return fetch();
		}
		
		#endregion
		
		#region Util methods
        private string getPrimaryKeyName()
        {
            string name = null;

            if (PrimaryKeys.Count > 0)
                getColumnProperties((PropertyInfo)PrimaryKeys[0], ref name);
            else
                getColumnProperties((PropertyInfo)GetType().GetProperties()[0], ref name);

            return name;
        }

        public object getPrimaryKey()
        {
            object result = null;

            if (PrimaryKeys.Count > 0)
                result = ((PropertyInfo)PrimaryKeys[0]).GetValue(this, null);
            else
                result = ((PropertyInfo)GetType().GetProperties()[0]).GetValue(this, null);

            return result;
        }

		public string GetTable() {
			return Table;
		}

		public void SetDebug(bool value) {
			debug = value;
		}
		
		public bool GetDebug() {
			return debug;
		}		
		
		protected bool loadFields(IDataReader dr, DataObject obj) {          
			if (dr != null && obj != null) {
                if (Conf.DataPooling)
                {
                    DataObject result = Conf.DataPool[obj.GetType().ToString() + "!" +
                        dr[getPrimaryKeyName()]];
                    if (result != null)
                        result.Copy(obj);
                    else
                    {
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            if (debug)
                                Console.WriteLine("Loading field[" + i + "](" + dr.GetName(i) + ")=" + dr[i] + "[" + dr[i].GetType() + "] from " + GetType());

                            obj.setField(dr.GetName(i), dr[i]);
                        }

                        Persisted = true;
                        if (Conf.DataPooling)
                            Conf.DataPool.add(obj);
                    }

                    if (debug)
                        Console.WriteLine("Loading from " + GetType() + " Cache");

                    Persisted = true;

                    return true;
                }
                else
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (debug)
                            Console.WriteLine("Loading field[" + i + "](" + dr.GetName(i) + ")=" + dr[i] + "[" + dr[i].GetType() + "] from " + GetType());

                        obj.setField(dr.GetName(i), dr[i]);
                    }

                    Persisted = true;
                    if (Conf.DataPooling)
                        Conf.DataPool.add(obj);

                    return true;
                }
			} else
			{
				if (debug && dr == null)
					Console.WriteLine("Loading field error: DataReader is NULL in " + GetType());
				if (debug && obj == null)
					Console.WriteLine("Loading field error: obj is NULL in " + GetType());
				return false;
			}
		}
		
		protected void AddModifiers(ref string sql)
		{
			if (Where != null && !Where.Trim().Equals(""))
			{	
				if (sql.ToUpper().IndexOf("WHERE") > -1)
					sql += " AND " + Where;
				else
					sql += " WHERE " + Where;
			}
					
			if (OrderBy != null && !OrderBy.Trim().Equals(""))
				sql += " ORDER BY " + OrderBy;
			
			if (GroupBy != null && !GroupBy.Trim().Equals(""))
				sql += " GROUP BY " + GroupBy;

			if (Limit != null && !Limit.Trim().Equals(""))
				sql += " LIMIT " + Limit;
			
		}

		public IList ToArray()
		{
			if (dr != null) {
				if (dr.IsClosed)
					throw new CsDOException("The search must not be closed yet !");
				
				ArrayList result = new ArrayList();
				
				while(dr.Read()) {
					DataObject obj = (DataObject) Activator.CreateInstance(GetType());
					
					loadFields(dr, obj);
					result.Add(obj);
				}
				
				return result;				
			} else			
				throw new CsDOException("No search has been performed before !");
		}

        public IList ToArray(bool listAll)
        {
            if (dr == null && listAll)
            {
                find();
            }

            return ToArray();
        }

		public override string ToString()
		{
            string result = this.GetType().ToString() + "!";

			if (PrimaryKeys.Count > 0) {						
				result += formatValue((PropertyInfo) PrimaryKeys[0]);
			} else {
				result += formatValue((PropertyInfo) GetType().GetProperties()[0]);
			}

            return result;				
		}

		protected PropertyInfo findColumn(string column) {
			PropertyInfo [] props =  GetType().GetProperties();
			bool persist = false;
			
			foreach (PropertyInfo property in props)
			{			
				string name = null;
				getColumnProperties(property, ref name, ref persist);
				
				if (!persist)
					continue;
				
				if ((name != null && column.ToLower().Equals(name.ToLower())) ||
				    (column.ToLower().Equals(property.Name.ToLower())))
				{
					return property;
				}
			}
			
			return null;
		}

		protected string formatValue(PropertyInfo property) {
				string result = "";

				if (property.PropertyType.IsSubclassOf(typeof(DataObject)))
				{
                    result = (property.GetValue(this, null) != null) ? ((DataObject)property.GetValue(this, null)).getPrimaryKey().ToString() : "NULL";
				} else
					result = (property.GetValue(this, null) != null) ? formatObject(property.GetValue(this, null)) : null;

				return result;
		}

		protected string formatObject(object valueObj) {
				CultureInfo culture = new CultureInfo("en-US");
				IFormatProvider formatNumber = culture.NumberFormat;
				string result = null;

                if (valueObj != null)
                {
                    if (valueObj.GetType() == typeof(System.String))
                    {
                        result = (valueObj != null) ? "'" + valueObj.ToString() + "'" : null;
                    }
                    else if (valueObj.GetType() == typeof(System.Int16) ||
                               valueObj.GetType() == typeof(System.Int32) ||
                               valueObj.GetType() == typeof(System.Int64))
                    {
                        result = (valueObj != null) ? valueObj.ToString() : null;
                    }
                    else if (valueObj.GetType() == typeof(System.Single))
                    {
                        result = (valueObj != null) ? ((Single)valueObj).ToString(formatNumber) : null;
                    }
                    else if (valueObj.GetType() == typeof(System.Double))
                    {
                        result = (valueObj != null) ? ((Double)valueObj).ToString(formatNumber) : null;
                    }
                    else if (valueObj.GetType() == typeof(System.Boolean))
                    {
                        result = (valueObj != null) ? ((Boolean)valueObj ? "'T'" : "'F'") : null;
                    }
                    else if (valueObj.GetType() == typeof(System.DateTime))
                    {
                        result = (valueObj != null) ? "'" + ((DateTime)valueObj).ToString("yyyyMMdd HH:mm:ss") + "'" : null;
                    }
                    else if (valueObj.GetType().IsSubclassOf(typeof(DataObject)))
                    {
                        result = (valueObj != null) ? ((DataObject)valueObj).getPrimaryKey().ToString() : null;
                    }
                    else
                        result = (valueObj != null) ? valueObj.ToString() : null;
                }

				return result;
		}

		protected string getTableProperties() {
			object[] attributes = GetType().GetCustomAttributes(typeof(Table), true);

			if (attributes.Length > 0) {
				Table table = (Table) attributes[0];

				if (table.Name !=  null)
					return table.Name;
			}

			return null;
		}

		protected void getColumnProperties(PropertyInfo property, ref string name, ref bool persist) {
			object[] attributes = property.GetCustomAttributes(typeof(Column), true);

			if (attributes.Length > 0) {
				Column column = (Column) attributes[0];
				persist = column.Persist;

				if (column.Name !=  null)
					name = column.Name;
			}
		}

		protected void getColumnProperties(PropertyInfo property, ref string name, ref bool persist, ref bool primaryKey) {
			object[] attributes = property.GetCustomAttributes(typeof(Column), true);

			if (attributes.Length > 0) {
				Column column = (Column) attributes[0];
				persist = column.Persist;

				if (column.Name !=  null)
					name = column.Name;
			}

			attributes = property.GetCustomAttributes(typeof(PrimaryKey), true);

			if (attributes.Length > 0)
				primaryKey = true;
		}

		protected void getColumnProperties(PropertyInfo property, ref string name) {
			object[] attributes = property.GetCustomAttributes(typeof(Column), true);
			
			if (attributes.Length > 0) {
				Column column = (Column) attributes[0];
				
				if (column.Name !=  null)
					name = column.Name;
			}
			
			if (name == null)
				name = property.Name;
		}
		
		protected bool isPrimaryKey(PropertyInfo property) {
			object[] attributes = property.GetCustomAttributes(typeof(PrimaryKey), true);

			return (attributes.Length > 0);
		}

        protected bool isIdentity(PropertyInfo property)
        {
            object[] attributes = property.GetCustomAttributes(typeof(Identity), true);

            return (attributes.Length > 0);
        }

		protected void getColumnProperties(PropertyInfo property, ref bool persist) {
			object[] attributes = property.GetCustomAttributes(typeof(Column), true);

			if (attributes.Length > 0) {
				Column column = (Column) attributes[0];
				persist = column.Persist;
			}
		}
		
		protected void setField(string col, object val)
		{
			PropertyInfo propriedade = findColumn(col);
			
			if (propriedade != null)
			{
				if (propriedade.PropertyType.IsSubclassOf(typeof(DataObject)) &&
					!val.GetType().IsSubclassOf(typeof(DataObject))) {
					if (val == null || val.ToString().Equals("0") || val.GetType() == typeof(DBNull)) {
						if (debug)
							Console.WriteLine("Set "+ GetType() +"[" + this + "]." + propriedade.Name+"=null(" +val.GetType()+ ")");
						propriedade.SetValue(this, null, null);
						return;
					}
				
					DataObject obj = (DataObject) propriedade.GetValue(this, null);	
					if (obj == null) {
						if (debug)
							Console.WriteLine("Activating: "+ propriedade.PropertyType);
                        if (depth > 0)
                        {
                            obj = (DataObject)Activator.CreateInstance(propriedade.PropertyType);
                            obj.depth = this.depth - 1;
                        }
                        else
                        {
                            val = assertField(val, propriedade);

                            if (debug)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val + "(" + val.GetType() + ")");
                            propriedade.SetValue(this, val, null);
                            return;
                        }
					}
						
					if (obj.PrimaryKeys.Count > 0) {
						if (debug)
							Console.WriteLine("Set "+ obj.GetType() +"[" + obj + "]." + ((PropertyInfo) obj.PrimaryKeys[0]).Name+"=" + val + "(" + val.GetType() + ")");
						obj.setField(((PropertyInfo) obj.PrimaryKeys[0]).Name, val);
						obj.find();
						obj.fetch();
						if (!obj.AssertField(((PropertyInfo) obj.PrimaryKeys[0]).Name, val)) {
							if (debug)
								Console.WriteLine("Set "+ GetType() +"[" + this + "]." + propriedade.Name + "=null(" +val.GetType() + ")");
							propriedade.SetValue(this, null, null); 
						} else {
							if (debug)
								Console.WriteLine("Set "+ GetType() +"[" + this + "]." + propriedade.Name + "="+obj+"(" +obj.GetType() + ")");
								propriedade.SetValue(this, obj, null);
						}							
					} else
						throw new CsDOException("Class '" + propriedade.PropertyType + "' has no Primary Key!");
					return;
				} else {
                    val = assertField(val, propriedade);

					if (debug)
						Console.WriteLine("Set "+ GetType() +"[" + this + "]." + propriedade.Name+"="+val+"(" +val.GetType()+ ")");
					propriedade.SetValue(this, val, null);
                    return;
				}
			}
			
			throw new CsDOException("Field '" + col + "' not found!");
		}

        private static object assertField(object val, PropertyInfo propriedade)
        {
            if (propriedade.PropertyType == typeof(bool))
            {
                if (val.ToString().ToUpper().Equals("T") || val.ToString().ToUpper().Equals("TRUE"))
                    val = (Boolean)true;
                else
                    val = (Boolean)false;
            }
            else if (propriedade.PropertyType == typeof(float) &&
                val.GetType() == typeof(double))
            {
                val = float.Parse(((double)val).ToString());
            }
            else if (val.GetType().IsSubclassOf(typeof(DBNull))
            || val.GetType() == typeof(DBNull))
            {
                if (propriedade.PropertyType == typeof(byte) ||
                    propriedade.PropertyType == typeof(int) ||
                    propriedade.PropertyType == typeof(long) ||
                    propriedade.PropertyType == typeof(float) ||
                    propriedade.PropertyType == typeof(double))
                    val = (byte)0;
                else
                    val = null;
            }
            return val;
        }

		protected bool AssertField(string col, object val)
		{
			PropertyInfo propriedade = findColumn(col);
			
			if (propriedade != null)
			{
				object obj = propriedade.GetValue(this, null);
				if (obj != null)
					return obj.ToString().Equals(val.ToString());
				else
					return obj == val;
			}
			
			throw new CsDOException("Field '" + col + "' not found!");
		}

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void Copy(DataObject obj)
        {
            if (obj.GetType().IsSubclassOf(this.GetType()) ||
                obj.GetType() == this.GetType())
            {
                foreach (FieldInfo source in this.GetType().GetFields())
                {
                    BindingFlags flags = 0 ;
                    flags |= source.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    flags |= source.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    FieldInfo destination = obj.GetType().GetField(source.Name, flags |
                        BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
                    if (!source.IsInitOnly && !source.IsStatic)
                        destination.SetValue(obj, source.GetValue(this));
                }
                foreach (PropertyInfo source in this.GetType().GetProperties())
                {
                    BindingFlags flags = 0;
                    flags |= BindingFlags.Public;
                    flags |= BindingFlags.Instance;
                    PropertyInfo destination = obj.GetType().GetProperty(source.Name, flags |
                        BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
                    if (source.CanWrite)
                        destination.SetValue(obj, source.GetValue(this, null), null);
                }
            }
        }
		#endregion
		
		#region Data manipulation
        public bool insert(DataObject obj)
        {
            if (obj != null && (this.GetType() == obj.GetType()))
                return obj.insert();
            else
                return false;
        }

        public bool update(DataObject obj)
        {
            if (obj != null && (this.GetType() == obj.GetType()))
                return obj.update();
            else
                return false;
        }

        public bool delete(DataObject obj)
        {
            if (obj != null && (this.GetType() == obj.GetType()))
                return obj.delete();
            else
                return false;
        }

		public bool insert()
		{
            PropertyInfo propriedadeIdentity = null;
            string ident = null;
            

			if (dr != null && !dr.IsClosed)
				dr.Close();
				
			string sql = "INSERT INTO " + Table;
			string values = " Values (";
			
			PropertyInfo [] props =  GetType().GetProperties();
			
			foreach (PropertyInfo propriedade in props)
			{
				bool persist = true;
				string data = null;


				getColumnProperties(propriedade, ref persist);

				if (!persist)
					continue;

				if (autoIncrement != null && isPrimaryKey(propriedade))
				{
					if (propriedade.PropertyType == typeof(int))
						propriedade.SetValue(this, autoIncrement(this), null);

                    if (debug)
                        Console.WriteLine("PrimaryKEY = " + ident);
                }

                if (isIdentity(propriedade))
                {
                    getColumnProperties(propriedade, ref ident);
                    propriedadeIdentity = propriedade;
                    
                    if (debug)
                        Console.WriteLine("PrimaryKEY" + ident);
                }

				data = formatValue(propriedade);
								
				if ((data != null) && !data.Equals("0")
                    && !data.Equals("'00010101 00:00:00'") 
                    && !data.Equals("NULL"))
					values += data + ",";
        
                
			}
			
			values = values.Substring(0, values.Length -1);
			values += ")";
			sql += " (" + ActiveFields + ")" + values;

            int i=0;
            
            if (debug)
                Console.WriteLine("Ident = "+ident);

            if (ident != null)
            {
                sql += " SELECT " + ident + "=@@Identity FROM " + Table;
                if (debug)
                    Console.WriteLine("Exec: \"" + sql + "\"");

                //int cod = DataBase.New().Exec(sql);
                int result = 0;   
                IDataReader dr1 = DataBase.New().Query(sql);
                if (dr1.Read())
                {
                    if (debug)
                        Console.WriteLine("Vou pegar os dados da consulta");

                    result = Convert.ToInt32(dr1[ident]);

                    if (debug)
                    Console.WriteLine("Codigo = " + result);
                }
                //i = result;
                if (propriedadeIdentity.PropertyType == typeof(int))
                    propriedadeIdentity.SetValue(this, result, null);
                   // propriedadeIdentity.SetValue(this, identity(this), null);
            }
            else
            {
                if (debug)
                    Console.WriteLine("Exec: \"" + sql + "\"");
                i = DataBase.New().Exec(sql);
                if (Conf.DataPooling)
                    Conf.DataPool.add(this);
            }

			if (debug)
				Console.WriteLine("Affected " + i +" rows");

			Persisted = (i == 1);
			return (i == 1);
		}

		public bool deleteCascade()
		{
			bool result = false;

			if (Persisted)
				result = delete();

			if (ForeignKeys.Count > 0)
				foreach(PropertyInfo foreignKey in ForeignKeys) {
					DataObject obj = (DataObject) foreignKey.GetValue(this, null);
					if (obj != null && obj.Persisted) {
						if (debug)
							Console.WriteLine("**** Deleting " + foreignKey.Name + " ...");
						result = result || obj.deleteCascade();
						obj = null;
						foreignKey.SetValue(this, null, null);
					}
					else if (debug)
						if (obj != null)
							Console.WriteLine("**** Skipped " + foreignKey.Name + "[" + obj.ToString() + "] ...");
						else
							Console.WriteLine("**** Skipped " + foreignKey.Name + " ...");
				}

			return result;
		}

		public bool delete()
		{
			if (dr != null && !dr.IsClosed)
				dr.Close();
			
			string sql = "DELETE ";
			string clausule = "";
			string item = "";
			string search = "";
			string operador = "";
			bool hasWhere = false;
			
			PropertyInfo [] props =  GetType().GetProperties();
			
			foreach (PropertyInfo propriedade in props)
			{
				bool primaryKey = false;
				bool persist = true;
				string name = null;
				
				getColumnProperties(propriedade, ref name, ref persist, ref primaryKey);
				
				if (!persist)
					continue;
				
				if (name == null)
					name = propriedade.Name;
				
				operador = " = ";

				if (propriedade.PropertyType == typeof(System.String))
				{
					operador = " LIKE ";
				} 
				
				search = formatValue(propriedade);
				
				//TODO: Identificar o que foi alterado
				if ((search != null) && !search.Equals("0"))
				{
					item = "("+ name + operador + search + ")";
					hasWhere = true;
					
					clausule  += item + " AND ";
				}
				
				if (primaryKey)
					break;
			}
			
			if (hasWhere) {
				clausule = clausule.Substring(0, clausule.Length -4);
				sql += "FROM " + Table + " WHERE " + clausule ;
			}

			if (hasWhere) {
				try {
					if (debug)
						Console.WriteLine("Exec: \"" +sql +"\"");
					int i = DataBase.New().Exec(sql);
					if (debug)
						Console.WriteLine("Affected " + i +" rows");
                    if (Conf.DataPooling)
                        Conf.DataPool.remove(this);
					return (i == 1);
				} catch (Exception e) {
					if (debug)
						Console.WriteLine(e.Message + "\n" + e.StackTrace);
					throw (e);
				}
			} else
				return false;
		}
		
		public bool update()
		{
			if (dr != null && !dr.IsClosed)
				dr.Close();
			
			string sql = "UPDATE ";
			string clausule = "";
			string item = "";
			string search = "";
			string operador = "";
			string values = "";
			bool hasWhere = false;
			
			PropertyInfo [] props =  GetType().GetProperties();
				
			foreach (PropertyInfo propriedade in props)
			{
				bool primaryKey = false;
				bool persist = true;
				string name = null;

				getColumnProperties(propriedade, ref name, ref persist, ref primaryKey);

				if (!persist)
					continue;
				
				if (name == null)
					name = propriedade.Name;
				
				operador = " = ";

				if (propriedade.PropertyType == typeof(System.String))
				{
					operador = " LIKE ";
				} 
				
				search = formatValue(propriedade);
				
				//TODO: Identificar o que foi alterado
				if (primaryKey && (search != null) && !search.Equals("0")
                    && !search.Equals("'00010101 00:00:00'") 
                    && !search.Equals("NULL"))
				{
					item = "("+ name + operador + search + ")";
					hasWhere = true;
					
					clausule  += item + " AND ";
				} else if ((search != null) && !search.Equals("0")
                    && !search.Equals("'00010101 00:00:00'")
                    && !search.Equals("NULL"))

					values += name + "=" + search + ",";
				
			}
					
			if (!values.Equals(""))
				values = values.Substring(0, values.Length - 1);
			
			if (hasWhere) {
				clausule = clausule.Substring(0, clausule.Length - 4);
				sql += Table + " SET " + values + " WHERE " + clausule ;
			}

			if (hasWhere) {
				if (debug)
					Console.WriteLine("Exec: \"" +sql +"\"");
				int i = DataBase.New().Exec(sql);
				if (debug)
					Console.WriteLine("Affected " +i +" rows");
                if (Conf.DataPooling)
                {
                    Conf.DataPool.remove(this);
                    Conf.DataPool.add(this);
                }
				Persisted = (i == 1);
				return (i == 1);
			} else
				return false;
		}
		#endregion

		#region Query
        public IList select(object keyValue)
        {
            DataSet result = new DataSet(Table);

            if (keyValue != null)
            {
                find(keyValue);
            }
            else
            {
                find();
            }



            return ToArray();
        }

		public bool find()
		{
			if (dr != null && !dr.IsClosed)
				dr.Close();
			
			string sql = "SELECT ";
			string clausule = "";
			string item = "";
			string search = "";
			string operador = "";
			bool hasWhere = false;
			
			PropertyInfo [] props =  GetType().GetProperties();
			
			foreach (PropertyInfo propriedade in props)
			{
				bool primaryKey = false;
				bool persist = true;
				string name = null;
				
				getColumnProperties(propriedade, ref name, ref persist, ref primaryKey);
				
				if (!persist)
					continue;
				
				if (name == null)
					name = propriedade.Name;
				
				operador = " = ";

				if (propriedade.PropertyType == typeof(System.String))
				{
					operador = " LIKE ";
				} 
				
				search = formatValue(propriedade);
				
				//TODO: Identificar o que foi alterado
                if (search != null && !search.Equals("0")
                    && !search.Equals("'00010101 00:00:00'")
                    && !search.Equals("NULL"))
			        {
				        item = "("+ name + operador + search + ")";
				        hasWhere = true;
    					
				        clausule  += item + " AND ";
			        }
				
				if (primaryKey && (search != null) && !search.Equals("0")
                    && !search.Equals("00010101 00:00:00"))
					break;
			}
					
			if (hasWhere) {
				clausule = clausule.Substring(0, clausule.Length -4);
				sql += Fields + " FROM " + Table + " WHERE " + clausule ;
			} else
				sql += Fields + " FROM " + Table;
			
			AddModifiers(ref sql);
			
			try 
			{
				if (debug)
					Console.WriteLine("Query: \"" +sql +"\"");				
				dr = DataBase.New().Query(sql);
			} catch(Exception e) {
				if (debug)
					Console.WriteLine("Exception: \"" + e.Message +"\"");
                throw (e);
			}

			return true;
		}
		
		///<summary>Repassa dos dados da consulta para o objeto</summary>
		public bool fetch()
		{
			if ((dr != null) && !dr.IsClosed) {
				bool result = dr.Read();
				if (debug) {
					if (result)
						Console.WriteLine("*** Reading DataReader and loading fields ...");
					else
						Console.WriteLine("*** Closing DataReader ...");
				}
				if (!result) {
					dr.Close();
				} else
					loadFields(dr, this);

				return result;
			}
			
			if (debug) {
				if (dr == null)
					Console.WriteLine("*** DataReader is NULL !!!");

				if (dr.IsClosed)
					Console.WriteLine("*** DataReader is CLOSED !!!");
			}
			
			return false;
		}
		#endregion

        #region Operators
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(DataObject o1, DataObject o2)
        {
            if ((Object)o1 == null)
                return (Object)o2 == null;
            else
            {
                if ((Object)o2 == null)
                    return false;

                if (o1.GetType().IsInstanceOfType(o2.GetType()))
                    return false;
                else
                    return o1.Equals(o2);
            }
        }

        public static bool operator !=(DataObject o1, DataObject o2)
        {
            return !(o1 == o2);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            else
            {
                if (Object.ReferenceEquals(this, obj))
                    return true;

                DataObject dataObject = obj as DataObject;
                if ((Object)dataObject == null)
                    return false;
                else
                    return (this.GetHashCode() == dataObject.GetHashCode());
            }
        }
        #endregion

        #region IDisposable implementation
        private DisposableState state = DisposableState.Alive;
        private void InternalDispose(bool disposing)
        {
            state = DisposableState.Disposing;
            Dispose(disposing);
            state = DisposableState.Disposed;
        }
        /// <summary>
        /// Releases all resources used by the object.
        /// </summary>
        public void Dispose()
        {
            if (state == DisposableState.Alive)
            {
                InternalDispose(true);
                // Take yourself off of the Finalization queue 
                // to prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }
        }
        protected void Dispose(bool disposing)
        {
        }
        ~DataObject()
        {
            InternalDispose(false);
        }
        /// <summary>
        /// Gets is object in disposing process now.
        /// </summary>
        [Column(false)]
        protected bool Disposing
        {
            get { return state == DisposableState.Disposing; }
        }
        /// <summary>
        /// Gets is object disposed and in nonoperational state.
        /// </summary>
        [Column(false)]
        protected bool Disposed
        {
            get { return state == DisposableState.Disposed; }
        }
        /// <summary>
        /// Gets disposable object state.
        /// </summary>
        [Column(false)]
        protected DisposableState State
        {
            get
            {
                return state;
            }
        }
        public DisposableState getState()
        {
            return State;
        }
        #endregion

        #region TODO
        /*
		//-- send a raw query
		public bool query()
		{
			return true;
		}
		
		// Perform a select count() request
		public bool count()
		{
			return true;
		}
		
		//-- Add selected columns
		public bool  selectAdd()
		{
			return true;
		}		
		
		//-- Escape a string for use with Like queries
		public bool escape()
		{
			return true;
		}
				
		// Automatic Table Linking and Joins
		//--  Automatic Table Linking - ::getLink(), ::getLinks(), ::joinAdd(), ::selectAs()
		
		// -- fetch and return a related object
		//public object getLink()
		{
		 object ob;
		 return ob;
		 
		}

		//-- load related objects
		public ArrayList getLinks()
		{
			ArrayList list = new ArrayList();
			return list;
		}
		
		//-- Build the select component of a query (usually for joins)
		public bool selectAs()
		{
			return true;
		}
		
		//-- add another dataobject to build a create join query
		public bool joinAdd()
		{
			return true;
		}
		
		//-- Copy items from Array or Object (for form posting)
		public bool setFrom()
		{
			return true;
		}
	
		//-- check object data, and call objects validation methods.
		public bool validate()
		{
			return true;
		}
		*/
		#endregion
	}
}
