using System;
using System.Collections.Generic;
using System.Linq;

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
            if(table.Columns.Any(co => co.cSharpDataType == "DateTime"))
            {
                classText.AppendLine("import android.annotation.SuppressLint");
            }
            classText.AppendLine("import com.google.gson.GsonBuilder");
            classText.AppendLine("import com.google.gson.reflect.TypeToken");
            classText.AppendLine($"import com.{_nameSpace}.BuildConfig");
            classText.AppendLine($"import com.{_nameSpace}.data.model.{table.Name}");

            classText.AppendLine("import kotlinx.coroutines.Dispatchers");
            classText.AppendLine("import kotlinx.coroutines.withContext");
            classText.AppendLine("import java.io.IOException");
            classText.AppendLine("import java.net.HttpURLConnection");

            if (table.Columns.Any(col => col.cSharpDataType == "DateTime"))
            {
                classText.AppendLine("import java.text.SimpleDateFormat");
                classText.AppendLine("import java.util.Date");
            }
            classText.AppendLine("import okhttp3.Response");

            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}DataSource {{");

            if (table.Columns.Any(col => col.cSharpDataType == "DateTime"))
            {
                classText.AppendLine("\t@SuppressLint(\"SimpleDateFormat\")");
            }
            classText.AppendLine($"\tsuspend fun save{table.Name}({Library.TableColumnsCode(table, Library.KotlinParameterNameAndType, false, true, true)}): Result<{table.Name}> {{");
            classText.AppendLine($"\t\tvar {table.Name.Decapitalise()}Id: {table.PrimaryKey.kotlinDataType}");
            classText.AppendLine("\t\treturn try {");

            classText.AppendLine("\t\t\twithContext(Dispatchers.IO) {");

            if (table.Columns.Any(col => col.cSharpDataType == "DateTime"))
            {
                classText.AppendLine($"\t\t\t\tval isoDateFormat: SimpleDateFormat = SimpleDateFormat(com.{_nameSpace}.isoDateFormat)");
            }

            classText.AppendLine($"\t\t\t\tval {table.Name.Decapitalise()}InputString: String = \"{{{Library.TableColumnsCode(table, CreateJsonSerialization, includePrimaryKey: false, appendCommas:true, singleLine: true)}}}\"");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\t\t\tval {table.Name.Decapitalise()}Response: Response = HttpAccess.postSigned(\"https://${{BuildConfig.BASE_URL}}/api/{table.Name.ToLower()}\", {table.Name.Decapitalise()}InputString)");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\t\t\tif ({table.Name.Decapitalise()}Response.code() in 100..399) {{");
            if (table.PrimaryKey.kotlinDataType == "String")
            {
                classText.AppendLine($"\t\t\t\t\t{table.Name.Decapitalise()}Id = {table.Name.Decapitalise()}Response.body()?.string()?.trim('\"') ?: \"\"");
            }
            else
            {
                classText.AppendLine($"\t\t\t\t\t{table.Name.Decapitalise()}Id = ({table.Name.Decapitalise()}Response.body()?.string()?.trim('\"') ?: \"\") as {table.PrimaryKey.kotlinDataType} ");
            }
                classText.AppendLine($"\t\t\t\t\tval {table.Name.Decapitalise()}:{table.Name} = {table.Name} (");
            classText.AppendLine(Library.TableColumnsCode(table, ParameterForObjectCreation, includePrimaryKey: true, appendCommas: true, singleLine: false));
            classText.AppendLine(")");

            classText.AppendLine($"\t\t\t\t\tResult.Success({table.Name.Decapitalise()})");
            classText.AppendLine("\t\t\t\t} else {");
            classText.AppendLine($"\t\t\t\t\tResult.Error(IOException(\"HTTP error code: ${{{table.Name.Decapitalise()}Response.code()}}\"))");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t} catch (e: Exception) {");
            classText.AppendLine($"\t\t\tResult.Error(IOException(\"Error getting {table.Name}\", e))");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");




            classText.AppendLine($"\tsuspend fun getAll{table.Name}(): Result<List<{table.Name}>> {{");
            classText.AppendLine("\t\treturn try {");

            classText.AppendLine("\t\t\twithContext(Dispatchers.IO) {");
            classText.AppendLine($"\t\t\t\tval response = HttpAccess.get(\"https://${{BuildConfig.BASE_URL}}/api/{table.Name.ToLower()}\")");
            classText.AppendLine("\t\t\t\tif (response.code() == HttpURLConnection.HTTP_OK) {");

            classText.AppendLine($"\t\t\t\t\tval builder = GsonBuilder()");
            classText.AppendLine($"\t\t\t\t\tval gson = builder.create()");
            classText.AppendLine($"\t\t\t\t\tval {table.Name.Decapitalise()}ListType = object : TypeToken<List<{table.Name}>>() {{}}.type");
            classText.AppendLine($"\t\t\t\t\tval {table.Name.Decapitalise()}s: List<{table.Name}> = gson.fromJson<List<{table.Name}>>(response.body()");
            classText.AppendLine($"\t\t\t\t\t\t?.string() ?: \"\", {table.Name.Decapitalise()}ListType)");

            classText.AppendLine($"\t\t\t\t\tResult.Success({table.Name.Decapitalise()}s)");


            classText.AppendLine("\t\t\t\t} else {");
            classText.AppendLine("\t\t\t\t\tResult.Error(IOException(\"HTTP error code: ${response.code()}\"))");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t} catch (e: Exception) {");
            classText.AppendLine($"\t\t\tResult.Error(IOException(\"Error getting {table.Name} data\", e))");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");

            classText.AppendLine($"\tsuspend fun get{table.Name}({table.Name.Decapitalise()}Id: {table.PrimaryKey.kotlinDataType}): Result<{table.Name}> {{");
            classText.AppendLine("\t\treturn try {");
            classText.AppendLine("\t\t\twithContext(Dispatchers.IO) {");
            classText.AppendLine($"\t\t\t\tval response = HttpAccess.get(\"https://${{BuildConfig.BASE_URL}}/api/{table.Name.ToLower()}/${table.Name.Decapitalise()}Id\")");
            classText.AppendLine("\t\t\t\tif (response.code() == HttpURLConnection.HTTP_OK) {");

            classText.AppendLine($"\t\t\t\t\tval builder = GsonBuilder()");
            classText.AppendLine($"\t\t\t\t\tval gson = builder.create()");
            classText.AppendLine($"\t\t\t\t\tval {table.Name.Decapitalise()}Type = object : TypeToken<{table.Name}>() {{}}.type");
            classText.AppendLine($"\t\t\t\t\tval {table.Name.Decapitalise()}: {table.Name} = gson.fromJson<{table.Name}>(response.body()");
            classText.AppendLine($"\t\t\t\t\t\t?.string() ?: \"\", {table.Name.Decapitalise()}Type)");

            classText.AppendLine($"Result.Success({table.Name.Decapitalise()})");


            classText.AppendLine("\t\t\t\t} else {");
            classText.AppendLine($"\t\t\tResult.Error(IOException(\"HTTP error code: ${{response.code()}}\"))");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t} catch (e: Exception) {");
            classText.AppendLine($"\t\t\tResult.Error(IOException(\"Error getting {table.Name} data\", e))");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");

            classText.AppendLine("}");
        }
                

        private string CreateJsonSerialization(SQLTableColumn column) 
        {
            return $"\\\"{column.Name.Decapitalise()}\\\": \\\"${{{(column.kotlinDataType == "Date" ? $"isoDateFormat.format({column.Name.Decapitalise()})" : column.Name.Decapitalise())}}}\\\"";
        }

        private string ParameterForObjectCreation(SQLTableColumn column)
        {
            string valueToBeSetFrom;
            if (column.PrimaryKey)
            {
                valueToBeSetFrom = column.TableName.Decapitalise() + "Id";
            }
            else
            {
                valueToBeSetFrom = column.Name.Decapitalise();
            }

            return $"{column.Name.Decapitalise()} = {valueToBeSetFrom}";
        }
    }
}
