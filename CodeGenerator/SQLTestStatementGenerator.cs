using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    public class SQLTestStatementGenerator : Generator
    {
        public SQLTestStatementGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            List<SQLForeignKeyRelation> foreignKeys = sQLForeignKeyRelationsForTable(table);
            
            classText.AppendLine($"\t\tinternal static Tuple<Guid, bool> GetOrCreate{table.Name}ForTesting({Library.ForeignKeysCode(foreignKeys, Library.CSharpParameterTypeAndNameForFunctionParameter, appendCommas:true, singleLine:true)})");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tGuid {Library.LowerFirstCharacter(table.PrimaryKey.Name)};");
            classText.AppendLine("");

            classText.AppendLine($"\t\t\tstring {Library.LowerFirstCharacter(table.Name)}Sql = $\"SELECT TOP 1 {table.Name}Id FROM {table.Name}{(foreignKeys.Count == 0 ? "" : " WHERE")}{Library.ForeignKeysCode(foreignKeys, Library.SqlSelectParameters, appendCommas: true, singleLine: true, separatorCharacter: " AND")} ORDER BY NEWID()\";");
            classText.AppendLine("");

            classText.AppendLine($"\t\t\tSqlDataReader {Library.LowerFirstCharacter(table.Name)}IdData = SQLDataServer.ExecuteSQLStringReturnDataReader({Library.LowerFirstCharacter(table.Name)}Sql);");
            classText.AppendLine("");

            classText.AppendLine($"\t\t\twhile ({Library.LowerFirstCharacter(table.Name)}IdData.Read())");
            classText.AppendLine($"\t\t\t{{");
            classText.AppendLine($"\t\t\t\t{Library.LowerFirstCharacter(table.Name)}Id = {Library.LowerFirstCharacter(table.Name)}IdData.GetGuid(0);");
            classText.AppendLine($"\t\t\t\treturn new Tuple<Guid, bool>({Library.LowerFirstCharacter(table.Name)}Id, false);");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine("");

            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Id = Guid.NewGuid();");
            classText.AppendLine("");

            string sqlParameters = "";

            foreach (SQLTableColumn column in table.Columns)
            {
                if (sqlParameters != "")
                    sqlParameters += ", ";

                if (column.PrimaryKey|| foreignKeys.Select(fk => fk.ParentTableColum).Contains(column))
                {
                    sqlParameters += $"'{{{Library.LowerFirstCharacter(column.Name)}}}'";
                }
                else
                {
                    sqlParameters += $"'{column.RandomValue(true)}'";
                }

            }

            

            classText.AppendLine($"\t\t\tstring create{table.Name}Message = $\"INSERT INTO {table.Name} ({Library.TableColumnsCode(table, Library.SqlParameters , includePrimaryKey: true, appendCommas: true, singleLine: true)})\\r\\nVALUES ({sqlParameters})\";");
            classText.AppendLine("");

            classText.AppendLine($"\t\t\tSQLDataServer.ExecuteSQL(create{table.Name}Message);");
            classText.AppendLine("");

            classText.AppendLine($"\t\t\treturn new Tuple<Guid, bool>({Library.LowerFirstCharacter(table.Name)}Id, true);");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tinternal static void Delete{table.Name}IfRecentlyCreated(Tuple<Guid, bool> {Library.LowerFirstCharacter(table.Name)}IdAndNew)");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tif ({Library.LowerFirstCharacter(table.Name)}IdAndNew.Item2)");
            classText.AppendLine($"\t\t\t{{");
            classText.AppendLine($"\t\t\t\tstring delete{table.Name}Sql = $\"DELETE {table.Name} WHERE {table.PrimaryKey.Name} = '{{{Library.LowerFirstCharacter(table.Name)}IdAndNew.Item1}}'\";");
            classText.AppendLine($"\t\t\t\tSQLDataServer.ExecuteSQL(delete{table.Name}Sql);");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tinternal static void Delete{table.Name}(Guid {Library.LowerFirstCharacter(table.PrimaryKey.Name)})");
            classText.AppendLine($"\t\t\t{{");
            classText.AppendLine($"\t\t\tstring delete{table.Name}Sql = $\"DELETE {table.Name} WHERE {table.PrimaryKey.Name} = '{{{Library.LowerFirstCharacter(table.PrimaryKey.Name)}}}'\";");
            classText.AppendLine($"\t\t\tSQLDataServer.ExecuteSQL(delete{table.Name}Sql);");
            classText.AppendLine($"\t\t}} ");
            classText.AppendLine("");
        }
    }
}
