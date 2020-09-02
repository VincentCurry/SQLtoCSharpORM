using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeGenerator
{
    public class IOCRepositoryGenerator
    {
        string newRegion = "#region ";
        string endRegion = "#endregion";
        public void GenerateClasses(List<SQLTable> tables, string destinationFolder, string nameSpace, string interfaceNameSpace)
        {
            foreach (SQLTable table in tables)
            {
                StringBuilder classText = new StringBuilder();

                classText.AppendLine("using System;");
                classText.AppendLine("using System.Collections.Generic;");
                classText.AppendLine("using DataServer;");
                classText.AppendLine("using System.Data.SqlClient;");
                classText.AppendLine("using System.Data;");
                classText.AppendLine($"using {interfaceNameSpace};");
                classText.AppendLine("");

                classText.AppendLine($"namespace {nameSpace}");
                classText.AppendLine("{");
                classText.AppendLine($"\tpublic class {table.Name}RepositorySql : IRepository<{table.Name}>");
                classText.AppendLine("\t{");
                classText.AppendLine("\t\t#region Loading Methods");
                classText.AppendLine($"\t\tpublic List<{table.Name}> GetAll()");
                classText.AppendLine("\t\t{");
                classText.AppendLine($"\t\t\treturn Populate{table.Name}List(\"{table.Name}GetAll\", new List<SqlParameter>());");
                classText.AppendLine("\t\t}");
                classText.AppendLine("");

                classText.AppendLine($"\t\tpublic {table.Name} GetByID({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.PrimaryKey.Name)})");
                classText.AppendLine("\t\t{");
                classText.AppendLine("\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
                classText.AppendLine($"\t\t\tSQLDataServer.AddParameter(ref parameters, \"@{table.PrimaryKey.Name}\", {Library.LowerFirstCharacter(table.PrimaryKey.Name)}, SqlDbType.{table.PrimaryKey.dotNetSqlDataTypes}, {table.PrimaryKey.MaximumLength.ToString()});");
                classText.AppendLine($"\t\t\tList<{table.Name}> {table.Name}Bases = Populate{table.Name}List(\"{table.Name}GetByID\", parameters);");
                classText.AppendLine("");
                classText.AppendLine($"\t\t\t\tswitch ({table.Name}Bases.Count)");
                classText.AppendLine("\t\t\t\t{");
                classText.AppendLine("\t\t\t\t\tcase 1:");
                classText.AppendLine($"\t\t\t\t\t\treturn {table.Name}Bases[0];");
                classText.AppendLine("\t\t\t\t\tcase 0:");
                classText.AppendLine("\t\t\t\t\t\treturn null;");
                classText.AppendLine("\t\t\t\t\tdefault:");
                classText.AppendLine("\t\t\t\t\t\tthrow new Exception(\"Get by ID returning more than one record\");");
                classText.AppendLine("\t\t\t\t}");
                classText.AppendLine("\t\t}");

                classText.AppendLine("");
                classText.AppendLine($"\t\tprivate List<{table.Name}> Populate{table.Name}List(string storedProcedure, List<SqlParameter> parameters)");
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

                classText.AppendLine($"\t\t\tList<{table.Name}> {Library.LowerFirstCharacter(table.Name)}s = new List<{table.Name}>();");
                classText.AppendLine("");
                classText.AppendLine($"\t\t\tSqlDataReader {Library.LowerFirstCharacter(table.Name)}sData = SQLDataServer.ExecuteSPReturnDataReader(storedProcedure, Constants.ConnectionString, parameters);");
                classText.AppendLine($"\t\t\twhile ({Library.LowerFirstCharacter(table.Name)}sData.Read())");
                classText.AppendLine("\t\t\t{");

                if (table.PrimaryKey.DataType == SQLDataTypes.uniqueIdentifier)
                    classText.AppendLine($"\t\t\t\t{table.Name} {Library.LowerFirstCharacter(table.Name)} = new {table.Name}({Library.LowerFirstCharacter(table.Name)}sData.GetGuid({Library.LowerFirstCharacter(table.PrimaryKey.Name)}Column));");
                else
                    classText.AppendLine($"\t\t\t\t{table.Name} {Library.LowerFirstCharacter(table.Name)} = new {table.Name}(Convert.ToInt32({Library.LowerFirstCharacter(table.Name)}sData[\"{table.PrimaryKey.Name}\"]));");
                foreach (SQLTableColumn column in table.Columns)
                {
                    if (!column.PrimaryKey)
                    {
                        classText.Append($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}.{column.Name} = ");

                        string dataReaderReadStatement;

                        switch (column.DataType)
                        {
                            case SQLDataTypes.uniqueIdentifier:
                                dataReaderReadStatement = $"{table.Name}sData.GetGuid({Library.LowerFirstCharacter(column.Name)}Column);";
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
                            default:
                                throw new SQLDBTypeNotSupported(column.DataType);
                        }

                        classText.AppendLine(dataReaderReadStatement);
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
                classText.AppendLine("\t\t#endregion");


                classText.AppendLine("");
                classText.AppendLine("\t\t#region Updating the database");

                classText.AppendLine($"\t\tpublic void Save({table.Name} {Library.LowerFirstCharacter(table.Name)})");
                classText.AppendLine("\t\t{");

                classText.AppendLine("\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
                classText.AppendLine("");

                foreach (SQLTableColumn column in table.Columns)
                {
                    if (!column.PrimaryKey)
                    {
                        classText.AppendLine($"\t\t\tSQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {Library.LowerFirstCharacter(table.Name)}.{column.Name}, SqlDbType.{column.dotNetSqlDataTypes}, {column.MaximumLength});");
                    }
                }

                foreach (SQLTableColumn column in table.Columns)
                {
                    if (column.PrimaryKey)
                    {
                        if (column.DataType == SQLDataTypes.uniqueIdentifier)
                        {
                            classText.AppendLine($"\t\t\tif ({Library.LowerFirstCharacter(table.Name)}.{column.Name} == Guid.Empty)");
                            classText.AppendLine("\t\t\t{");
                            classText.AppendLine($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}.{column.Name} = SQLDataServer.ExecuteSPReturnGuid(\"{table.Name}Insert\", Constants.ConnectionString, parameters);");
                            classText.AppendLine("\t\t\t}");
                            classText.AppendLine("\t\t\telse");
                            classText.AppendLine("\t\t\t{");
                            classText.AppendLine($"\t\t\t\tSQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {Library.LowerFirstCharacter(table.Name)}.{column.Name}, SqlDbType.UniqueIdentifier, 16);");
                            classText.AppendLine($"\t\t\t\tSQLDataServer.ExecuteSP(\"{table.Name}Update\", Constants.ConnectionString, parameters);");
                            classText.AppendLine("\t\t\t}");
                        }
                        else
                        {
                            classText.AppendLine($"\t\t\tif ({Library.LowerFirstCharacter(table.Name)}.{column.Name} == 0)");
                            classText.AppendLine("\t\t\t{");
                            classText.AppendLine($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}.{column.Name}= SQLDataServer.ExecuteSPReturnLong(\"{table.Name}Insert\", Constants.ConnectionString, parameters, \"@{column.Name}\");");
                            classText.AppendLine("\t\t\t}");
                            classText.AppendLine("\t\t\telse");
                            classText.AppendLine("\t\t\t{");
                            classText.AppendLine($"\t\t\t\tSQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {Library.LowerFirstCharacter(table.Name)}.{column.Name}, SqlDbType.Int, 4);");
                            classText.AppendLine($"\t\t\t\tSQLDataServer.ExecuteSP(\"{table.Name}Update\", Constants.ConnectionString, parameters);");
                            classText.AppendLine("\t\t\t}");
                        }
                    }
                }

                classText.AppendLine("\t\t}");
                classText.AppendLine("\t\t#endregion");

                classText.AppendLine("");

                classText.AppendLine($"\t\t{newRegion}Deleting Items from the database");

                classText.AppendLine($"\t\tpublic void Delete({table.Name} {Library.LowerFirstCharacter(table.Name)})");
                classText.AppendLine($"\t\t{{");
                classText.AppendLine($"\t\t\tList<SqlParameter> parameters = new List<SqlParameter>();");
                classText.AppendLine("");
                classText.AppendLine($"\t\t\t{AddParameter(table, table.PrimaryKey)}");
                classText.AppendLine();
                classText.AppendLine($"\t\t\t{ExecuteSP(table.Name.ToString(), "Delete")})");

                classText.AppendLine("\t\t" + endRegion);

                classText.AppendLine("\t}"); // This is the closing of the class.
                classText.AppendLine("}"); // This is the closing of the namespace.

                TextWriter writer = File.CreateText($"{destinationFolder + table.Name}RepositorySql.cs");

                writer.Write(classText.ToString());

                writer.Close();
            }

            string AddParameter(SQLTable table, SQLTableColumn column)
            {
                return $"SQLDataServer.AddParameter(ref parameters, \"@{column.Name}\", {Library.LowerFirstCharacter(table.Name)}.{column.Name}, SqlDbType.{column.dotNetSqlDataTypes}, {column.MaximumLength})";
            }

            string ExecuteSP(string tableName, string action)
            {
                return $"SQLDataServer.ExecuteSP(\"{tableName}{action}\", Constants.ConnectionString, parameters);";

            }
        }
    }
}
