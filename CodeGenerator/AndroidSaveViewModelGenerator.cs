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

            classText.AppendLine($"\tfun {table.Name.Decapitalise()}DataChanged({Library.TableColumnsCode(table, Library.KotlinParameterNameAndType, false, true, true)}) {{");

            bool firstColumn = true;
            foreach(SQLTableColumn column in table.Columns)
            {
                if(column.IsToBeValidated)
                {
                    if (firstColumn)
                    {
                        classText.AppendLine($"\t\tif (!is{column.Name}Valid({column.Name.Decapitalise()})) {{");
                    }
                    else
                    {
                        classText.AppendLine($"\t\t}} else if (!is{column.Name}Valid({column.Name.Decapitalise()})) {{");
                    }

                    string invalidParameterResourcesKey = $"invalid_{table.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}";
                    classText.AppendLine($"\t\t\t_{table.Name.Decapitalise()}Form.value = {table.Name}FormState({column.Name.Decapitalise()}Error = R.string.{invalidParameterResourcesKey})");

                    Library.WriteToKotlinStringsFile(invalidParameterResourcesKey, $"Problem with {column.Name}", _destinationFolder);

                    firstColumn = false;
                }
            }
            classText.AppendLine("\t\t} else {");
            classText.AppendLine($"\t\t\t_{table.Name.Decapitalise()}Form.value = {table.Name}FormState(isDataValid = true)");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            foreach (SQLTableColumn tableColumn in table.Columns)
            {
                if (tableColumn.IsToBeValidated)
                {
                    classText.AppendLine($"\tprivate fun is{tableColumn.Name}Valid({tableColumn.Name.Decapitalise()}: String): Boolean {{");
                    if(!tableColumn.Nullable && tableColumn.cSharpDataType == "string")
                    {

                        classText.AppendLine($"\t\t\treturn {tableColumn.Name.Decapitalise()}.isNotBlank() && {tableColumn.Name.Decapitalise()}.length < {tableColumn.MaximumLength}");
                    }
                    else if (!tableColumn.Nullable)
                    {
                         classText.AppendLine($"\t\t\treturn {tableColumn.Name.Decapitalise()}.isNotBlank()");
                    }
                    else if(tableColumn.cSharpDataType == "string")
                    {
                        classText.AppendLine($"\t\t\treturn {tableColumn.Name.Decapitalise()}.length < {tableColumn.MaximumLength}");
                    }

                    classText.AppendLine("}");
                    classText.Append(Environment.NewLine);
                }
            }
            classText.AppendLine("}");

        }

    }
}
