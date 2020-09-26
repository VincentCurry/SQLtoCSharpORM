using DataServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace CodeGenerator
{
    /*select * from sys.tables

select * from sys.objects

select * from INFORMATION_SCHEMA.COLUMNS

select * from sys.columns*/
    public class SQLTable
    {
        public string Name { get; set; }
        public int id{get;set;}
        public List<SQLTableColumn> Columns { get; set; }

        public SQLTableColumn PrimaryKey
        {
            get { return this.Columns[0]; }
        }

        public static List<SQLTable> LoadTables(string connectionString)
        {
            List<SQLTable> tables = new List<SQLTable>();
            SqlDataReader dataReader;

            dataReader= SQLDataServer.ExecuteSQLStringReturnDataReader("select name, object_id from sys.tables where name <> 'sysdiagrams' and name not like 'aspnet_%'", connectionString);
            
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

    public class SQLTableColumn
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public int OrdinalPosition { get; set; }
        public bool Nullable { get; set; }
        //public System.Data.SqlDbType DataType { get; set; }
        public string DataType{get;set;}
        public SQLTable ParentTable { get; set; }
        public List<SQLTable> DatabaseTables { get; set; }
        public List<SQLForeignKeyRelation> ForeignKeys { get; set; }
        public bool PrimaryKey { get; set; }

        private int maximumLength;
        public int MaximumLength
        {
            get
            {
                switch (DataType)
                {
                    case SQLDataTypes.varChar:
                        return maximumLength;
                    case SQLDataTypes.dateTime:
                        return 8;
                    case SQLDataTypes.intData:
                        return 4;
                    case SQLDataTypes.uniqueIdentifier:
                        return 16;
                    default:
                        return 0;
                }
            }
            set { maximumLength = value; }
        }

        public string RandomValue()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());

            switch (DataType)
            {
                case SQLDataTypes.intData:
                    byte[] buf = new byte[8];
                    random.NextBytes(buf);
                    long longRand = BitConverter.ToInt32(buf, 0);
                    return Math.Abs(longRand % long.MaxValue).ToString();
                case SQLDataTypes.varChar:
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    return "\"" + new string(Enumerable.Repeat(chars, MaximumLength)
                      .Select(s => s[random.Next(s.Length)]).ToArray()) + "\"";
                case SQLDataTypes.uniqueIdentifier:
                    return new Guid().ToString();
                case SQLDataTypes.bit:
                    return false.ToString();
                case SQLDataTypes.dateTime:
                    return DateTime.Now.ToString();
                case SQLDataTypes.varBinary:
                    return "binary";
                case SQLDataTypes.decimalData:
                    return "3.1415";
                case SQLDataTypes.binary:
                    return "binary";
                case SQLDataTypes.floatData:
                    return "float";
                case SQLDataTypes.ncharData:
                    return "string";
                case SQLDataTypes.charType:
                    return "string";
                case SQLDataTypes.timeType:
                    return "TimeSpan";
                default:
                    return "random";
            }

        }

        public static List<SQLTableColumn> LoadColumnsForTable(string tableName, string connectionString, SQLTable parentTable, ref List<SQLTable> databaseTables)
        {
            List<SQLTableColumn> columns = new List<SQLTableColumn>();

            string selectStatement;

            selectStatement = "select COLUMN_NAME, TABLE_NAME, ORDINAL_POSITION, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, DATA_TYPE from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
            
            SqlDataReader dataReader = SQLDataServer.ExecuteSQLStringReturnDataReader(selectStatement, connectionString);

            while (dataReader.Read())
            {
                SQLTableColumn column = new SQLTableColumn();

                column.Name = Convert.ToString(dataReader["COLUMN_NAME"]);
                column.TableName = Convert.ToString(dataReader["TABLE_NAME"]);
                column.OrdinalPosition = Convert.ToInt32(dataReader["ORDINAL_POSITION"]);
                column.Nullable = DBBooleanValues.ReturnBooleanFromYesOrNo(dataReader["IS_NULLABLE"]);
                column.DataType = Convert.ToString(dataReader["DATA_TYPE"]);
                column.MaximumLength = DBNullReturnValues.Return0(dataReader["CHARACTER_MAXIMUM_LENGTH"]);

                column.ParentTable = parentTable;

                if (column.OrdinalPosition == 1)
                    column.PrimaryKey = true;

                column.ForeignKeys = SQLForeignKeyRelation.LoadForeignKeysForColumn(column.ParentTable.id, column.OrdinalPosition, connectionString, ref databaseTables);

                columns.Add(column);
            }

            dataReader.Close();

            return columns;
        }

        /// <summary>
        /// Returns the datatype for the column for use in defining variables in C#
        /// </summary>
        public string cSharpDataType
        {
            get
            {
                switch (DataType)
                {
                    case SQLDataTypes.intData:
                        return "long";
                    case SQLDataTypes.varChar:
                        return "string";
                    case SQLDataTypes.uniqueIdentifier:
                        return "Guid";
                    case SQLDataTypes.bit:
                        return "bool";
                    case SQLDataTypes.dateTime:
                        return "DateTime";
                    case SQLDataTypes.varBinary:
                        return "binary";
                    case SQLDataTypes.decimalData:
                        return "decimal";
                    case SQLDataTypes.binary:
                        return "binary";
                    case SQLDataTypes.floatData:
                        return "float";
                    case SQLDataTypes.ncharData:
                        return "string";
                    case SQLDataTypes.charType:
                        return "string";
                    case SQLDataTypes.timeType:
                        return "TimeSpan";
                    default:
                        throw new SQLDBTypeNotSupported(DataType);
                }
            }
        }

        public string dotNetSqlDataTypes
        {
            get
            {
                switch (DataType)
                {
                    case SQLDataTypes.intData:
                        return "Int";
                    case SQLDataTypes.varChar:
                        return "NVarChar";
                    case SQLDataTypes.uniqueIdentifier:
                        return "UniqueIdentifier";
                    case SQLDataTypes.bit:
                        return "Bit";
                    case SQLDataTypes.dateTime:
                        return "DateTime";
                    case SQLDataTypes.varBinary:
                        return "VarBinary";
                    case SQLDataTypes.decimalData:
                        return "Decimal";
                    case SQLDataTypes.binary:
                        return "Binary";
                    case SQLDataTypes.floatData:
                        return "Float";
                    case SQLDataTypes.ncharData:
                        return "NChar";
                    case SQLDataTypes.charType:
                        return "Char";
                    case SQLDataTypes.timeType:
                        return "Time";
                    default:
                        throw new SQLDBTypeNotSupported(DataType);
                }
            }
        }
    }

    public class SQLForeignKeyRelation
    {
        public SQLTableColumn ParentTableColum
        {
            get { return IdentifyColumn(parentObjectID, parentColumnID); }
        }
        public SQLTableColumn ReferencedTableColumn { get { return IdentifyColumn(referencedObjectID, referencedColumnID); } }
        private int parentObjectID;
        private int parentColumnID;
        private int referencedObjectID;
        private int referencedColumnID;
        public List<SQLTable> DatabaseTables;

        private SQLTableColumn IdentifyColumn(int tableID, int columnID)
        {
            foreach (SQLTable table in DatabaseTables)
            {
                if (table.id == tableID)
                {
                    foreach (SQLTableColumn column in table.Columns)
                        if (column.OrdinalPosition == columnID)
                            return column;
                }
            }
            return null;
        }

        public static List<SQLForeignKeyRelation> LoadForeignKeysForColumn(int tableID, int columnID, string connectionString, ref List<SQLTable> databaseTables)
        {

            List<SQLForeignKeyRelation> foreignKeys = new List<SQLForeignKeyRelation>();

            string selectStatement = "Select constraint_object_id, constraint_column_id, parent_object_id, parent_column_id, referenced_object_id, referenced_column_id from sys.foreign_key_columns where referenced_object_id = " + tableID + " and referenced_column_id = " + columnID;

            SqlDataReader dataReader = SQLDataServer.ExecuteSQLStringReturnDataReader(selectStatement, connectionString);

            while (dataReader.Read())
            {
                SQLForeignKeyRelation foreignKey = new SQLForeignKeyRelation();

                foreignKey.DatabaseTables = databaseTables;

                foreignKey.parentObjectID = Convert.ToInt32(dataReader["parent_object_id"]);
                foreignKey.parentColumnID = Convert.ToInt32(dataReader["parent_column_id"]);
                foreignKey.referencedObjectID = Convert.ToInt32(dataReader["referenced_object_id"]);
                foreignKey.referencedColumnID = Convert.ToInt32(dataReader["referenced_column_id"]);

                foreignKeys.Add(foreignKey);
            }

            dataReader.Close();

            return foreignKeys;
        }
    }
        /*SELECT f.name AS ForeignKey, OBJECT_NAME(f.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferenceColumnName
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc
ON f.OBJECT_ID = fc.constraint_object_id

select * from sys.foreign_keys
select * from sys.foreign_key_columns
         
         select COLUMN_NAME, TABLE_NAME, ORDINAL_POSITION, IS_NULLABLE, DATA_TYPE* from INFORMATION_SCHEMA.COLUMNS
select * from sys.columns
select * from sys.foreign_keys
select * from sys.foreign_key_columns*/
}
