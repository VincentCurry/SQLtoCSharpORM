using System.Collections.Generic;
      
namespace CodeGenerator
{
    public class MvcApiControllerGenerator  : Generator
    {
        public MvcApiControllerGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            filePrefix = "Controller";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine("using Microsoft.AspNetCore.Mvc;");
            classText.AppendLine($"using {_nameSpace}.Repository;");
            classText.AppendLine("using System;");
            classText.AppendLine("using System.Collections.Generic;");
            classText.AppendLine("");

            classText.AppendLine($"namespace {_nameSpace}.Controllers");
            classText.AppendLine("{");
            classText.AppendLine("\t[Route(\"api/[controller]\")]");
            classText.AppendLine("\t[ApiController]");
            classText.AppendLine($"\tpublic class {table.Name}Controller : ControllerBase");
            classText.AppendLine("\t{");
            classText.AppendLine($"\t\tIRepository <{ClassName(table.Name)}, {table.PrimaryKey.cSharpDataType}> {Library.LowerFirstCharacter(table.Name)}Repository;");
            classText.AppendLine("");

            classText.AppendLine($"\t\tpublic {table.Name}Controller(IRepository<{ClassName(table.Name)}, {table.PrimaryKey.cSharpDataType}> {Library.LowerFirstCharacter(table.Name)}Repository)");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\tthis.{Library.LowerFirstCharacter(table.Name)}Repository = {Library.LowerFirstCharacter(table.Name)}Repository;");
            classText.AppendLine("\t\t}");
            classText.AppendLine("");

            classText.AppendLine($"\t\t[HttpGet]");
            classText.AppendLine($"\t\tpublic IEnumerable<{ClassName(table.Name)}> Get()");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\treturn {Library.LowerFirstCharacter(table.Name)}Repository.GetAll();");
            classText.AppendLine("\t\t}");
            classText.AppendLine($"\t\t");

            classText.AppendLine("\t\t[HttpGet(\"{id}\")]");
            classText.AppendLine($"\t\tpublic {ClassName(table.Name)} Get({table.PrimaryKey.cSharpDataType} id)");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\treturn {Library.LowerFirstCharacter(table.Name)}Repository.GetByID(id);");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\t");
            classText.AppendLine($"\t\t[HttpPost]");
            classText.AppendLine($"\t\tpublic {table.PrimaryKey.cSharpDataType} Post([FromBody] {ClassName(table.Name)} value)");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\t{ClassName(table.Name)} new{table.Name} = value;");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Repository.Save(new{table.Name});");
            classText.AppendLine($"\t\t");
            classText.AppendLine($"\t\t\treturn new{table.Name}.{table.Name}Id;");
            classText.AppendLine("\t\t}");

            classText.AppendLine($"\t\t");
            classText.AppendLine("\t\t[HttpPut(\"{id}\")]");
            classText.AppendLine($"\t\tpublic void Put({table.PrimaryKey.cSharpDataType} id, [FromBody] {ClassName(table.Name)} value)");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\t{ClassName(table.Name)} existing{table.Name} = value;");
            classText.AppendLine($"\t\t\t");
            classText.AppendLine($"\t\t\texisting{table.Name}.{table.Name}Id = id;");
            classText.AppendLine($"\t\t\t");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Repository.Save(existing{table.Name});");
            classText.AppendLine("\t\t}");
            classText.AppendLine($"\t\t");
            classText.AppendLine($"\t\t[HttpDelete(\"{{id}}\")]");
            classText.AppendLine($"\t\tpublic void Delete({table.PrimaryKey.cSharpDataType} id)");
            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\t{Library.LowerFirstCharacter(table.Name)}Repository.Delete(id);");
            classText.AppendLine("\t\t}");
            classText.AppendLine($"\t\t");
            classText.AppendLine("\t}");
            classText.AppendLine("}");

        }
    }
}
