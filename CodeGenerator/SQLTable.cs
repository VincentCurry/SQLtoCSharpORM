using DataServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CodeGenerator
{
    public class SQLTable
    {
        public string Name { get; set; }
        public int id { get; set; }
        public List<SQLTableColumn> Columns { get; set; }

        public SQLTableColumn PrimaryKey
        {
            get { return this.Columns[0]; }
        }

        public static List<SQLTable> LoadTables(string connectionString)
        {
            List<SQLTable> tables = new List<SQLTable>();
            SqlDataReader dataReader;

            dataReader = SQLDataServer.ExecuteSQLStringReturnDataReader("select name, object_id from sys.tables where name <> 'sysdiagrams' and name not like 'aspnet_%'", connectionString);

            while (dataReader.Read())
            {
                SQLTable table = new SQLTable();

                table.Name = Convert.ToString(dataReader["name"]);
                table.id = Convert.ToInt32(dataReader["object_id"]);

                table.Columns = SQLTableColumn.LoadColumnsForTable(table.Name, connectionString, table, ref tables);

                tables.Add(table);
            }

            dataReader.Close();

            return tables;
        }
    }
}
