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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using System.Globalization;
using System.Collections;
using System.Data.Common;

namespace CsDO.Lib
{

    #region Delegates
    public delegate int AutoIncrement(DataObject table);

    public delegate void OnBeforeManipulation(DataObject sender);
    public delegate void OnAfterManipulation(DataObject sender, bool success);
    //public delegate int Identity(DataObject table);
    #endregion

    #region Disposable State
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
    #endregion

    [Serializable()]
    public class DataObject : IDisposable, ICloneable
    {
        #region Properties
        private bool debug = false;

        /// <summary>
        /// Armazena o resultado de uma busca efetuada pelos métodos find() ou Get()
        /// </summary>
        [NonSerialized()]
        private DataTable _resultSet;

        /// <summary>
        /// Indica qual linha do resultado de uma pesquisa deve ser utilizada,
        /// pelo método fetch(), para popular a instância
        /// </summary>
        private int rowsCounter = 0;

        /// <summary>
        /// Hashtable que contém as transações ativas com o SGBD.
        /// Utilizada pelos métodos BeginTransaction, CommitTransaction e RollBackTransaction.
        /// </summary>
        private static Hashtable transactions = new Hashtable();

        private bool _persisted = false;
        private string _fields = null;
        private string _where = null;
        private string _limit = null;
        private string _orderBy = null;
        private string _groupBy = null;
        private IList _primaryKeys = null;
        private IList _foreignKeys = null;
        private int depth = 3;

        /// <summary>
        /// Retorna/Armazena o resultado de uma busca efetuada pelos métodos find() ou Get()
        /// </summary>
        protected DataTable ResultSet
        {
            get
            {
                return _resultSet;
            }
            set
            {
                this._resultSet = value;
                //Serve para indicar ao fetch qual linha usar, deve ser zerado a cada nova pesquisa.
                this.rowsCounter = 0;
            }
        }

        [Column(false)]
        public int Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        /// <summary>
        /// Retorna o nome da tabela, no SGBD, relacionada à classe
        /// </summary>
        [Column(false)]
        private string Table
        {
            get
            {
                object[] attributes = GetType().GetCustomAttributes(typeof(Table), true);

                if (attributes.Length > 0)
                {
                    Table table = (Table)attributes[0];

                    if (!String.IsNullOrEmpty(table.Name))
                        return table.Name;
                }
                return this.GetType().Name;
            }
        }

        //[Column(false)]
        //protected string Table
        //{
        //    get
        //    {
        //        if (_table == null)
        //        {
        //            _table = getTableProperties();

        //            if (_table == null)
        //            {
        //                string[] fullName = GetType().FullName.Split('.');
        //                return fullName[fullName.Length - 1].ToString();
        //            }
        //        }

        //        return _table;
        //    }
        //}

        protected string Fields
        {
            get
            {
                if (_fields == null)
                {
                    StringBuilder fields = new StringBuilder();
                    PropertyInfo[] props = GetType().GetProperties();

                    foreach (PropertyInfo propriedade in props)
                    {
                        string name = null;
                        bool persist = true;

                        getColumnProperties(propriedade, ref name, ref persist);

                        if (!persist)
                            continue;

                        if (name == null)
                            name = propriedade.Name;

                        fields.Append(name);
                        fields.Append(",");
                    }
                    fields.Remove(fields.Length - 1, 1);
                    _fields = fields.ToString();
                }

                return _fields;
            }
        }

        protected string ActiveFields
        {
            get
            {
                StringBuilder _active_fields = new StringBuilder();
                PropertyInfo[] props = GetType().GetProperties();


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
                        DataObject temp = (propriedade.GetValue(this, null) != null) ?
                            (DataObject)propriedade.GetValue(this, null) : null;
                        if (temp == null)
                            continue;
                        if ((int)temp.getPrimaryKey() == 0)
                            continue;
                    }

                    if (name == null)
                        name = propriedade.Name;

                    _active_fields.Append(name);
                    _active_fields.Append(",");
                }
                if (_active_fields.Length > 0)
                    _active_fields.Remove(_active_fields.Length - 1, 1);

                return _active_fields.ToString();
            }
        }

        [Column(false)]
        protected IList PrimaryKeys
        {
            get
            {
                if (_primaryKeys == null)
                {
                    PropertyInfo[] props = GetType().GetProperties();
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
        protected IList ForeignKeys
        {
            get
            {
                if (_foreignKeys == null)
                {
                    PropertyInfo[] props = GetType().GetProperties();
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
        public DataObject() { }

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
            PropertyInfo[] props = GetType().GetProperties();

            foreach (PropertyInfo propriedade in props)
            {
                if (propriedade.PropertyType == typeof(bool) ||
                    propriedade.PropertyType == typeof(System.Boolean))
                {
                    propriedade.SetValue(this, false, null);
                }
                else
                {
                    if (propriedade.PropertyType == typeof(byte) ||
                        propriedade.PropertyType == typeof(System.Byte) ||
                        propriedade.PropertyType == typeof(short) ||
                        propriedade.PropertyType == typeof(System.Int16) ||
                        propriedade.PropertyType == typeof(int) ||
                        propriedade.PropertyType == typeof(System.Int32) ||
                        propriedade.PropertyType == typeof(long) ||
                        propriedade.PropertyType == typeof(System.Int64) ||
                        propriedade.PropertyType == typeof(float) ||
                        propriedade.PropertyType == typeof(System.Single) ||
                        propriedade.PropertyType == typeof(double) ||
                        propriedade.PropertyType == typeof(System.Double))
                    {
                        propriedade.SetValue(this, 0, null);
                    }
                    else
                    {
                        propriedade.SetValue(this, null, null);
                    }
                }
            }

            setField(column, whereValue);
            return find();
        }

        /// <summary>
        /// Realiza uma busca, na tabela do SGBD referente à classe da instância
        /// passando como condiçâo de busca o valor da chave primária da tabela
        /// </summary>
        /// <param name="keyValue">Valor da chave primária para a consulta ao SGBD</param>
        /// <returns>Verdadeiro caso o registro tenha sido encontrado, falso caso contrário</returns>
        protected bool find(object keyValue)
        {
            string name = getPrimaryKeyName();

            setField(name, keyValue);
            return find();
        }

        /// <summary>
        /// Obtém e repassa os dados de uma busca pela chave primária à intância
        /// </summary>
        /// <param name="keyValue">Valor da chave primária para a consulta ao SGBD</param>
        /// <returns>Verdadeiro caso o registro tenhasido encontrado, falso caso contrário</returns>
        public bool Get(object keyValue)
        {
            if (find(keyValue))
                return fetch();
            else
                return false;
        }

        #endregion

        #region Util methods
        /// <summary>
        /// Formata o valor de um objeto para ser repassado ao SGBD
        /// </summary>
        /// <param name="valueObj">Objeto a ser analisado</param>
        /// <returns>String representando o valor do objeto</returns>
        protected string formatObject(object valueObj)
        {
            if (valueObj != null)
            {
                CultureInfo culture = new CultureInfo("en-US");

                IFormatProvider formatNumber = culture.NumberFormat;

                StringBuilder sb;

                switch (valueObj.GetType().ToString())
                {
                    case "System.String":
                        if (valueObj != null)
                        {
                            sb = new StringBuilder("'");
                            sb.Append(valueObj.ToString());
                            sb.Append("'");
                            return sb.ToString();
                        }
                        else
                            return "''";

                    case "System.Nullable`1[System.Char]":
                        sb = new StringBuilder("'");
                        sb.Append(((Char)valueObj).ToString());
                        sb.Append("'");
                        return sb.ToString();

                    case "System.Char":
                        sb = new StringBuilder("'");
                        sb.Append(((Char)valueObj).ToString());
                        sb.Append("'");
                        return sb.ToString();

                    case "System.Nullable`1[System.Single]":
                        return ((Single)valueObj).ToString(formatNumber);

                    case "System.Single":
                        return ((Single)valueObj).ToString(formatNumber);


                    case "System.Nullable`1[System.Decimal]":
                        return ((Decimal)valueObj).ToString(formatNumber);

                    case "System.Decimal":
                        return ((Decimal)valueObj).ToString(formatNumber);

                    case "System.Nullable`1[System.Double]":
                        return ((Double)valueObj).ToString(formatNumber);

                    case "System.Double":
                        return ((Double)valueObj).ToString(formatNumber);

                    case "System.Nullable`1[System.Boolean]":
                        return ((Boolean)valueObj) ? "'T'" : "'F'";

                    case "System.Boolean":
                        return ((Boolean)valueObj) ? "'T'" : "'F'";

                    case "System.Nullable`1[System.DateTime]":
                        sb = new StringBuilder("'");
                        sb.Append(((DateTime)valueObj).ToString("yyyyMMdd HH:mm:ss"));
                        sb.Append("'");
                        return sb.ToString();

                    case "System.DateTime":
                        sb = new StringBuilder("'");
                        sb.Append(((DateTime)valueObj).ToString("yyyyMMdd HH:mm:ss"));
                        sb.Append("'");
                        return sb.ToString();

                    default:
                        if (valueObj.GetType().IsSubclassOf(typeof(DataObject)))
                            return formatObject(((DataObject)valueObj).getPrimaryKey());
                        else
                            return valueObj.ToString();
                }
            }
            else
                return "NULL";
        }

        /// <summary>
        /// Obtém informações de persistência de um campo, field, passado por parâmetro
        /// </summary>
        /// <param name="field">Referência do campo de que se deseja obter informações</param>
        /// <param name="name">Nome que o campo especificado por parâmetro possui, caso seja um campo persisitido, no SGBD</param>
        /// <param name="isIdentity">Indica se o campo especificado por parâmetro, caso seja um campo persisitido, é do tipo Identity</param>
        /// <returns>Retorna verdadeiro caso o campo indicado seja persistido, falso caso contrário</returns>
        protected void getColumnProperties(PropertyInfo property, ref string name, ref bool persist)
        {
            object[] attributes = property.GetCustomAttributes(typeof(Column), true);

            if (attributes.Length > 0)
            {
                Column column = (Column)attributes[0];
                persist = column.Persist;

                if (column.Name != null)
                    name = column.Name;
            }
        }

        /// <summary>
        /// Obtém informações de persistência de um campo, field, passado por parâmetro
        /// </summary>
        /// <param name="field">Referência do campo de que se deseja obter informações</param>
        /// <param name="name">Nome que o campo especificado por parâmetro possui, caso seja um campo persisitido, no SGBD</param>
        /// <param name="isIdentity">Indica se o campo especificado por parâmetro, caso seja um campo persisitido, é do tipo Identity</param>
        /// <param name="isPrimaryKey">Indica se o campo especificado por parâmetro, caso seja um campo persisitido, é uma chave primária</param>
        /// <returns>Retorna verdadeiro caso o campo indicado seja persistido, falso caso contrário</returns>
        protected void getColumnProperties(PropertyInfo property, ref string name, ref bool persist, ref bool primaryKey)
        {
            object[] attributes = property.GetCustomAttributes(typeof(Column), true);

            if (attributes.Length > 0)
            {
                Column column = (Column)attributes[0];

                persist = column.Persist;

                if (column.Name != null)
                    name = column.Name;
            }

            attributes = property.GetCustomAttributes(typeof(PrimaryKey), true);

            if (attributes.Length > 0)
            {
                primaryKey = true;
            }
        }

        /// <summary>
        /// Obtém informações de persistência de um campo, field, passado por parâmetro
        /// </summary>
        /// <param name="field">Referência do campo de que se deseja obter informações</param>
        /// <param name="name">Nome que o campo especificado por parâmetro possui, caso seja um campo persisitido, no SGBD</param>
        /// <returns>Retorna verdadeiro caso o campo indicado seja persistido, falso caso contrário</returns>
        protected void getColumnProperties(PropertyInfo property, ref string name)
        {
            object[] attributes = property.GetCustomAttributes(typeof(Column), true);

            if (attributes.Length > 0)
            {
                Column column = (Column)attributes[0];

                if (column.Name != null)
                    name = column.Name;
            }

            if (name == null)
                name = property.Name;
        }

        private string getPrimaryKeyName()
        {
            string name = null;

            if (PrimaryKeys.Count > 0)
                getColumnProperties((PropertyInfo)PrimaryKeys[0], ref name);
            else
                getColumnProperties((PropertyInfo)GetType().GetProperties()[0], ref name);

            return name;
        }

        public void setPrimaryKey(object key)
        {
            if (PrimaryKeys.Count > 0)
                ((PropertyInfo)PrimaryKeys[0]).SetValue(this, key, null);
            else
                ((PropertyInfo)GetType().GetProperties()[0]).SetValue(this, key, null);
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

        public string GetTable()
        {
            return Table;
        }

        public void SetDebug(bool value)
        {
            debug = value;
        }

        public bool GetDebug()
        {
            return debug;
        }

        protected bool loadFields(DataRow row, DataObject obj)
        {
            if (row != null && obj != null)
            {
                if (Conf.DataPooling)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(obj.GetType().ToString());
                    sb.Append("!");
                    sb.Append(row[getPrimaryKeyName()]);
                    DataObject result = Conf.DataPool[sb.ToString()];
                    if (result != null)
                        result.Copy(obj);
                    else
                    {
                        obj.setField(row);

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
                    obj.setField(row);

                    Persisted = true;
                    if (Conf.DataPooling)
                        Conf.DataPool.add(obj);

                    return true;
                }
            }
            else
            {
                if (debug && row == null)
                    Console.WriteLine("Loading field error: DataReader is NULL in " + GetType());
                if (debug && obj == null)
                    Console.WriteLine("Loading field error: obj is NULL in " + GetType());
                return false;
            }
        }

        protected void AddModifiers(StringBuilder sb)
        {
            string sql = sb.ToString();

            if (Where != null && !Where.Trim().Equals(""))
            {
                if (sql.ToUpper().IndexOf("WHERE") > -1)
                {
                    sb.Append(" AND ");
                    sb.Append(Where);
                }
                else
                {
                    sb.Append(" WHERE ");
                    sb.Append(Where);
                }
            }

            if (GroupBy != null && !GroupBy.Trim().Equals(""))
            {
                sb.Append(" GROUP BY ");
                sb.Append(GroupBy);
            }

            if (OrderBy != null && !OrderBy.Trim().Equals(""))
            {
                sb.Append(" ORDER BY ");
                sb.Append(OrderBy);
            }
        }

        public IList ToArray()
        {
            if (ResultSet != null && ResultSet.Rows.Count > 0)
            {
                ArrayList result = new ArrayList();

                foreach (DataRow row in ResultSet.Rows)
                {
                    DataObject obj = (DataObject)Activator.CreateInstance(GetType());

                    loadFields(row, obj);
                    result.Add(obj);
                }

                return result;
            }
            else
                throw new CsDOException("No search has been performed before !");
        }

        public IList ToArray(bool listAll)
        {
            if (listAll)
            {
                find();
            }

            return ToArray();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.GetType().ToString());
            result.Append("!");

            if (PrimaryKeys.Count > 0)
            {
                result.Append(formatValue((PropertyInfo)PrimaryKeys[0]));
            }
            else
            {
                result.Append(formatValue((PropertyInfo)GetType().GetProperties()[0]));
            }

            return result.ToString();
        }

        protected PropertyInfo findColumn(string column)
        {
            PropertyInfo[] props = GetType().GetProperties();
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

        protected string formatValue(PropertyInfo property)
        {
            string result = "";

            if (property.PropertyType.IsSubclassOf(typeof(DataObject)))
            {
                result = (property.GetValue(this, null) != null) ? ((DataObject)property.GetValue(this, null)).getPrimaryKey().ToString() : "NULL";
            }
            else
                result = (property.GetValue(this, null) != null) ? formatObject(property.GetValue(this, null)) : null;

            return result;
        }

        protected string getTableProperties()
        {
            object[] attributes = GetType().GetCustomAttributes(typeof(Table), true);

            if (attributes.Length > 0)
            {
                Table table = (Table)attributes[0];

                if (table.Name != null)
                    return table.Name;
            }

            return null;
        }

        protected bool isPrimaryKey(PropertyInfo property)
        {
            object[] attributes = property.GetCustomAttributes(typeof(PrimaryKey), true);

            return (attributes.Length > 0);
        }

        protected bool isIdentity(PropertyInfo property)
        {
            object[] attributes = property.GetCustomAttributes(typeof(Identity), true);

            return (attributes.Length > 0);
        }

        protected void getColumnProperties(PropertyInfo property, ref bool persist)
        {
            object[] attributes = property.GetCustomAttributes(typeof(Column), true);

            if (attributes.Length > 0)
            {
                Column column = (Column)attributes[0];
                persist = column.Persist;
            }
        }

        protected void setField(DataRow row)
        {
            PropertyInfo[] props = GetType().GetProperties();
            bool persist = false;
            List<PropertyInfo> keys = new List<PropertyInfo>();
            ArrayList values = new ArrayList();

            foreach (PropertyInfo propriedade in props)
            {
                string name = null;
                getColumnProperties(propriedade, ref name, ref persist);

                if (!persist)
                    continue;

                if (String.IsNullOrEmpty(name))
                    name = propriedade.Name;

                object val = row[name];

                if (propriedade != null)
                {
                    #region Property holds a Foreign Key
                    if (propriedade.PropertyType.IsSubclassOf(typeof(DataObject)) &&
                                    !val.GetType().IsSubclassOf(typeof(DataObject)))
                    {
                        if (val == null || val.ToString().Equals("0") || val.GetType() == typeof(DBNull))
                        {
                            if (debug)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=null(" + val.GetType() + ")");
                            propriedade.SetValue(this, null, null);
                            continue;
                        }

                        DataObject obj = (DataObject)propriedade.GetValue(this, null);
                        if (obj == null)
                        {
                            if (debug)
                                Console.WriteLine("Activating: " + propriedade.PropertyType);
                            if (depth > 0)
                            {
                                obj = (DataObject)Activator.CreateInstance(propriedade.PropertyType);
                                obj.depth = this.depth - 1;
                            }
                            else
                            {
                                val = assertField(null, propriedade);

                                if (debug)
                                    Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val + "(" + val.GetType() + ")");
                                propriedade.SetValue(this, val, null);
                                continue;
                            }

                            if (obj.PrimaryKeys.Count > 0)
                            {
                                if (debug)
                                    Console.WriteLine("Set " + obj.GetType() + "[" + obj + "]." + ((PropertyInfo)obj.PrimaryKeys[0]).Name + "=" + val + "(" + val.GetType() + ")");
                                ((PropertyInfo)obj.PrimaryKeys[0]).SetValue(obj, val, null);
                            }
                        }

                        if (obj != null)
                        {
                            propriedade.SetValue(this, obj, null);
                            keys.Add(propriedade);
                            values.Add(val);
                        }
                    }
                    #endregion
                    else
                    #region Property holds data
                    {
                        val = assertField(val, propriedade);

                        if (debug)
                            if (val != null)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val + "(" + val.GetType() + ")");
                            else
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val);

                        propriedade.SetValue(this, val, null);
                    }
                    #endregion
                }
            }

            //dr.Close();

            #region Retrieve Foreign Keys

            for (int i = 0; i < keys.Count; i++)
            {
                PropertyInfo propriedade = keys[i];
                DataObject obj = (DataObject)propriedade.GetValue(this, null);
                object val = values[i];

                if (obj != null)
                {
                    if (obj.PrimaryKeys.Count > 0)
                    {
                        obj.find();
                        obj.fetch();
                        if (!obj.AssertField(((PropertyInfo)obj.PrimaryKeys[0]), val))
                        {
                            if (debug)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=null(" + val.GetType() + ")");
                            propriedade.SetValue(this, null, null);
                        }
                        else
                        {
                            if (debug)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + obj + "(" + obj.GetType() + ")");
                            propriedade.SetValue(this, obj, null);
                        }
                    }
                    else
                        throw new CsDOException("Class '" + propriedade.PropertyType + "' has no Primary Key!");
                }
            }
            #endregion
        }

        //TODO: Eliminar mais tarde		
        protected void setField(string col, object val)
        {
            PropertyInfo propriedade = findColumn(col);

            if (propriedade != null)
            {
                if (propriedade.PropertyType.IsSubclassOf(typeof(DataObject)) &&
                    !val.GetType().IsSubclassOf(typeof(DataObject)))
                {
                    if (val == null || val.ToString().Equals("0") || val.GetType() == typeof(DBNull))
                    {
                        if (debug)
                            Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=null(" + val.GetType() + ")");
                        propriedade.SetValue(this, null, null);
                        return;
                    }

                    DataObject obj = (DataObject)propriedade.GetValue(this, null);
                    if (obj == null)
                    {
                        if (debug)
                            Console.WriteLine("Activating: " + propriedade.PropertyType);
                        if (depth > 0)
                        {
                            obj = (DataObject)Activator.CreateInstance(propriedade.PropertyType);
                            obj.depth = this.depth - 1;
                        }
                        else
                        {
                            val = assertField(null, propriedade);

                            if (debug)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val + "(" + val.GetType() + ")");
                            propriedade.SetValue(this, val, null);
                            return;
                        }
                    }

                    if (obj.PrimaryKeys.Count > 0)
                    {
                        if (debug)
                            Console.WriteLine("Set " + obj.GetType() + "[" + obj + "]." + ((PropertyInfo)obj.PrimaryKeys[0]).Name + "=" + val + "(" + val.GetType() + ")");
                        ((PropertyInfo)obj.PrimaryKeys[0]).SetValue(obj, val, null);
                        obj.find();
                        if (obj.fetch())
                        {
                            if (!obj.AssertField(((PropertyInfo)obj.PrimaryKeys[0]), val))
                            {
                                if (debug)
                                    Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=null(" + val.GetType() + ")");
                                propriedade.SetValue(this, null, null);
                            }
                            else
                            {
                                if (debug)
                                    Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + obj + "(" + obj.GetType() + ")");
                                propriedade.SetValue(this, obj, null);
                            }
                        }
                        else
                        {
                            if (debug)
                                Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=null(" + val.GetType() + ")");
                            propriedade.SetValue(this, null, null);
                        }
                    }
                    else
                        throw new CsDOException("Class '" + propriedade.PropertyType + "' has no Primary Key!");
                    return;
                }
                else
                {
                    val = assertField(val, propriedade);

                    if (debug)
                        if (val != null)
                            Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val + "(" + val.GetType() + ")");
                        else
                            Console.WriteLine("Set " + GetType() + "[" + this + "]." + propriedade.Name + "=" + val);

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
            else if (val != null && (val.GetType().IsSubclassOf(typeof(DBNull))
            || val.GetType() == typeof(DBNull)))
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

        protected bool AssertField(PropertyInfo propriedade, object val)
        {
            if (propriedade != null)
            {
                object obj = propriedade.GetValue(this, null);
                if (obj != null)
                    return obj.ToString().Equals(val.ToString());
                else
                    return obj == val;
            }

            throw new CsDOException("Field not found!");
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
                    BindingFlags flags = 0;
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
        /// <summary>
        /// Neste OverLoad do método as operações são executadas na forma de Transaction SQL, ou seja, execução
        /// de mais de uma operação em bancos de dados
        /// </summary>
        /// <param name="transactionID">Identificação da transação (Obtida através do método BeginTransaction)</param>
        /// <returns>Verdadeiro caso a operação tenha sido executada corretamente, falso caso contrário</returns>
        public bool insert(int? transactionID)
        {
            DbTransaction trans = (DbTransaction)transactions[transactionID.Value];

            if (trans != null)
            {
                PropertyInfo fieldIdentity = null;

                StringBuilder sql = new StringBuilder("INSERT INTO ");
                sql.Append(this.Table);

                StringBuilder fieldnames = new StringBuilder(" (");

                StringBuilder values = new StringBuilder(" Values (");

                PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    string name = null;
                    bool isIdentity = false;
                    object data = null;
                    bool persist = false;

                    getColumnProperties(property, ref name, ref persist);

                    //If this field insn't persistent then go to the next field.
                    if (!persist)
                        continue;

                    if (name == null)
                        name = property.Name;

                    //If this field is identity then it must not be included in the
                    //insert list, and it must be updated its value after the insert.
                    if (isIdentity)
                        fieldIdentity = property;
                    else
                    {
                        data = property.GetValue(this, null);

                        if (data != null)
                        {
                            fieldnames.Append(name);
                            fieldnames.Append(",");

                            values.Append(formatObject(data));
                            values.Append(",");
                        }
                    }
                }

                fieldnames.Remove(fieldnames.Length - 1, 1);
                fieldnames.Append(")");
                values.Remove(values.Length - 1, 1);
                values.Append(")");
                sql.Append(fieldnames);
                sql.Append(values);

                int i = 0;

                if (fieldIdentity != null)
                {
                    DbParameter[] parameters = new DbParameter[] {
                                                    Conf.Driver.getParameter("identColumn", DbType.Int32,4)
                                                    };
                    parameters[0].Direction = ParameterDirection.Output;

                    sql.Append(" SELECT @identColumn =@@Identity FROM ");
                    sql.Append(Table);

                    i = DataBase.New().Exec(trans, sql.ToString(), parameters);

                    if (fieldIdentity.PropertyType == typeof(int) || fieldIdentity.PropertyType == typeof(int?))
                        fieldIdentity.SetValue(this, parameters[0].Value, null);
                }
                else
                    i = DataBase.New().Exec(trans, sql.ToString());

                return (i != 0);
            }
            else
                return false;
        }

        /// <summary>
        /// Persiste os valores contidos nos campos da instância.
        /// Caso a instância passua um campo do tipo Identity,
        /// esse é atualizado após os dados terem sido persistidos
        /// </summary>
        /// <returns>Verdadeiro caso os dados tenham sido persitidos corretamente, falso caso contrário</returns>
        public bool insert()
        {
            if (BeforeInsert != null)
                BeforeInsert(this);

            PropertyInfo propriedadeIdentity = null;
            string ident = null;

            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(Table);

            StringBuilder values = new StringBuilder(" Values (");

            PropertyInfo[] props = GetType().GetProperties();

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
                {
                    values.Append(data);
                    values.Append(",");
                }
            }

            values.Remove(values.Length - 1, 1);
            values.Append(")");
            sql.Append(" (");
            sql.Append(ActiveFields);
            sql.Append(")");
            sql.Append(values);

            int i = 0;

            if (debug)
                Console.WriteLine("Ident = " + ident);

            if (ident != null)
            {
                sql.Append(" SELECT ");
                sql.Append(ident);
                sql.Append("=@@Identity FROM ");
                sql.Append(Table);
                if (debug)
                    Console.WriteLine("Exec: \"" + sql + "\"");

                //int cod = DataBase.New().Exec(sql);
                int result = 0;
                IDataReader dr1 = DataBase.New().Query(sql.ToString());
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
                    Console.WriteLine("Exec: \"" + sql.ToString() + "\"");
                i = DataBase.New().Exec(sql.ToString());
                if (Conf.DataPooling)
                    Conf.DataPool.add(this);
            }

            if (debug)
                Console.WriteLine("Affected " + i + " rows");

            Persisted = (i == 1);

            if (AfterInsert != null)
                AfterInsert(this, Persisted);

            return (Persisted);
        }

        /// <summary>
        /// Neste OverLoad do método as operações são executadas na forma de Transaction SQL, ou seja, execução
        /// de mais de uma operação em bancos de dados
        /// </summary>
        /// <param name="transactionID">Identificação da transação (Obtida através do método BeginTransaction)</param>
        /// <returns>Verdadeiro caso a operação tenha sido executada corretamente, falso caso contrário</returns>
        public bool update(int? transactionID)
        {
            DbTransaction trans = (DbTransaction)transactions[transactionID.Value];

            if (trans != null)
            {
                StringBuilder sql = new StringBuilder("UPDATE ");
                StringBuilder clausule = new StringBuilder();
                StringBuilder values = new StringBuilder();

                object search = "";
                string operador = "";

                bool hasWhere = false;

                PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    bool primaryKey = false;
                    bool isIdentity = false;
                    string name = null;
                    bool persist = false;

                    getColumnProperties(property, ref name, ref persist, ref primaryKey);

                    //If this field insn't persistent then go to the next field.
                    if (!persist)
                        continue;

                    if (name == null)
                        name = property.Name;

                    operador = " = ";

                    if (property.PropertyType == typeof(System.String))
                    {
                        operador = " LIKE '%' + ";
                    }

                    search = property.GetValue(this, null);

                    //TODO: Identificar o que foi alterado
                    if (primaryKey && (search != null))
                    {
                        clausule.Append("(");
                        clausule.Append(name);
                        clausule.Append(operador);
                        clausule.Append(formatObject(search));
                        if (property.PropertyType == typeof(System.String))
                            clausule.Append(" + '%'");
                        clausule.Append(")");
                        clausule.Append(" AND ");

                        hasWhere = true;
                    }
                    else if (!isIdentity)
                    {
                        values.Append(name);
                        values.Append("=");
                        values.Append(formatObject(search));
                        values.Append(",");
                    }
                }

                if (String.IsNullOrEmpty(values.ToString()))
                    return false;

                values.Remove(values.Length - 1, 1);

                bool result = false;

                if (hasWhere)
                {
                    clausule.Remove(clausule.Length - 4, 4);
                    sql.Append(Table);
                    sql.Append(" SET ");
                    sql.Append(values);
                    sql.Append(" WHERE ");
                    sql.Append(clausule);

                    int i = DataBase.New().Exec(trans, sql.ToString());
                    result = (i == 1);
                }

                return result;
            }
            else
                return false;
        }

        /// <summary>
        /// Atualiza, no SGBD, valores contidos nos campos da instância.
        /// </summary>
        /// <returns>Verdadeiro caso os dados tenham sido atualizados com corretamente, falso caso contrário</returns>
        public bool update()
        {
            if (BeforeUpdate != null)
                BeforeUpdate(this);

            StringBuilder sql = new StringBuilder("UPDATE ");
            StringBuilder clausule = new StringBuilder();
            string search = "";
            string operador = "";
            StringBuilder values = new StringBuilder();
            bool hasWhere = false;

            PropertyInfo[] props = GetType().GetProperties();

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
                    StringBuilder item = new StringBuilder();

                    item.Append("(");
                    item.Append(name);
                    item.Append(operador);
                    item.Append(search);
                    item.Append(")");
                    hasWhere = true;

                    clausule.Append(item);
                    clausule.Append(" AND ");
                }
                else if ((search != null) && !search.Equals("0")
                    && !search.Equals("'00010101 00:00:00'")
                    && !search.Equals("NULL"))
                {
                    values.Append(name);
                    values.Append("=");
                    values.Append(search);
                    values.Append(",");
                }
            }

            if (!values.Equals(""))
                values.Remove(values.Length - 1, 1);

            if (hasWhere)
            {
                clausule.Remove(clausule.Length - 4, 4);
                sql.Append(Table);
                sql.Append(" SET ");
                sql.Append(values);
                sql.Append(" WHERE ");
                sql.Append(clausule);
            }

            bool result = false;

            if (hasWhere)
            {
                if (debug)
                    Console.WriteLine("Exec: \"" + sql + "\"");
                int i = DataBase.New().Exec(sql.ToString());
                if (debug)
                    Console.WriteLine("Affected " + i + " rows");
                if (Conf.DataPooling)
                {
                    Conf.DataPool.remove(this);
                    Conf.DataPool.add(this);
                }
                Persisted = (i == 1);
                result = (i == 1);
            }
            else
                result = false;

            if (AfterUpdate != null)
                AfterUpdate(this, result);

            return result;
        }

        /// <summary>
        /// Neste OverLoad do método as operações são executadas na forma de Transaction SQL, ou seja, execução
        /// de mais de uma operação em bancos de dados.
        /// É permitida a exlusão de múltiplos registros caso o valor da chave primária na instância seja nulo.
        /// </summary>
        /// <param name="transactionID">Identificação da transação (Obtida através do método BeginTransaction)</param>
        /// <returns>Verdadeiro caso a operação tenha sido executada corretamente, falso caso contrário</returns>
        public bool delete(int? transactionID)
        {
            StringBuilder sql = new StringBuilder("DELETE ");
            StringBuilder clausule = new StringBuilder();
            object search = null;
            string operador = "";
            bool hasWhere = false;

            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                bool primaryKey = false;
                bool persist = false;
                string name = null;

                getColumnProperties(property, ref name, ref persist, ref primaryKey);

                if (!persist)
                    continue;

                if (name == null)
                    name = property.Name;

                operador = " = ";

                if (property.PropertyType == typeof(System.String))
                {
                    operador = " LIKE '%' + ";
                }

                search = property.GetValue(this, null);

                if (search != null)
                {
                    clausule.Append("(");
                    clausule.Append(name);
                    clausule.Append(operador);
                    clausule.Append(formatObject(search));
                    if (property.PropertyType == typeof(System.String))
                        clausule.Append(" + '%'");
                    clausule.Append(")");
                    clausule.Append(" AND ");
                    hasWhere = true;
                }

                if (primaryKey && (search != null))
                    break;
            }

            bool result = false;

            if (hasWhere)
            {
                clausule.Remove(clausule.Length - 4, 4);
                sql.Append("FROM ");
                sql.Append(Table);
                sql.Append(" WHERE ");
                sql.Append(clausule);

                try
                {
                    int i = DataBase.New().Exec(sql.ToString());
                    result = (i > 0);
                }
                catch (Exception e)
                {
                    throw (e);
                }
            }

            return result;
        }

        /// <summary>
        /// Remove, no SGBD, o registro referente à intância.
        /// Permite a exlusão de múltiplos registros caso o valor da chave primária na instância seja nulo.
        /// </summary>
        /// <returns>Verdadeiro caso a operação tenha sido executada corretamente, falso caso contrário</returns>
        public bool delete()
        {
            if (BeforeDelete != null)
                BeforeDelete(this);

            StringBuilder sql = new StringBuilder("DELETE ");
            StringBuilder clausule = new StringBuilder();
            string search = "";
            string operador = "";
            bool hasWhere = false;

            PropertyInfo[] props = GetType().GetProperties();

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
                    StringBuilder item = new StringBuilder();

                    item.Append("(");
                    item.Append(name);
                    item.Append(operador);
                    item.Append(search);
                    item.Append(")");
                    hasWhere = true;

                    clausule.Append(item);
                    clausule.Append(" AND ");
                }

                if (primaryKey)
                    break;
            }

            if (hasWhere)
            {
                clausule.Remove(clausule.Length - 4, 4);
                sql.Append("FROM ");
                sql.Append(Table);
                sql.Append(" WHERE ");
                sql.Append(clausule);
            }

            bool result = false;

            if (hasWhere)
            {
                try
                {
                    if (debug)
                        Console.WriteLine("Exec: \"" + sql + "\"");
                    int i = DataBase.New().Exec(sql.ToString());
                    if (debug)
                        Console.WriteLine("Affected " + i + " rows");
                    if (Conf.DataPooling)
                        Conf.DataPool.remove(this);
                    result = (i == 1);
                }
                catch (Exception e)
                {
                    if (debug)
                        Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    throw (e);
                }
            }
            else
                result = false;

            if (AfterDelete != null)
                AfterDelete(this, result);

            return result;
        }

        public bool deleteCascade()
        {
            bool result = true;

            if (Persisted)
                result &= delete();

            if (ForeignKeys.Count > 0)
                foreach (PropertyInfo foreignKey in ForeignKeys)
                {
                    DataObject obj = (DataObject)foreignKey.GetValue(this, null);
                    if (obj != null && obj.Persisted)
                    {
                        if (debug)
                            Console.WriteLine("**** Deleting " + foreignKey.Name + " ...");
                        result &= obj.deleteCascade();
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
        #endregion

        #region Query
        /// <summary>
        /// Realiza uma busca, na tabela do SGBD referente à classe da instância,
        /// passando como condições de busca os valores, não nulos, dos campos da instância
        /// </summary>
        /// <returns>Verdadeiro caso tenha sido retornado algum registro, falso caso contrário</returns>
        public bool find()
        {
            StringBuilder sql = new StringBuilder("SELECT ");
            StringBuilder clausule = new StringBuilder();
            string search = "";
            string operador = "";
            bool hasWhere = false;

            PropertyInfo[] props = GetType().GetProperties();

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

                if (propriedade.PropertyType == typeof(bool) ||
                    propriedade.PropertyType == typeof(System.Boolean))
                    continue;

                if (propriedade.PropertyType == typeof(bool))
                    continue;

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
                    StringBuilder item = new StringBuilder();
                    item.Append("(");
                    item.Append(name);
                    item.Append(operador);
                    item.Append(search);
                    item.Append(")");
                    hasWhere = true;

                    clausule.Append(item);
                    clausule.Append(" AND ");
                }

                if (primaryKey && (search != null) && !search.Equals("0")
                    && !search.Equals("00010101 00:00:00"))
                    break;
            }

            if (hasWhere)
            {
                clausule.Remove(clausule.Length - 4, 4);

                if (Limit != null && !Limit.Trim().Equals(""))
                {
                    sql.Append("TOP ");
                    sql.Append(Limit);
                    sql.Append(" ");
                }

                sql.Append(Fields);
                sql.Append(" FROM ");
                sql.Append(Table);
                sql.Append(" WHERE ");
                sql.Append(clausule);
            }
            else
            {
                if (Limit != null && !Limit.Trim().Equals(""))
                {
                    sql.Append("TOP ");
                    sql.Append(Limit);
                    sql.Append(" ");
                }

                sql.Append(Fields);
                sql.Append(" FROM ");
                sql.Append(Table);
            }

            AddModifiers(sql);

            try
            {
                if (debug)
                    Console.WriteLine("Query: \"" + sql + "\"");
                ResultSet = DataBase.New().QueryDT(CommandType.Text, sql.ToString(), new DbParameter[] { });
            }
            catch (Exception e)
            {
                if (debug)
                    Console.WriteLine("Exception: \"" + e.Message + "\"");
                throw (e);
            }

            return true;
        }

        /// <summary>
        /// Repassa os dados da consulta à intância
        /// </summary>
        /// <returns>Valor Booleano indicando verdadeiro caso tenha sido repassado o valor para o objeto, falso caso contrário</returns>
        public bool fetch()
        {
            bool result = false;

            if (ResultSet != null && ResultSet.Rows.Count > 0)
            {
                DataRow row = ResultSet.Rows[rowsCounter++];

                if (rowsCounter > ResultSet.Rows.Count)
                {
                    rowsCounter = ResultSet.Rows.Count;
                }
                else
                {
                    if (BeforeFetch != null)
                        BeforeFetch(this);

                    result = loadFields(row, this);

                    if (AfterFetch != null)
                        AfterFetch(this, result);
                }
            }

            return result;
        }

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
        public void Dispose()
        {
            if (state == DisposableState.Alive)
            {
                InternalDispose(true);
                GC.SuppressFinalize(this);
            }
        }
        protected void Dispose(bool disposing)
        {
            if (_foreignKeys != null)
                foreach (PropertyInfo foreign in _foreignKeys)
                {
                    if (foreign.PropertyType.IsSubclassOf(typeof(DataObject)))
                    {
                        DataObject obj = (DataObject)foreign.GetValue(this, new object[] { });
                        if (obj != null)
                            obj.Dispose();
                    }
                }
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

        #region Events
        //  protected event Identity identity = null; //for SQL SERVER
        protected event AutoIncrement autoIncrement = null;

        public event OnAfterManipulation AfterDelete = null;
        public event OnBeforeManipulation BeforeDelete = null;

        public event OnAfterManipulation AfterInsert = null;
        public event OnBeforeManipulation BeforeInsert = null;

        public event OnAfterManipulation AfterUpdate = null;
        public event OnBeforeManipulation BeforeUpdate = null;

        public event OnAfterManipulation AfterFetch = null;
        public event OnBeforeManipulation BeforeFetch = null;
        #endregion

        #region Transaction
        /// <summary>
        /// Inicia uma transação com o SGBD.
        /// Deve-se ser utilizado em conjunto com os métodos 
        /// </summary>
        /// <returns>Identificador para a transação iniciada</returns>
        public static int? BeginTransaction()
        {
            DbConnection conn = Conf.Driver.getConnection();
            conn.Open();
            DbTransaction trans = conn.BeginTransaction();
            if (!DataObject.transactions.ContainsKey(trans.GetHashCode()))
            {
                DataObject.transactions.Add(trans.GetHashCode(), trans);
                return trans.GetHashCode();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Salva as mudanças efetuadas pela transação indicada por parâmetro
        /// </summary>
        /// <param name="transactionID">Identificação da transação que se deseja salvar</param>
        public static void CommitTransaction(int? transactionID)
        {
            DbTransaction trans = (DbTransaction)transactions[transactionID.Value];

            if (trans != null)
            {
                DataObject.transactions.Remove(transactionID);
                trans.Commit();
                trans.Dispose();
            }
        }

        /// <summary>
        /// Cancela as mudanças efetuadas pela transação indicada por parâmetro
        /// </summary>
        /// <param name="transactionID">Identificação da transação que se deseja cancelar</param>
        public static void RollBackTransaction(int? transactionID)
        {
            DbTransaction trans = (DbTransaction)transactions[transactionID.Value];

            if (trans != null)
            {
                DataObject.transactions.Remove(transactionID);
                trans.Rollback();
                trans.Dispose();
            }
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
