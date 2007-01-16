using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CsDO.CodeGenerator.Properties;
using CsDO.Lib;
using System.Data.Common;
using Npgsql;

namespace CsDO.CodeGenerator
{
    public class SchemaReader
    {

        public static List<string> ReadSchema(IDataBase driver)
        {
            List<string> tables = new List<string>();
            using (IDbConnection connection = driver.getConnection())
            {
                if (connection.State == ConnectionState.Broken ||
                    connection.State == ConnectionState.Closed)
                    connection.Open();
                DataTable tableList = driver.getSchema("Tables", new string[] { });
                tableList.DefaultView.RowFilter = "(table_schema <> 'information_schema') AND (table_schema <> 'pg_catalog')";
                foreach (DataRow row in tableList.DefaultView.ToTable().Rows)
                    tables.Add(row["TABLE_NAME"].ToString());
            }

            return tables;
        }

        public static List<FieldDefinition>
          ReadSchema(IDataBase driver, string tableName)
        {
            List<FieldDefinition> fields = new List<FieldDefinition>();
            IDbConnection connection = driver.getConnection();
            try
            {
                DataTable table = new DataTable();
                if (connection.GetType() == typeof(NpgsqlConnection))
                {
                    NpgsqlCommand command = driver.getCommand("SELECT * FROM \"" + tableName + "\"") as NpgsqlCommand; 
                    NpgsqlDataAdapter adapter = driver.getDataAdapter(command) as NpgsqlDataAdapter;
                    adapter.FillSchema(table, SchemaType.Source);
                }
                else
                {
                    IDbCommand command = driver.getCommand("SELECT * FROM \"" + tableName + "\"");
                    DbDataAdapter adapter = driver.getDataAdapter(command) as DbDataAdapter;
                    adapter.FillSchema(table, SchemaType.Source);
                }

                foreach (DataColumn c in table.Columns)
                {
                    fields.Add(new FieldDefinition(c.ColumnName.Replace(";", ""), c.DataType));
                }

                foreach (DataColumn pk in table.PrimaryKey)
                {
                    foreach (FieldDefinition field in fields)
                        if (field.FieldName.Equals("_" + pk.ColumnName.Replace(";","")))
                            field.PrimaryKey = true;
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return fields;
        }
    }
}
