using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeGenerator
{
    public class ObjectModelGenerator
    {
        public void GenerateClasses(List<SQLTable> tables, string destinationFolder, string nameSpace)
        {
            foreach (SQLTable table in tables)
            {
                StringBuilder classText = new StringBuilder();

                classText.AppendLine("using System;");
                classText.AppendLine("using System.Collections.Generic;");
                classText.AppendLine("using System.Linq;");
                classText.AppendLine("using System.Text;");
                classText.AppendLine("using DataServer;");
                classText.AppendLine("using System.Data.SqlClient;");
                classText.AppendLine("using System.Data;");
                classText.AppendLine(Environment.NewLine);

                classText.AppendLine("namespace " + nameSpace);
                classText.AppendLine("{");
                classText.AppendLine("public class " + table.Name);
                classText.AppendLine("{");

                classText.AppendLine("#region Constructors");

                if (table.PrimaryKey.DataType == SQLDataTypes.uniqueIdentifier)
                    classText.AppendLine("public " + table.Name + "(Guid id)");
                else
                    classText.AppendLine("public " + table.Name + "(int id)");

                classText.AppendLine("{");
                classText.AppendLine("this." + Library.LowerFirstCharacter(table.PrimaryKey.Name) + " = id;");
                classText.AppendLine("}");

                classText.AppendLine("public " + table.Name + "()");
                classText.AppendLine("{ }");

                classText.AppendLine("#endregion");
                classText.AppendLine(Environment.NewLine);

                classText.AppendLine("#region Private Properties");
                foreach (SQLTableColumn column in table.Columns)
                {
                    classText.AppendLine("private " + column.cSharpDataType + " " + Library.LowerFirstCharacter(column.Name) + ";");
                }
                classText.AppendLine("#endregion");

                classText.AppendLine("#region Public Properties");

                foreach(SQLTableColumn column in table.Columns)
                {

                    classText.AppendLine("public " + column.cSharpDataType + " " + column.Name);
                    classText.AppendLine("{");
                    classText.AppendLine("get { return " + Library.LowerFirstCharacter(column.Name) + "; }");
                    
                    if(!column.PrimaryKey)
                    {
                        classText.AppendLine("set { " + Library.LowerFirstCharacter(column.Name) + " = value; }");
                    }

                    classText.AppendLine("}");
                }
                classText.AppendLine("#endregion");
                
                classText.AppendLine("#region Loading Methods");
                classText.AppendLine("public static List<" + table.Name + "> GetAll" + table.Name + "s()");
                classText.AppendLine("{");
                classText.AppendLine("return Populate" + table.Name + "List(\"" + table.Name + "GetAll\", new List<SqlParameter>());");
                classText.AppendLine("}");
                classText.AppendLine("");
                
                classText.AppendLine("public static " + table.Name + " Get" + table.Name + "ByID(" + table.PrimaryKey.cSharpDataType + " " + Library.LowerFirstCharacter(table.PrimaryKey.Name) + ")");
                classText.AppendLine("{");
                    classText.AppendLine("List<SqlParameter> parameters = new List<SqlParameter>();");
                    classText.AppendLine("SQLDataServer.AddParameter(ref parameters, \"@" + table.PrimaryKey.Name + "\", " + Library.LowerFirstCharacter(table.PrimaryKey.Name) + ", SqlDbType."+ table.PrimaryKey.dotNetSqlDataTypes+", " + table.PrimaryKey.MaximumLength.ToString() + ");");
                    classText.AppendLine("return Populate" + table.Name + "List(\"" + table.Name + "GetByID\", parameters)[0];");
                classText.AppendLine("}");

                classText.AppendLine("private static List<" + table.Name + "> Populate" + table.Name + "List(string storedProcedure, List<SqlParameter> parameters)");
                classText.AppendLine("{");

                //Check for guid columns as they need to use their ordinal to be identified
                foreach(SQLTableColumn column in table.Columns)
                {
                    if (column.DataType == SQLDataTypes.uniqueIdentifier || column.DataType == SQLDataTypes.timeType)
                    {
                        classText.AppendLine("int " + Library.LowerFirstCharacter(column.Name) + "Column = " + (column.OrdinalPosition - 1) + ";");
                    }
                }
                classText.AppendLine(Environment.NewLine);

                classText.AppendLine("List<" + table.Name + "> " + Library.LowerFirstCharacter(table.Name) + "s = new List<" + table.Name + ">();");
                classText.AppendLine(Environment.NewLine);
                classText.AppendLine("SqlDataReader " + Library.LowerFirstCharacter(table.Name) + "sData = SQLDataServer.ExecuteSPReturnDataReader(storedProcedure, SQLDataServer.ConnectionString, parameters);");
                classText.AppendLine("while (" + Library.LowerFirstCharacter(table.Name) + "sData.Read())"); 
                classText.AppendLine("{");

                if(table.PrimaryKey.DataType==SQLDataTypes.uniqueIdentifier)
                classText.AppendLine(table.Name + " " + Library.LowerFirstCharacter(table.Name) + " = new " + table.Name + "(" + Library.LowerFirstCharacter(table.Name) + "sData.GetGuid(" + Library.LowerFirstCharacter(table.PrimaryKey.Name) + "Column));");
                else
                    classText.AppendLine(table.Name + " " + Library.LowerFirstCharacter(table.Name) + " = new " + table.Name + "(Convert.ToInt32(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + table.PrimaryKey.Name + "\"]));");
                foreach (SQLTableColumn column in table.Columns)
                {
                    if (!column.PrimaryKey)
                    {
                        classText.Append("" + Library.LowerFirstCharacter(table.Name) + "." + Library.LowerFirstCharacter(column.Name) + " = ");
                            
                        string dataReaderReadStatement;
                        
                        switch(column.DataType)
                        {
                            case SQLDataTypes.uniqueIdentifier:
                                dataReaderReadStatement = Library.LowerFirstCharacter(table.Name) + "sData.GetGuid(" + Library.LowerFirstCharacter(column.Name) + "Column);";
                                break;
                            case SQLDataTypes.intData:
                                dataReaderReadStatement = "Convert.ToInt32(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.varChar:
                                dataReaderReadStatement = "Convert.ToString(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.bit:
                                dataReaderReadStatement = "Convert.ToBoolean(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.dateTime:
                                dataReaderReadStatement = "Convert.ToDateTime(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.varBinary:
                                dataReaderReadStatement = "0; //to be honest, I'm a little surpised";
                                break;
                            case SQLDataTypes.decimalData:
                                dataReaderReadStatement = "Convert.ToDecimal(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.floatData:
                                dataReaderReadStatement = "(float)" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"];";
                                break;
                            case SQLDataTypes.binary:
                                dataReaderReadStatement = "Need another look at this";
                                break;
                            case SQLDataTypes.ncharData:
                                dataReaderReadStatement = "Convert.ToString(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.charType:
                                dataReaderReadStatement = "Convert.ToString(" + Library.LowerFirstCharacter(table.Name) + "sData[\"" + column.Name + "\"]);";
                                break;
                            case SQLDataTypes.timeType:
                                dataReaderReadStatement = Library.LowerFirstCharacter(table.Name) + "sData.GetTimeSpan(" + Library.LowerFirstCharacter(column.Name) + "Column);";
                                break;
                            default:
                                throw new SQLDBTypeNotSupported(column.DataType);
                        }
                        
                        classText.AppendLine(dataReaderReadStatement);
                    }
                }
                classText.AppendLine(Environment.NewLine);
                classText.AppendLine("" + Library.LowerFirstCharacter(table.Name) + "s.Add(" + Library.LowerFirstCharacter(table.Name) + ");");
                classText.AppendLine("}");
                classText.AppendLine("" + Library.LowerFirstCharacter(table.Name) + "sData.Close();");
                classText.AppendLine(Environment.NewLine); 
                classText.AppendLine("return " + Library.LowerFirstCharacter(table.Name) + "s;");
                classText.AppendLine("}"); 
                classText.AppendLine(Environment.NewLine);
                classText.AppendLine("#endregion");


                classText.AppendLine(Environment.NewLine);
                classText.AppendLine("#region Updating the database");

                classText.AppendLine("public void Save" + table.Name + "()");
                classText.AppendLine("{");

                classText.AppendLine("List<SqlParameter> parameters = new List<SqlParameter>();");

                foreach (SQLTableColumn column in table.Columns)
                {
                    if (!column.PrimaryKey)
                    {
                        classText.AppendLine("SQLDataServer.AddParameter(ref parameters, \"@" + column.Name + "\", this." + Library.LowerFirstCharacter(column.Name) + ", SqlDbType." + column.dotNetSqlDataTypes + ", " + column.MaximumLength.ToString() + ");");
                    }
                }

                foreach (SQLTableColumn column in table.Columns)
                {
                    if (column.PrimaryKey)
                    {
                        if (column.DataType == SQLDataTypes.uniqueIdentifier)
                        {
                            classText.AppendLine("if (this." + Library.LowerFirstCharacter(column.Name) + " == Guid.Empty)");
                            classText.AppendLine("{");
                            classText.AppendLine("this." + Library.LowerFirstCharacter(column.Name) + " = SQLDataServer.ExecuteSPReturnGuid(\"" + table.Name + "Insert\", SQLDataServer.ConnectionString, parameters);");
                            classText.AppendLine("}");
                            classText.AppendLine("else");
                            classText.AppendLine("{");
                            classText.AppendLine("SQLDataServer.AddParameter(ref parameters, \"@" + column.Name + "\", this." + Library.LowerFirstCharacter(column.Name) + ", SqlDbType.UniqueIdentifier, 16);");
                            classText.AppendLine("SQLDataServer.ExecuteSP(\"" + table.Name + "Update\", SQLDataServer.ConnectionString, parameters);");
                            classText.AppendLine("}");
                        }
                        else
                        {
                            classText.AppendLine("if (this." + Library.LowerFirstCharacter(column.Name) + " == 0)");
                            classText.AppendLine("{");
                            classText.AppendLine("this." + Library.LowerFirstCharacter(column.Name) + "= SQLDataServer.ExecuteSPReturnLong(\"" + table.Name + "Insert\", SQLDataServer.ConnectionString, parameters, \"@" + column.Name + "\");");
                            classText.AppendLine("}");
                            classText.AppendLine("else");
                            classText.AppendLine("{");
                            classText.AppendLine("SQLDataServer.AddParameter(ref parameters, \"@" + column.Name + "\", this." + Library.LowerFirstCharacter(column.Name) + ", SqlDbType.Int, 4);");
                            classText.AppendLine("SQLDataServer.ExecuteSP(\"" + table.Name + "Update\", SQLDataServer.ConnectionString, parameters);");
                            classText.AppendLine("}");
                        }
                    }
                }

                classText.AppendLine("}");
                classText.AppendLine("#endregion");

                classText.AppendLine("}"); // This is the closing of the class.
                classText.AppendLine("}"); // This is the closing of the namespace.
                TextWriter writer = File.CreateText(destinationFolder + table.Name + ".cs");

                writer.Write(classText.ToString());

                writer.Close();

            }
        }
    }
}
