using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    public class AndroidApiDataSourceGenerator : Generator
    {
        public AndroidApiDataSourceGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "DataSource";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{_nameSpace}.data");
            classText.Append(Environment.NewLine);

            classText.AppendLine("import com.google.gson.GsonBuilder");
            classText.AppendLine("import com.google.gson.reflect.TypeToken");
            classText.AppendLine($"import com.{_nameSpace}.BuildConfig");
            classText.AppendLine($"import com.{_nameSpace}.data.model.{table.Name}");

            classText.AppendLine("import kotlinx.coroutines.Dispatchers");
            classText.AppendLine("import kotlinx.coroutines.withContext");
            classText.AppendLine("import java.io.IOException");
            classText.AppendLine("import java.net.HttpURLConnection");
            classText.AppendLine("import java.net.URLEncoder");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}DataSource {{");

            classText.AppendLine($"\tsuspend fun save{table.Name}({Library.TableColumnsCode(table, Library.KotlinParameterNameAndType, false, true, true)}): Result<{table.Name}> {{");
            classText.AppendLine($"\t\tvar {table.Name.Decapitalise()}Id: String");
            classText.AppendLine("\t\treturn try {");

            // Perform the HTTP request on the IO dispatcher
            classText.AppendLine("\t\t\twithContext(Dispatchers.IO) {");
            classText.AppendLine($"\t\t\t\tval {table.Name.Decapitalise()}InputString: String = \"{{{Library.TableColumnsCode(table, CreateJsonSerialization, false, true, true)}}}\"");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\t\t\tval {table.Name.Decapitalise()}Response: Response = HttpAccess.postSigned(\"https://${{BuildConfig.BASE_URL}}/api/{table.Name.ToLower()}\", {table.Name.Decapitalise()}InputString)");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\t\t\tif ({table.Name.Decapitalise()}Response.code() in 100..399) {{");
            classText.AppendLine($"\t\t\t\t\t{table.Name.Decapitalise()}Id = {table.Name.Decapitalise()}Response.body()?.string()?.trim('\"') ?: \"\"");
            classText.AppendLine($"\t\t\t\t\t{table.Name.Decapitalise()}.{table.Name.Decapitalise()}Id = {table.Name.Decapitalise()}Id");
            classText.AppendLine($"\t\t\t\t\tResult.Success({table.Name.Decapitalise()})");
            classText.AppendLine("\t\t\t\t} else {");
            classText.AppendLine($"\t\t\t\t\tResult.Error(IOException(\"HTTP error code: ${{{table.Name.Decapitalise()}Response.code()}}\"))");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t} catch (e: Exception) {");
            classText.AppendLine($"\t\t\tResult.Error(IOException(\"Error getting scan code\", e))");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }

        private string CreateJsonSerialization(SQLTableColumn column) {
            return $"\\\"{column.Name.Decapitalise()}\\\": \\\"${{{column.TableName.Decapitalise()}.{column.Name.Decapitalise()}}}\\\"";
        }
    }
}
