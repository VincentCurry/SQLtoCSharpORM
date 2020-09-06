using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeGenerator
{
    public class SqlIntegrationTestsGenerator : Generator
    {
        public SqlIntegrationTestsGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        { }


        internal override void GenerateFilePerTable(SQLTable table)
        {

            StringBuilder classText = new StringBuilder();

            classText.AppendLine($"using {_nameSpace}.Repository;");
            classText.AppendLine("using System;");
            classText.AppendLine("using Xunit;");
            classText.AppendLine("");

            classText.AppendLine($"namespace {_nameSpace}.SqlRepository.Test");
            classText.AppendLine("{");
            classText.AppendLine($"\tpublic class {table.Name}RepositoryIntegrationTest");
            classText.AppendLine($"\t{{");

            foreach(SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    if (column.DataType == SQLDataTypes.uniqueIdentifier)
                        classText.AppendLine($"\t\tprivate {column.cSharpDataType} _{Library.LowerFirstCharacter(column.Name)} = Guid.NewGuid();");
                    else
                        classText.AppendLine($"\t\tprivate {column.cSharpDataType} _{Library.LowerFirstCharacter(column.Name)} = {column.RandomValue()};");
                }
            }

            classText.AppendLine($"\t\tprivate {table.Name}RepositorySql _{Library.LowerFirstCharacter(table.Name)}Repository = new {table.Name}RepositorySql();");
            classText.AppendLine("");

            classText.AppendLine($"\t\t[Fact]");
            classText.AppendLine($"\t\tpublic void {table.Name}CRD()");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\t{table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.Name)}Id;");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Id = Create();");
            classText.AppendLine($"\t\t\tGetById({Library.LowerFirstCharacter(table.Name)}Id);");
            classText.AppendLine($"\t\t\tDelete({Library.LowerFirstCharacter(table.Name)}Id);");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tprivate {table.PrimaryKey.cSharpDataType} Create()");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\t{dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)} = new {dataObjectClassIdentifier}();");
            classText.AppendLine("");
            foreach (SQLTableColumn columnSetValue in table.Columns)
            {
                if (!columnSetValue.PrimaryKey)
                    classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}.{columnSetValue.Name} = _{Library.LowerFirstCharacter(columnSetValue.Name)};");
            }
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t_{Library.LowerFirstCharacter(table.Name)}Repository.Save({Library.LowerFirstCharacter(table.Name)});");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\tConsole.WriteLine($\"{table.Name} created id:{{{Library.LowerFirstCharacter(table.Name)}.{table.Name}Id}}\");");
            classText.AppendLine("");

            string idNotSetValue = table.PrimaryKey.cSharpDataType == "Guid" ? "Guid.Empty" : "0";

            classText.AppendLine($"\t\t\tAssert.NotEqual({idNotSetValue}, {Library.LowerFirstCharacter(table.Name)}.{table.Name}Id);");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\treturn {Library.LowerFirstCharacter(table.Name)}.{table.Name}Id;");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");

            classText.AppendLine($"\t\tprivate void GetById({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.Name)}Id)");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t{dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)} = _{Library.LowerFirstCharacter(table.Name)}Repository.GetByID({Library.LowerFirstCharacter(table.Name)}Id);");
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

            classText.AppendLine($"\t\tprivate void Delete({table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.Name)}Id)");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tint start{table.Name}Count = _{Library.LowerFirstCharacter(table.Name)}Repository.GetAll().Count;");

            classText.AppendLine("");
            classText.AppendLine($"\t\t\t_{Library.LowerFirstCharacter(table.Name)}Repository.Delete({Library.LowerFirstCharacter(table.Name)}Id);");
            
            classText.AppendLine("");
            classText.AppendLine($"\t\t\t{dataObjectClassIdentifier} {Library.LowerFirstCharacter(table.Name)} = _{Library.LowerFirstCharacter(table.Name)}Repository.GetByID({Library.LowerFirstCharacter(table.Name)}Id);");

            classText.AppendLine("");
            classText.AppendLine($"\t\t\tint end{table.Name}Count = _{Library.LowerFirstCharacter(table.Name)}Repository.GetAll().Count;");

            classText.AppendLine("");
            classText.AppendLine($"\t\t\tConsole.WriteLine($\"{table.Name} deleted id: {{{Library.LowerFirstCharacter(table.Name)}Id}}\");");

            classText.AppendLine("");
            classText.AppendLine($"\t\t\tAssert.Null({Library.LowerFirstCharacter(table.Name)});");
            classText.AppendLine($"\t\t\tAssert.Equal(end{table.Name}Count + 1, start{table.Name}Count);");

            classText.AppendLine("");
            classText.AppendLine($"\t\t}}");


            classText.AppendLine("\t}");
            classText.AppendLine("}");



            TextWriter writer = File.CreateText($"{_destinationFolder}{table.Name}RepositoryIntegrationTest.cs");

            writer.Write(classText.ToString());

            writer.Close();
        }
    }
}
