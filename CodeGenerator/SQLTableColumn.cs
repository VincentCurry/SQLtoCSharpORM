using DataServer;
using System;
using System.Collections.Generic;
using System.Data;
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
                    case SQLDataTypes.charType:
                        return maximumLength;
                    case SQLDataTypes.ncharData:
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
                    case SQLDataTypes.floatData:
                        return 8;
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
                if (DataType == SQLDataTypes.ncharData)
                    return (" (" + MaximumLength.ToString() + ")");

                if (DataType == SQLDataTypes.varChar)
                    return (" (" + MaximumLength.ToString() + ")");

                if (DataType == SQLDataTypes.charType)
                    return (" (" + MaximumLength.ToString() + ")");

                if (DataType == SQLDataTypes.decimalData)
                    return($" ({NumericPrecision},{NumericScale})");

                return "";
            }
        }

        public string RandomValue()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            switch (DataType)
            {
                case SQLDataTypes.intData:
                    byte[] buf = new byte[8];
                    random.NextBytes(buf);
                    long longRand = BitConverter.ToInt32(buf, 0);
                    return Math.Abs(longRand % long.MaxValue).ToString();
                case SQLDataTypes.varChar:
                    return "\"" + new string(Enumerable.Repeat(chars, (MaximumLength == -1 ? 50 : MaximumLength))
                      .Select(s => s[random.Next(s.Length)]).ToArray()) + "\"";
                case SQLDataTypes.uniqueIdentifier:
                    return new Guid().ToString();
                case SQLDataTypes.bit:
                    return "false";
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
                    return "\"" + new string(Enumerable.Repeat(chars, MaximumLength)
                      .Select(s => s[random.Next(s.Length)]).ToArray()) + "\"";
                case SQLDataTypes.charType:
                    return "\"" + new string(Enumerable.Repeat(chars, MaximumLength)
                      .Select(s => s[random.Next(s.Length)]).ToArray()) + "\"";
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
                        return "double";
                    case SQLDataTypes.ncharData:
                        return "string";
                    case SQLDataTypes.charType:
                        return "string";
                    case SQLDataTypes.timeType:
                        return "TimeSpan";
                    case SQLDataTypes.moneyType:
                        return "decimal";
                    case SQLDataTypes.bigInt:
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
                    case SQLDataTypes.moneyType:
                        return "Money";
                    case SQLDataTypes.bigInt:
                        return "BigInt";
                    default:
                        throw new SQLDBTypeNotSupported(DataType);
                }
            }
        }

        public string htmlInputFormType
        {
            get
            {
                return DataType switch
                {
                    SQLDataTypes.intData => htmlFormValueType.number,
                    SQLDataTypes.varChar => htmlFormValueType.text,
                    SQLDataTypes.uniqueIdentifier => htmlFormValueType.text,
                    SQLDataTypes.bit => htmlFormValueType.checkbox,
                    SQLDataTypes.dateTime => htmlFormValueType.datetimeLocal,
                    SQLDataTypes.varBinary => htmlFormValueType.text,
                    SQLDataTypes.decimalData => htmlFormValueType.number,
                    SQLDataTypes.binary => htmlFormValueType.number,
                    SQLDataTypes.floatData => htmlFormValueType.number,
                    SQLDataTypes.ncharData => htmlFormValueType.text,
                    SQLDataTypes.charType => htmlFormValueType.text,
                    SQLDataTypes.timeType => htmlFormValueType.number,
                    SQLDataTypes.moneyType => htmlFormValueType.number,
                    SQLDataTypes.bigInt => htmlFormValueType.number,
                    _ => throw new SQLDBTypeNotSupported(DataType)
                } ;
            }
        }

        public string kotlinDataType
        {
            get
            {
                return DataType switch
                {
                    SQLDataTypes.intData => kotlinDataTypes.integer,
                    SQLDataTypes.varChar => kotlinDataTypes.strings,
                    SQLDataTypes.uniqueIdentifier => kotlinDataTypes.strings,
                    SQLDataTypes.bit => kotlinDataTypes.boolean,
                    SQLDataTypes.dateTime => kotlinDataTypes.date,
                    SQLDataTypes.varBinary => kotlinDataTypes.strings,
                    SQLDataTypes.decimalData => kotlinDataTypes.doubleNum,
                    SQLDataTypes.binary => kotlinDataTypes.integer,
                    SQLDataTypes.floatData => kotlinDataTypes.floatNum,
                    SQLDataTypes.ncharData => kotlinDataTypes.strings,
                    SQLDataTypes.charType => kotlinDataTypes.strings,
                    SQLDataTypes.timeType => kotlinDataTypes.date,
                    SQLDataTypes.moneyType => kotlinDataTypes.floatNum,
                    SQLDataTypes.bigInt => kotlinDataTypes.integer,
                    _ => throw new SQLDBTypeNotSupported(DataType)
                };
            }
        }

        public string iosDataType
        {
            get
            {
                return DataType switch
                {
                    SQLDataTypes.intData => iosDataTypes.integer,
                    SQLDataTypes.varChar => iosDataTypes.strings,
                    SQLDataTypes.uniqueIdentifier => iosDataTypes.strings,
                    SQLDataTypes.bit => iosDataTypes.boolean,
                    SQLDataTypes.dateTime => iosDataTypes.date,
                    SQLDataTypes.varBinary => iosDataTypes.strings,
                    SQLDataTypes.decimalData => iosDataTypes.doubleNum,
                    SQLDataTypes.binary => iosDataTypes.integer,
                    SQLDataTypes.floatData => iosDataTypes.floatNum,
                    SQLDataTypes.ncharData => iosDataTypes.strings,
                    SQLDataTypes.charType => iosDataTypes.strings,
                    SQLDataTypes.timeType => iosDataTypes.date,
                    SQLDataTypes.moneyType => iosDataTypes.floatNum,
                    SQLDataTypes.bigInt => iosDataTypes.integer,
                    _ => throw new SQLDBTypeNotSupported(DataType)
                };
            }
        }

        public string sqlLiteDataType
        {
            get
            {

                string[] blobTypes = { "binary", "blob", "varbinary" };
                string[] textTypes = { "char", "charater", "clob", "national varying character", "native character", "nchar", "nvarchar", "text", "uniqueidentifier", "varchar", "variant", "varying character" };
                string[] dateTypes = { "date", "datetime", "time", "timestamp" };
                string[] intTypes = { "bigint", "bit", "bool", "boolean", "int", "int2", "int8", "integer", "mediumint", "smallint", "tinyint" };
                string[] nullTypes = { "null" };
                string[] realTypes = { "decimal", "double", "double precision", "float", "numeric", "real", "money" };

                if (blobTypes.Contains(DataType))
                    return sqlLiteStorageDataTypes.blobStore;
                if (textTypes.Contains(DataType))
                    return sqlLiteStorageDataTypes.textStore;
                if (dateTypes.Contains(DataType))
                    return sqlLiteStorageDataTypes.floatStore;
                if (intTypes.Contains(DataType))
                    return sqlLiteStorageDataTypes.intStore;
                if (realTypes.Contains(DataType))
                    return sqlLiteStorageDataTypes.floatStore;

                return sqlLiteStorageDataTypes.nullStore;

            }
        }

        public string sqlLiteBindType
        {
            get
            {
                if (DataType == SQLDataTypes.intData || DataType == SQLDataTypes.bit)
                    return "int";
                else
                    return "text";
            }
        }

    }
}
