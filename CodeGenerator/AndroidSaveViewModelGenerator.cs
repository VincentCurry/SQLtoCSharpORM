using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            classText.Append(table.ContainsDateColumn ? $"import kotlinx.coroutines.flow.StateFlow{Environment.NewLine}" : "");
            classText.AppendLine("import kotlinx.coroutines.launch");
            classText.Append(table.ContainsDateColumn ? $"import java.time.LocalDate //this is for date columns, it might be deleteable{Environment.NewLine}" : "");
            classText.Append(table.ContainsDateColumn ? $"import java.time.LocalDateTime //this is for dateandtime columns, it might be deleteable{Environment.NewLine}" : "");
            classText.Append(table.ContainsDateColumn ? $"import java.util.Date{Environment.NewLine}" : "");
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

            classText.AppendLine(Library.TableColumnsCode(table, FieldsAsFilters, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.Append(Environment.NewLine);
            classText.AppendLine(Library.TableColumnsCode(table, ExposedAsImmutable, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.Append(Environment.NewLine);
            classText.AppendLine(Library.TableColumnsCode(table, UpdateFunctions, includePrimaryKey: false, appendCommas: false, singleLine: false));

            classText.AppendLine($"\tfun save{table.Name}() {{");
            classText.AppendLine("\t\tviewModelScope.launch {");
            classText.Append($"\t\t\tval result = {table.Name.Decapitalise()}Repository.save{table.Name}(");
            classText.Append(Library.TableColumnsCode(table, FieldForSaving, includePrimaryKey: false, appendCommas: true, singleLine: true));
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

            classText.AppendLine($"\tfun validateField(field: {table.Name}FormField<*>, value: String, touched: Boolean) {{");
            classText.AppendLine($"\t\tval current = _{table.Name.Decapitalise()}Form.value ?: {table.Name}FormState()");
            classText.AppendLine($"\t\t_{table.Name.Decapitalise()}Form.value = when (field) {{");
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), ValidateField, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tfun validateAll(): Boolean {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\tval fieldsToBeValidated = mapOf({Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), MapOfColumnsToBeValidated, includePrimaryKey: false, appendCommas: true, singleLine: true)})");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t\tvar newState = _formState.value");
            classText.AppendLine("\t\tfor ((field, value) in fieldsToBeValidated) {");
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
            StringBuilder validateField = new StringBuilder();

            validateField.AppendLine($"\t\t\tis {column.TableName}FormField.{column.Name}  -> current.copy(");
            validateField.AppendLine($"\t\t\t\t{column.Name.Decapitalise()}Error = if(current.{column.Name.Decapitalise()}Touched || touched) field.validator(value) else null,");
            validateField.AppendLine($"\t\t\t\t{column.Name.Decapitalise()}Touched = current.{column.Name.Decapitalise()}Touched || touched");
            validateField.Append("\t\t\t)");
            
            return validateField.ToString();
        }

        private string ValidateAllField(SQLTableColumn column)
        {
            return $"\t\t\t\tis {column.TableName}FormField.{column.Name} -> newState.copy({column.Name.Decapitalise()}Error = field.validator(value as String))";
        }

        private string FieldsAsFilters(SQLTableColumn column)
        {
            if (column.kotlinDataType == kotlinDataTypes.date)
            {
                StringBuilder dateFields = new StringBuilder();
                dateFields.AppendLine("//// Date and Time column fields. Delete if the field is a date only.");
                dateFields.AppendLine($"\tprivate val _{column.Name.Decapitalise()} = MutableStateFlow<LocalDateTime?>(null)");
                dateFields.AppendLine("// Date column fields. Delete if the field is a datetime");
                dateFields.AppendLine($"\tprivate val _{column.Name.Decapitalise()} = MutableStateFlow<LocalDate?>(null)");

                return dateFields.ToString();
            }
            else
            {
                return $"\tprivate val _{column.Name.Decapitalise()} = MutableStateFlow<{column.kotlinDataType}?>(null)";
            }
        }

        private string ExposedAsImmutable(SQLTableColumn column)
        {
            if (column.kotlinDataType == kotlinDataTypes.date)
            {
                StringBuilder dateFields = new StringBuilder();
                dateFields.AppendLine("//// Date and Time column fields. Delete if the field is a date only.");
                dateFields.AppendLine($"\tval {column.Name.Decapitalise()}: StateFlow<LocalDateTime?> = _{column.Name.Decapitalise()}");
                dateFields.AppendLine("// Date column fields. Delete if the field is a datetime");
                dateFields.AppendLine($"\t\tval {column.Name.Decapitalise()}: StateFlow<LocalDate?> = _{column.Name.Decapitalise()}");

                return dateFields.ToString();
            }
            else
            {
                return $"\t\tval {column.Name.Decapitalise()}: StateFlow<{column.kotlinDataType}?> = _{column.Name.Decapitalise()}";
            }
        }

        private string UpdateFunctions(SQLTableColumn column)
        {
            if(column.kotlinDataType == kotlinDataTypes.date)
            {
                StringBuilder dateTimeField = new StringBuilder();

                dateTimeField.AppendLine("//Date and time column fields. Delete if the field is a date");
                dateTimeField.AppendLine($"\tfun update{column.Name}(dateTime: LocalDateTime) {{");
                dateTimeField.AppendLine($"\t\t_{column.Name.Decapitalise()}.value = dateTime");
                dateTimeField.AppendLine($"\t}}");

                if (column.Nullable)
                {
                    dateTimeField.AppendLine($"\r\n\tfun set{column.Name}Enabled(enabled: Boolean) {{");
                    dateTimeField.AppendLine($"\t\tif (!enabled) {{");
                    dateTimeField.AppendLine($"\t\t\t_{column.Name.Decapitalise()}.value = null");
                    dateTimeField.AppendLine($"\t\t}}");
                    dateTimeField.AppendLine($"\t}}");
                }

                dateTimeField.AppendLine("//Date column fields. Delete if the field is a datetime");
                dateTimeField.AppendLine($"\tfun update{column.Name}(dateTime: LocalDate) {{");
                dateTimeField.AppendLine($"\t\t_{column.Name.Decapitalise()}.value = dateTime");
                dateTimeField.AppendLine($"\t}}");

                if (column.Nullable)
                {
                    dateTimeField.AppendLine($"\r\n\tfun set{column.Name}Enabled(enabled: Boolean) {{");
                    dateTimeField.AppendLine($"\t\tif (!enabled) {{");
                    dateTimeField.AppendLine($"\t\t\t_{column.Name.Decapitalise()}.value = null");
                    dateTimeField.AppendLine($"\t\t}}");
                    dateTimeField.AppendLine($"\t}}");
                }

                return dateTimeField.ToString();
            }
            else
            {
                if (column.IsToBeValidated) {
                    StringBuilder fieldUpdate = new StringBuilder();
                    fieldUpdate.AppendLine($"fun update{column.Name}({column.Name.Decapitalise()}: {column.kotlinDataType}) {{");
                    fieldUpdate.AppendLine($"_{column.Name.Decapitalise()}.value = {column.Name.Decapitalise()}");
                    fieldUpdate.AppendLine($"validateField({column.TableName}FormField.{column.Name}, {column.Name.Decapitalise()}, touched = false)");
                    fieldUpdate.AppendLine("}");

                    return fieldUpdate.ToString();
                }
                else
                {
                    return $"fun update{column.Name}({column.Name.Decapitalise()}: {column.kotlinDataType}) {{_{column.Name.Decapitalise()}.value = {column.Name.Decapitalise()}}}";
                }
            }
        }

        private string FieldForSaving(SQLTableColumn column)
        {
            if (column.kotlinDataType == kotlinDataTypes.date)
            {
                return $"Date.from(_{column.Name.Decapitalise()}.value?.atZone(ZoneId.systemDefault())?.toInstant())  Date.from(_{column.Name.Decapitalise()}.value?.atStartOfDay(ZoneId.systemDefault())?.toInstant())";
            }
            else
            {
                return $"_{column.Name.Decapitalise()}.value{(column.Nullable ? "" : ".toString()") }";
            }
        }

        private string MapOfColumnsToBeValidated(SQLTableColumn column)
        {
            return $"{column.TableName}FormField.{column.Name} to if (_{column.Name.Decapitalise()}.value == null ) \"\" else _{column.Name.Decapitalise()}.value";
        }
    }
}
