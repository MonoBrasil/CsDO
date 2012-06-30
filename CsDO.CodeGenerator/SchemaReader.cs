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

        public static List<ClassDefinition> ReadSchema(IDataBase driver)
        {
            List<ClassDefinition> tables = new List<ClassDefinition>();
            using (IDbConnection connection = driver.getConnection())
            {
                if (connection.State == ConnectionState.Broken ||
                    connection.State == ConnectionState.Closed)
                    connection.Open();
                DataTable tableList = driver.getSchema("Tables", new string[] { });
                tableList.DefaultView.RowFilter = "(table_schema <> 'information_schema') AND (table_schema <> 'pg_catalog')";
                foreach (DataRow row in tableList.DefaultView.ToTable().Rows)
                    tables.Add(new ClassDefinition(row["TABLE_NAME"].ToString()));
            }

            return tables;
        }

        public static void ReadSchema(IDataBase driver, ClassDefinition table)
        {
            IDbConnection connection = driver.getConnection();
            try
            {
                DataTable schema = new DataTable();
                if (connection.GetType() == typeof(NpgsqlConnection))
                {
                    NpgsqlCommand command = driver.getCommand("SELECT * FROM \"" + table.Table + "\"") as NpgsqlCommand; 
                    NpgsqlDataAdapter adapter = driver.getDataAdapter(command) as NpgsqlDataAdapter;
                    adapter.FillSchema(schema, SchemaType.Source);
                }
                else
                {
                    IDbCommand command = driver.getCommand("SELECT * FROM \"" + table.Table + "\"");
                    DbDataAdapter adapter = driver.getDataAdapter(command) as DbDataAdapter;
                    adapter.FillSchema(schema, SchemaType.Source);
                }

                foreach (DataColumn c in schema.Columns)
                {
                    table.Columns.Add(new FieldDefinition(c.ColumnName, c.DataType));
                }

                foreach (DataColumn pk in schema.PrimaryKey)
                {
                    foreach (FieldDefinition field in table.Columns)
                        if (field.ColumnName.Equals(pk.ColumnName))
                            field.PrimaryKey = true;
                }

                if (connection.GetType() == typeof(NpgsqlConnection))
                {
                    NpgsqlCommand command = driver.getCommand(@"SELECT
tc.constraint_schema, tc.constraint_name,
kcu.table_schema, kcu.table_name, kcu.column_name,
ctu.table_schema as dest_table_schema,
ctu.table_name as dest_table_name,
ccu.column_name as dest_column_name
FROM information_schema.table_constraints tc
INNER JOIN information_schema.constraint_table_usage ctu ON ctu.constraint_name = tc.constraint_name
INNER JOIN information_schema.constraint_column_usage ccu ON ccu.constraint_name = tc.constraint_name
INNER JOIN information_schema.key_column_usage kcu ON kcu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_name = '" + table + "';") as NpgsqlCommand; 
                    DbDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        foreach (FieldDefinition field in table.Columns)
                            if (field.ColumnName.Equals(dr["column_name"].ToString()))
                            {
                                field.ForeignKey = true;
                                field.ForeignKeyType = dr["dest_table_name"].ToString();
                            }
                    }
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}
