using System.Collections.Generic;

namespace CodeGenerator
{
    public class MvcApiControllerTestsGenerator : Generator
    {
        public MvcApiControllerTestsGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            filePrefix = "ControllerTest";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"using Moq;");
            classText.AppendLine($"using {_nameSpace}.Controllers;");
            classText.AppendLine($"using {_nameSpace}.Repository;");
            classText.AppendLine($"using System;");
            classText.AppendLine($"using Xunit;");
            classText.AppendLine($"");

            classText.AppendLine($"namespace {_nameSpace}.Tests");
            classText.AppendLine($"{{");
            classText.AppendLine($"\tpublic class {table.Name}ControllerTest");
            classText.AppendLine($"\t{{");

            classText.AppendLine($"\t\tMock<IRepository<{table.Name}, {table.PrimaryKey.cSharpDataType}>> mock{table.Name}Repo;");
            classText.AppendLine($"\t\t{table.Name}Controller {Library.LowerFirstCharacter(table.Name)}Controller;");
            classText.AppendLine($"\t\tMock<{table.Name}> mock{table.Name};");
            classText.AppendLine($"");

            classText.AppendLine($"\t\tvoid Setup()");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tmock{table.Name}Repo = new Mock<IRepository<{table.Name}, {table.PrimaryKey.cSharpDataType}>>();");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Controller = new {table.Name}Controller(mock{table.Name}Repo.Object);");
            classText.AppendLine($"\t\t\tmock{table.Name} = new Mock<{table.Name}>();");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"");

            classText.AppendLine($"\t\t[Fact]");
            classText.AppendLine($"\t\tpublic void TestGetCallsGetAllRepository()");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tSetup();");
            classText.AppendLine($"");

            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Controller.Get();");
            classText.AppendLine($"");
            classText.AppendLine($"\t\t\tmock{table.Name}Repo.Verify(b => b.GetAll(), Times.Once);");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"");


            classText.AppendLine($"\t\t[Fact]");

            classText.AppendLine($"\t\tpublic void TestGetOneCallsGetRepository()"); 
            
            classText.AppendLine($"\t\t{{");

            classText.AppendLine($"\t\t\tSetup();");

            classText.AppendLine($"");

            classText.AppendLine($"\t\t\t{table.PrimaryKey.cSharpDataType} {Library.LowerFirstCharacter(table.PrimaryKey.Name)} = {idTestValue(table.PrimaryKey)};");
            classText.AppendLine($"");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Controller.Get({Library.LowerFirstCharacter(table.PrimaryKey.Name)});");
            classText.AppendLine($"");

            classText.AppendLine($"\t\t\tmock{table.Name}Repo.Verify(b => b.GetByID({Library.LowerFirstCharacter(table.PrimaryKey.Name)}), Times.Once);");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"");

            classText.AppendLine($"\t\t[Fact]");
            classText.AppendLine($"\t\tpublic void TestPostCallsSaveRepository()");

            classText.AppendLine($"\t\t{{");

            classText.AppendLine($"\t\t\tSetup();");
            classText.AppendLine($"");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Controller.Post(mock{table.Name}.Object);");
            classText.AppendLine($"");
            classText.AppendLine($"\t\t\tmock{table.Name}Repo.Verify(b => b.Save(mock{table.Name}.Object), Times.Once);");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine($"");

            classText.AppendLine($"\t\t[Fact]");
            classText.AppendLine($"\t\tpublic void TestPutCallsSaveRepository()");
            classText.AppendLine($"\t\t{{");
            classText.AppendLine($"\t\t\tSetup();");
            classText.AppendLine($"");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Controller.Put({idTestValue(table.PrimaryKey)}, mock{table.Name}.Object); ");
            
            classText.AppendLine($"");

            classText.AppendLine($"\t\t\tmock{table.Name}Repo.Verify(b => b.Save(mock{table.Name}.Object), Times.Once); ");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"}}");

         }

        string idTestValue(SQLTableColumn primaryKeyColumn)
        {
            if (primaryKeyColumn.DataType == SQLDataTypes.uniqueIdentifier)
                return "Guid.NewGuid()";
            else
                return primaryKeyColumn.RandomValue().ToString();
        }
    }
}
