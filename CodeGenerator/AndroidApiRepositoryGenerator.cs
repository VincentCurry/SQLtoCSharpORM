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

            classText.AppendLine("import androidx.lifecycle.MutableLiveData");
            classText.AppendLine($"import com.{_nameSpace}.data.model.{table.Name}");
            if (table.Columns.Any(col => col.cSharpDataType == "DateTime"))
            {
                classText.AppendLine("import java.util.Date");
            }
            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}Repository (val dataSource: {table.Name}DataSource) {{");
            classText.AppendLine($"\tvar {table.Name.Decapitalise()}s: MutableLiveData<List<{table.Name}>> = MutableLiveData<List<{table.Name}>> (null)");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tsuspend fun fetch{table.Name}({table.Name.Decapitalise()}Id: {table.PrimaryKey.kotlinDataType}) {{");
            classText.AppendLine($"\t\tval result: Result<{table.Name}> = dataSource.get{table.Name}({table.Name.Decapitalise()}Id)");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t\tif (result is Result.Success) {");
            classText.AppendLine($"\t\t\t{table.Name.Decapitalise()}s.value = listOf(result.data)");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tsuspend fun fetchAll{table.Name}() {{");
            classText.AppendLine($"\t\tval result: Result<List<{table.Name}>> = dataSource.getAll{table.Name}()");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t\tif (result is Result.Success) {");
            classText.AppendLine($"\t\t\t{table.Name.Decapitalise()}s.value = result.data");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tsuspend fun save{table.Name}({Library.TableColumnsCode(table, Library.KotlinParameterNameAndType, false, true, true)}) : Result<{table.Name}> {{");
            classText.AppendLine($"\t\treturn dataSource.save{table.Name}({Library.TableColumnsCode(table, Library.ColumnNameDecapitalised, false, true, true)})");
            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }
    }
}
