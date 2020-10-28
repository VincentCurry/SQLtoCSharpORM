using DataServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace CodeGenerator
{
    public class SQLTableColumn
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public int OrdinalPosition { get; set; }
        public bool Nullable { get; set; }
        public string DataType { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public int DateTimePrecision { get; set; }
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
                        return DateTimePrecision;
                    case SQLDataTypes.intData:
                        return 4;
                    case SQLDataTypes.uniqueIdentifier:
                        return 16;
                    case SQLDataTypes.decimalData:
                        return NumericPrecision;
                    case SQLDataTypes.moneyType:
                        return NumericPrecision;
                    default:
                        return 0;
                }
            }
            set { maximumLength = value; }
        }

        public string SizeForSQLProcedureParameters
        {
            get
            {
                if (DataType == SQLDataTypes.varChar)
                    return (" (" + MaximumLength.ToString() + ")");

                if (DataType == SQLDataTypes.decimalData)
                    return($" ({NumericPrecision},{NumericScale})");

                return "";
            }
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
                    return $"new DateTime({DateTime.Now.Year}, {DateTime.Now.Month}, {DateTime.Now.Day}, {DateTime.Now.Hour}, {DateTime.Now.Minute}, {DateTime.Now.Second})";
                case SQLDataTypes.varBinary:
                    return "binary";
                case SQLDataTypes.decimalData:
                    return "3.1415M";
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
                case SQLDataTypes.moneyType:
                    return "34.50M";
                default:
                    return "random";
            }

        }

        public static List<SQLTableColumn> LoadColumnsForTable(string tableName, string connectionString, SQLTable parentTable, ref List<SQLTable> databaseTables)
        {
            List<SQLTableColumn> columns = new List<SQLTableColumn>();

            string selectStatement;

            selectStatement = "select COLUMN_NAME, TABLE_NAME, ORDINAL_POSITION, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE, DATETIME_PRECISION from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";

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
                column.NumericPrecision = DBNullReturnValues.Return0(dataReader["NUMERIC_PRECISION"]);
                column.NumericScale = DBNullReturnValues.Return0(dataReader["NUMERIC_SCALE"]);
                column.DateTimePrecision = DBNullReturnValues.Return0(dataReader["DATETIME_PRECISION"]);

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
                    case SQLDataTypes.moneyType:
                        return "decimal";
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
                    case SQLDataTypes.moneyType:
                        return "Money";
                    default:
                        throw new SQLDBTypeNotSupported(DataType);
                }
            }
        }
    }
}
