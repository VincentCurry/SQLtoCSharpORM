using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidSaveViewModelGenerator : Generator
    {
        public AndroidSaveViewModelGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "ViewModel";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {

            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.ToLower()}");
            classText.Append(Environment.NewLine);

            classText.AppendLine("import androidx.lifecycle.LiveData");
            classText.AppendLine("import androidx.lifecycle.MutableLiveData");
            classText.AppendLine("import androidx.lifecycle.ViewModel");
            classText.AppendLine("import androidx.lifecycle.viewModelScope");
            classText.AppendLine($"import com.{_nameSpace}.R");
            classText.AppendLine($"import com.{_nameSpace}.data.{table.Name}Repository");
            classText.AppendLine($"import com.{_nameSpace}.data.Result");
            classText.AppendLine("import kotlinx.coroutines.flow.MutableStateFlow");
            classText.AppendLine("import kotlinx.coroutines.launch");
            if (table.Columns.Any(col => col.cSharpDataType == "DateTime"))
            {
                classText.AppendLine("import java.util.Date");
            }
            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}ViewModel(private val {table.Name.Decapitalise()}Repository: {table.Name}Repository) : ViewModel() {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tprivate val _{table.Name.Decapitalise()}Form = MutableLiveData<{table.Name}FormState>()");
            classText.AppendLine($"\tval {table.Name.Decapitalise()}FormState: LiveData<{table.Name}FormState> = _{table.Name.Decapitalise()}Form");
            classText.AppendLine($"\tprivate val _formState = MutableStateFlow({table.Name}FormState())");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tprivate val _{table.Name.Decapitalise()}Result = MutableLiveData<{table.Name}Result>()");
            classText.AppendLine($"\tval {table.Name.Decapitalise()}Result: LiveData<{table.Name}Result> = _{table.Name.Decapitalise()}Result");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tfun save{table.Name}({Library.TableColumnsCode(table, Library.KotlinParameterNameAndType, false, true, true)}) {{");
            classText.AppendLine("\t\tviewModelScope.launch {");
            classText.Append($"\t\t\tval result = {table.Name.Decapitalise()}Repository.save{table.Name}(");
            classText.Append($"{Library.TableColumnsCode(table, Library.ColumnNameDecapitalised, false, true, true)}");
            classText.AppendLine(")");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t\t\tif (result is Result.Success) {");
            classText.AppendLine($"\t\t\t\t_{table.Name.Decapitalise()}Result.value =");
            classText.AppendLine($"\t\t\t\t\t{table.Name}Result(success = result.data)");
            classText.AppendLine("\t\t\t} else {");
            string resourceKeySaveFailed = $"save_{table.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_failed";
            classText.AppendLine($"\t\t\t\t_{table.Name.Decapitalise()}Result.value = {table.Name}Result(error = R.string.{resourceKeySaveFailed})");
            Library.WriteToKotlinStringsFile(resourceKeySaveFailed, $"Saving {table.Name} failed", _destinationFolder);

            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tfun validateField(field: {table.Name}FormField<*>, value: String) {{");
            classText.AppendLine($"\t\tval current = _{table.Name.Decapitalise()}Form.value ?: {table.Name}FormState()");
            classText.AppendLine($"\t\t_{table.Name.Decapitalise()}Form.value = when (field) {{");
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), ValidateField, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tfun validateAll(values: Map<{table.Name}FormField<*>, String>): Boolean {{");
            classText.AppendLine("\t\tvar newState = _formState.value");
            classText.AppendLine("\t\tfor ((field, value) in values) {");
            classText.AppendLine("");
            classText.AppendLine("\t\t\tnewState = when (field) {");
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), ValidateAllField, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");
            classText.AppendLine("");
            classText.AppendLine("\t\t_formState.value = newState");
            classText.AppendLine("\t\treturn newState.allErrors().all { it == null }");
            classText.AppendLine("\t}");


            classText.AppendLine("}");

        }
        private string ValidateField(SQLTableColumn column)
        {
            return $"\t\t\tis {column.TableName}FormField.{column.Name}  -> current.copy({column.Name.Decapitalise()}Error = field.validator(value as String))";
        }

        private string ValidateAllField(SQLTableColumn column)
        {
            return $"\t\t\t\tis {column.TableName}FormField.{column.Name} -> newState.copy({column.Name.Decapitalise()}Error = field.validator(value as String))";
        }
    }
}
