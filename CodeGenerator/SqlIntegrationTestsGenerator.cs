using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerator
{
    public class SqlIntegrationTestsGenerator : Generator
    {
        
        public SqlIntegrationTestsGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileNameSuffix = "RepositoryIntegrationTest";
        }


        internal override void GenerateFilePerTable(SQLTable table)
        {
            List<SQLForeignKeyRelation> foreignKeys = sQLForeignKeyRelationsForTable(table);

            classText.AppendLine($"using {_nameSpace}.Repository;");
            classText.AppendLine("using System;");
            classText.AppendLine("using Xunit;");
            classText.AppendLine("");

            classText.AppendLine($"namespace {_nameSpace}.SqlRepository.Test");
            classText.AppendLine("{");
            classText.AppendLine($"\tpublic class {table.Name}RepositoryIntegrationTest");
            classText.AppendLine($"\t{{");

            classText.AppendLine(ColumnParametersForTable(table));
            classText.AppendLine(ForeignKeyCode(table, ColumnParametersForTable));

            classText.AppendLine(CreateRepository(table));
            classText.AppendLine(ForeignKeyCode(table, CreateRepository));

            classText.AppendLine("");

            classText.AppendLine($"\t\t[Fact]");
            classText.AppendLine($"\t\tpublic void {table.Name}CRD()");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\t{table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.PrimaryKey.Name)};");
            classText.AppendLine("");

            classText.AppendLine(ForeignKeyCode(table, CreateParentObjectCode, prepend:true));

            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.PrimaryKey.Name)} = Create();");
            classText.AppendLine($"\t\t\tGetById({Library.LowerFirstCharacter(table.PrimaryKey.Name)});");
            classText.AppendLine($"\t\t\tDelete({Library.LowerFirstCharacter(table.PrimaryKey.Name)});");

            classText.AppendLine(ForeignKeyCode(table, DeleteParentObjectCode));

            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");

            classText.AppendLine(ForeignKeyCode(table, CreateRecordInTableFunction));

            classText.AppendLine(CreateRecordInTableFunction(table, false));
           
            classText.AppendLine("");

            classText.AppendLine($"\t\tprivate void GetById({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.PrimaryKey.Name)})");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t{dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)} = _{Library.LowerFirstCharacter(table.Name)}Repository.GetByID({Library.LowerFirstCharacter(table.PrimaryKey.Name)});");
            classText.AppendLine("");
            foreach(SQLTableColumn columnCheckValue in table.Columns)
            {
                if (columnCheckValue.PrimaryKey)
                    classText.AppendLine($"\t\t\tAssert.Equal({Library.LowerFirstCharacter(columnCheckValue.Name)}, {Library.LowerFirstCharacter(table.Name)}.{columnCheckValue.Name});");
                else
                    classText.AppendLine($"\t\t\tAssert.Equal(_{Library.LowerFirstCharacter(columnCheckValue.Name)}, {Library.LowerFirstCharacter(table.Name)}.{columnCheckValue.Name});");
            }
            classText.AppendLine("");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine(DeleteRecordInTableFunction(table, false));
            classText.AppendLine(ForeignKeyCode(table, DeleteRecordInTableFunction));

            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }

        private string CreateRecordInTableFunction(SQLTable table)
        {
            return CreateRecordInTableFunction(table, true);
        }
        private string CreateRecordInTableFunction(SQLTable table, bool foreignKeyTable)
        {
            StringBuilder createFunction = new StringBuilder();

            createFunction.AppendLine($"\t\tprivate {table.PrimaryKey.cSharpDataType} Create{(foreignKeyTable?table.Name:"")}()");
            createFunction.AppendLine($"\t\t{{");
            createFunction.AppendLine($"\t\t\t{(table.Name == _nameSpace ? $"Repository.{table.Name}" : table.Name)} {Library.LowerFirstCharacter(table.Name)} = new {(table.Name == _nameSpace ? $"Repository.{table.Name}" : table.Name)}();");
            createFunction.AppendLine("");
            foreach (SQLTableColumn columnSetValue in table.Columns)
            {
                if (!columnSetValue.PrimaryKey)
                {
                    string value =  $"_{Library.LowerFirstCharacter(columnSetValue.Name)}";
                    createFunction.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}.{columnSetValue.Name} = {value};");
                }
            }
            createFunction.AppendLine("");
            createFunction.AppendLine($"\t\t\t_{Library.LowerFirstCharacter(table.Name)}Repository.Save({Library.LowerFirstCharacter(table.Name)});");
            createFunction.AppendLine("");
            createFunction.AppendLine($"\t\t\tConsole.WriteLine($\"{table.Name} created id:{{{Library.LowerFirstCharacter(table.Name)}.{table.PrimaryKey.Name}}}\");");
            createFunction.AppendLine("");

            string idNotSetValue = table.PrimaryKey.cSharpDataType == "Guid" ? "Guid.Empty" : "0";

            createFunction.AppendLine($"\t\t\tAssert.NotEqual({idNotSetValue}, {Library.LowerFirstCharacter(table.Name)}.{table.PrimaryKey.Name});");
            createFunction.AppendLine("");
            createFunction.AppendLine($"\t\t\treturn {Library.LowerFirstCharacter(table.Name)}.{table.PrimaryKey.Name};");
            createFunction.AppendLine($"\t\t}}");

            return createFunction.ToString();
        }

        private string DeleteRecordInTableFunction(SQLTable table)
        {
            return DeleteRecordInTableFunction(table, true);
        }

        private string DeleteRecordInTableFunction(SQLTable table, bool appendTableNameToFunction)
        {
            StringBuilder deleteFunction = new StringBuilder();

            deleteFunction.AppendLine($"\t\tprivate void Delete{(appendTableNameToFunction ? table.Name : "")}({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.PrimaryKey.Name)})");
            deleteFunction.AppendLine($"\t\t{{");
            deleteFunction.AppendLine($"\t\t\tint start{table.Name}Count = _{Library.LowerFirstCharacter(table.Name)}Repository.GetAll().Count;");

            deleteFunction.AppendLine("");
            deleteFunction.AppendLine($"\t\t\t_{Library.LowerFirstCharacter(table.Name)}Repository.Delete({Library.LowerFirstCharacter(table.PrimaryKey.Name)});");

            deleteFunction.AppendLine("");
            deleteFunction.AppendLine($"\t\t\t{(table.Name == _nameSpace ? $"Repository.{table.Name}" : table.Name)} {Library.LowerFirstCharacter(table.Name)} = _{Library.LowerFirstCharacter(table.Name)}Repository.GetByID({Library.LowerFirstCharacter(table.PrimaryKey.Name)});");

            deleteFunction.AppendLine("");
            deleteFunction.AppendLine($"\t\t\tint end{table.Name}Count = _{Library.LowerFirstCharacter(table.Name)}Repository.GetAll().Count;");

            deleteFunction.AppendLine("");
            deleteFunction.AppendLine($"\t\t\tConsole.WriteLine($\"{table.Name} deleted id: {{{Library.LowerFirstCharacter(table.PrimaryKey.Name)}}}\");");

            deleteFunction.AppendLine("");
            deleteFunction.AppendLine($"\t\t\tAssert.Null({Library.LowerFirstCharacter(table.Name)});");
            deleteFunction.AppendLine($"\t\t\tAssert.Equal(end{table.Name}Count + 1, start{table.Name}Count);");

            deleteFunction.AppendLine("");
            deleteFunction.AppendLine($"\t\t}}");

            return deleteFunction.ToString();
        }

        private string CreateRepository(SQLTable table)
        {
            return $"\t\tprivate {table.Name}RepositorySql _{Library.LowerFirstCharacter(table.Name)}Repository = new {table.Name}RepositorySql();";
        }

        private string CreateParentObjectCode(SQLTable table)
        {
            return ($"\t\t\t_{Library.LowerFirstCharacter(table.PrimaryKey.Name)} = Create{table.Name}();");
        }

        private string DeleteParentObjectCode(SQLTable table)
        {
            return ($"\t\t\tDelete{table.Name}(_{Library.LowerFirstCharacter(table.PrimaryKey.Name)});");
        }

        private string ColumnParametersForTable(SQLTable table)
        {
            StringBuilder columnNames = new StringBuilder();

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    if (column.DataType == SQLDataTypes.uniqueIdentifier)
                        columnNames.AppendLine($"\t\tprivate {column.cSharpDataType} _{Library.LowerFirstCharacter(column.Name)} = Guid.NewGuid();");
                    else
                        columnNames.AppendLine($"\t\tprivate {column.cSharpDataType} _{Library.LowerFirstCharacter(column.Name)} = {column.RandomValue()};");
                }
            }

            return columnNames.ToString();
        }

        private delegate string CodeFunction(SQLTable table);
        private string ForeignKeyCode(SQLTable table, CodeFunction codeFunction)
        {
            return ForeignKeyCode(table, codeFunction, false);
        }

        private string ForeignKeyCode(SQLTable table, CodeFunction codeFunction, bool prepend)
        {
            StringBuilder foreignKeyCode = new StringBuilder();

            foreach (SQLForeignKeyRelation foreignKey in sQLForeignKeyRelationsForTable(table))
            {
                SQLTableColumn column = foreignKey.ReferencedTableColumn;
                if (prepend)
                    foreignKeyCode.Insert(0, codeFunction(column.ParentTable) + Environment.NewLine);
                else
                    foreignKeyCode.Append(codeFunction(column.ParentTable) + Environment.NewLine);

                string childForeignKeyCreateFunctions = ForeignKeyCode(column.ParentTable, codeFunction, prepend);
                if (childForeignKeyCreateFunctions != "")
                {
                    if (prepend)
                        foreignKeyCode.Insert(0, childForeignKeyCreateFunctions);
                    else
                        foreignKeyCode.Append(childForeignKeyCreateFunctions);
                }
            }

            return foreignKeyCode.ToString();
        }


    }
}
