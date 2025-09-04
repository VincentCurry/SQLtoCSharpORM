using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidApiRepositoryGenerator : Generator
    {
        public AndroidApiRepositoryGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "Repository";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {

            classText.AppendLine($"package com.{_nameSpace}.data");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"import com.{_nameSpace}.data.model.{table.Name}");
            if (table.Columns.Any(col => col.cSharpDataType == "DateTime"))
            {
                classText.AppendLine("import java.util.Date");
            }
            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}Repository (val dataSource: {table.Name}DataSource) {{");

            classText.AppendLine($"\tsuspend fun fetch{table.Name}({table.Name.Decapitalise()}Id: {table.PrimaryKey.kotlinDataType}): Result<{table.Name}> {{");
            classText.AppendLine($"\t\treturn dataSource.get{table.Name}({table.Name.Decapitalise()}Id)");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tsuspend fun fetchAll{table.Name}s() : Result<List<{table.Name}>> {{");
            classText.AppendLine($"\t\treturn dataSource.getAll{table.Name}()");
            classText.AppendLine("\t\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tsuspend fun save{table.Name}({Library.TableColumnsCode(table, Library.KotlinParameterNameAndType, false, true, true)}) : Result<{table.Name}> {{");
            classText.AppendLine($"\t\treturn dataSource.save{table.Name}({Library.TableColumnsCode(table, Library.ColumnNameDecapitalised, false, true, true)})");
            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }
    }
}
