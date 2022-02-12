using System.Collections.Generic;

namespace CodeGenerator
{
    public class IOCRepositoryGenerator : Generator
    {
        public IOCRepositoryGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            filePrefix = "RepositorySql";
        }
        
        internal override void GenerateFilePerTable(SQLTable table)
        {
            List<SQLForeignKeyRelation> foreignKeys = sQLForeignKeyRelationsForTable(table);

            classText.AppendLine("using DataServer;");
            classText.AppendLine("using System;");
            classText.AppendLine("using System.Collections.Generic;");
            classText.AppendLine("using System.Data;");
            classText.AppendLine("using System.Data.SqlClient;");
            classText.AppendLine($"using {_nameSpace}.Repository;");
            classText.AppendLine("");

            classText.AppendLine($"namespace {_nameSpace}.SqlRepository");
            classText.AppendLine("{");
            classText.AppendLine($"\tpublic partial class {table.Name}RepositorySql : IRepository<{dataObjectClassIdentifier}, {table.PrimaryKey.cSharpDataType}>");
            classText.AppendLine("\t{");
            classText.AppendLine("\t\t#region Loading Methods");
            classText.AppendLine($"\t\tpublic List<{dataObjectClassIdentifier}> GetAll()");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\treturn Populate{table.Name}List(\"{table.Name}GetAll\", new List<SqlParameter>());");
            classText.AppendLine("\t\t}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tpublic {dataObjectClassIdentifier} GetByID({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.PrimaryKey.Name)})");
            classText.AppendLine("\t\t{");
            classText.AppendLine("\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
            classText.AppendLine($"\t\t\tSQLDataServer.AddParameter(ref parameters, \"@{table.PrimaryKey.Name}\", {Library.LowerFirstCharacter(table.PrimaryKey.Name)}, SqlDbType.{table.PrimaryKey.dotNetSqlDataTypes}, {table.PrimaryKey.MaximumLength});");
            classText.AppendLine($"\t\t\treturn Individual{table.Name}(\"{table.Name}GetByID\", parameters);");
            classText.AppendLine("\t\t}");
            classText.AppendLine("");


            foreach (SQLForeignKeyRelation foreignKeyRelation in foreignKeys)
            {
                SQLTableColumn column = foreignKeyRelation.ReferencedTableColumn;

                classText.AppendLine($"\t\tpublic List<{dataObjectClassIdentifier}> GetBy{column.Name}({column.cSharpDataType} {Library.LowerFirstCharacter(column.Name)})");
                classText.AppendLine("\t\t{");
                classText.AppendLine("\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
                classText.AppendLine($"\t\t\tSQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {Library.LowerFirstCharacter(column.Name)}, SqlDbType.{column.dotNetSqlDataTypes}, {column.MaximumLength});");
                classText.AppendLine($"\t\t\treturn Populate{table.Name}List(\"{table.Name}GetBy{column.Name}\", parameters);");
                classText.AppendLine("\t\t}");
                classText.AppendLine("");
            }
            
            classText.AppendLine($"\t\tprivate List<{dataObjectClassIdentifier}> Populate{table.Name}List(string storedProcedure, List<SqlParameter> parameters)");
            classText.AppendLine("\t\t{");

            //Check for guid columns as they need to use their ordinal to be identified
            foreach (SQLTableColumn column in table.Columns)
            {
                if (column.DataType == SQLDataTypes.uniqueIdentifier || column.DataType == SQLDataTypes.timeType)
                {
                    classText.AppendLine($"\t\t\tint {Library.LowerFirstCharacter(column.Name)}Column = {(column.OrdinalPosition - 1)};");
                }
            }
            classText.AppendLine("");

            classText.AppendLine($"\t\t\tList<{dataObjectClassIdentifier}> {Library.LowerFirstCharacter(table.Name)}s = new List<{dataObjectClassIdentifier}>();");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\tSqlDataReader {Library.LowerFirstCharacter(table.Name)}sData = SQLDataServer.ExecuteSPReturnDataReader(storedProcedure, parameters);");
            classText.AppendLine($"\t\t\twhile ({Library.LowerFirstCharacter(table.Name)}sData.Read())");
            classText.AppendLine("\t\t\t{");

            if (table.PrimaryKey.DataType == SQLDataTypes.uniqueIdentifier)
                classText.AppendLine($"\t\t\t\t{dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)} = new {dataObjectClassIdentifier}({Library.LowerFirstCharacter(table.Name)}sData.GetGuid({Library.LowerFirstCharacter(table.PrimaryKey.Name)}Column));");
            else
                classText.AppendLine($"\t\t\t\t{dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)} = new {dataObjectClassIdentifier}(Convert.ToInt32({Library.LowerFirstCharacter(table.Name)}sData[\"{table.PrimaryKey.Name}\"]));");
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    if (column.Nullable)
                    {
                        classText.AppendLine("");
                        classText.AppendLine($"\t\t\t\tif ({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"] != DBNull.Value)");
                        classText.Append("\t");
                    }

                    classText.Append($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}.{column.Name} = ");

                    string dataReaderReadStatement;

                    switch (column.DataType)
                    {
                        case SQLDataTypes.uniqueIdentifier:
                            dataReaderReadStatement = $"{Library.LowerFirstCharacter(table.Name)}sData.GetGuid({Library.LowerFirstCharacter(column.Name)}Column);";
                            break;
                        case SQLDataTypes.intData:
                            dataReaderReadStatement = $"Convert.ToInt32({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.varChar:
                            dataReaderReadStatement = $"Convert.ToString({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.bit:
                            dataReaderReadStatement = $"Convert.ToBoolean({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.dateTime:
                            dataReaderReadStatement = $"Convert.ToDateTime({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.varBinary:
                            dataReaderReadStatement = "0; //to be honest, I'm a little surpised";
                            break;
                        case SQLDataTypes.decimalData:
                            dataReaderReadStatement = $"Convert.ToDecimal({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.floatData:
                            dataReaderReadStatement = $"(float){Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"];";
                            break;
                        case SQLDataTypes.binary:
                            dataReaderReadStatement = "Need another look at this";
                            break;
                        case SQLDataTypes.ncharData:
                            dataReaderReadStatement = $"Convert.ToString({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.charType:
                            dataReaderReadStatement = $"Convert.ToString({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        case SQLDataTypes.timeType:
                            dataReaderReadStatement = $"{Library.LowerFirstCharacter(table.Name)}sData.GetTimeSpan({Library.LowerFirstCharacter(column.Name)}Column);";
                            break;
                        case SQLDataTypes.moneyType:
                            dataReaderReadStatement = $"Convert.ToDecimal({Library.LowerFirstCharacter(table.Name)}sData[\"{column.Name}\"]);";
                            break;
                        default:
                            throw new SQLDBTypeNotSupported(column.DataType);
                    }

                    classText.AppendLine(dataReaderReadStatement);

                    if (column.Nullable)
                        classText.AppendLine("");
                }
            }
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}s.Add({Library.LowerFirstCharacter(table.Name)});");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}sData.Close();");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\treturn {Library.LowerFirstCharacter(table.Name)}s;");
            classText.AppendLine("\t\t}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tprivate {dataObjectClassIdentifier} Individual{table.Name}(string storedProcedure, List<SqlParameter> parameters)");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\tList<{dataObjectClassIdentifier}> {Library.LowerFirstCharacter(table.Name)}s = Populate{table.Name}List(storedProcedure, parameters);");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t\tswitch ({Library.LowerFirstCharacter(table.Name)}s.Count)");
            classText.AppendLine("\t\t\t\t{");
            classText.AppendLine($"\t\t\t\t\tcase 1: return {Library.LowerFirstCharacter(table.Name)}s[0];");
            classText.AppendLine("\t\t\t\t\tcase 0: return null;");
            classText.AppendLine("\t\t\t\t\tdefault: throw new Exception(\"Get by ID returning more than one record\");");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t}");
            classText.AppendLine("");

            classText.AppendLine("\t\t#endregion");


            classText.AppendLine("");
            classText.AppendLine("\t\t#region Updating the database");

            classText.AppendLine($"\t\tpublic void Save({dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)})");
            classText.AppendLine("\t\t{");

            classText.AppendLine($"\t\t\tList<SqlParameter> parameters = {table.Name}SaveParameters({Library.LowerFirstCharacter(table.Name)});");
            classText.AppendLine("");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (column.PrimaryKey)
                {
                    if (column.DataType == SQLDataTypes.uniqueIdentifier)
                    {
                        classText.AppendLine($"\t\t\tif ({Library.LowerFirstCharacter(table.Name)}.{column.Name} == Guid.Empty)");
                        classText.AppendLine("\t\t\t{");
                        classText.AppendLine($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}.{column.Name} = SQLDataServer.ExecuteSPReturnGuid(\"{table.Name}Insert\", parameters);");
                        classText.AppendLine("\t\t\t}");
                        classText.AppendLine("\t\t\telse");
                        classText.AppendLine("\t\t\t{");
                        classText.AppendLine($"\t\t\t\t{AddParameter(column)}");
                        classText.AppendLine($"\t\t\t\t{ExecuteSP(table.Name, "Update")}");
                        classText.AppendLine("\t\t\t}");
                    }
                    else
                    {
                        classText.AppendLine($"\t\t\tif ({Library.LowerFirstCharacter(table.Name)}.{column.Name} == 0)");
                        classText.AppendLine("\t\t\t{");
                        classText.AppendLine($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}.{column.Name}= SQLDataServer.ExecuteSPReturnLong(\"{table.Name}Insert\", parameters, \"@{column.Name}\");");
                        classText.AppendLine("\t\t\t}");
                        classText.AppendLine("\t\t\telse");
                        classText.AppendLine("\t\t\t{");
                        classText.AppendLine($"\t\t\t\t{AddParameter(column)}");
                        classText.AppendLine($"\t\t\t\t{ExecuteSP(table.Name, "Update")}");
                        classText.AppendLine("\t\t\t}");
                    }
                }
            }

            classText.AppendLine("\t\t}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tprivate List<SqlParameter> {table.Name}SaveParameters({dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)})");
            classText.AppendLine("\t\t{");

            classText.AppendLine("\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
            classText.AppendLine("");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    classText.AppendLine($"\t\t\t{AddParameter(column)}");
                }
            }
            classText.AppendLine("");
            classText.AppendLine("\t\t\treturn parameters;");

            classText.AppendLine("\t\t}");


            classText.AppendLine("\t\t#endregion");

            classText.AppendLine("");

            classText.AppendLine($"\t\t{newRegion}Deleting Items from the database");

            classText.AppendLine($"\t\tpublic void Delete({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.Name)}Id)");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t{AddParameter(Library.LowerFirstCharacter(table.Name) + "Id", table.PrimaryKey)}");
            classText.AppendLine();
            classText.AppendLine($"\t\t\t{ExecuteSP(table.Name.ToString(), "Delete")}");

            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");




            foreach (SQLForeignKeyRelation foreignKeyRelation in foreignKeys)
            {
                SQLTableColumn column = foreignKeyRelation.ReferencedTableColumn;
                string loweredColumnName = Library.LowerFirstCharacter(column.Name);

                classText.AppendLine($"\t\tpublic void DeleteBy{column.Name}({column.cSharpDataType} {loweredColumnName})");
                classText.AppendLine($"\t\t{{");
                classText.AppendLine($"\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
                classText.AppendLine("");
                classText.AppendLine($"\t\t\t{AddParameter(loweredColumnName, column)}");
                classText.AppendLine();
                classText.AppendLine($"\t\t\t{ExecuteSP(table.Name.ToString(), "DeleteFor" + column.TableName)}");
                classText.AppendLine($"\t\t}}");
            }
            

            classText.AppendLine("\t\t" + endRegion);

            classText.AppendLine("\t}"); // This is the closing of the class.
            classText.AppendLine("}"); // This is the closing of the namespace.

        }

        string AddParameter(SQLTableColumn column)
        {
            return $"SQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {Library.LowerFirstCharacter(column.TableName)}.{column.Name}, SqlDbType.{column.dotNetSqlDataTypes}, {column.MaximumLength});";
        }

        string AddParameter(string parameterValue, SQLTableColumn column)
        {

            return $"SQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {parameterValue}, SqlDbType.{column.dotNetSqlDataTypes}, {column.MaximumLength});";
        }

        string ExecuteSP(string tableName, string action)
        {
            return $"SQLDataServer.ExecuteSP(\"{tableName}{action}\", parameters);";

        }

    }
}
