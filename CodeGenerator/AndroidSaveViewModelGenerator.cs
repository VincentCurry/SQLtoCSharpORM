using System;
using System.Collections.Generic;

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

            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.Decapitalise()}");
            classText.Append(Environment.NewLine);

            classText.AppendLine("import android.util.Patterns");
            classText.AppendLine("import androidx.lifecycle.LiveData");
            classText.AppendLine("import androidx.lifecycle.MutableLiveData");
            classText.AppendLine("import androidx.lifecycle.ViewModel");
            classText.AppendLine("import androidx.lifecycle.viewModelScope");
            classText.AppendLine($"import com.{_nameSpace}.R");
            classText.AppendLine($"import com.{_nameSpace}.data.{table.Name}Repository");
            classText.AppendLine($"import com.{_nameSpace}.data.Result");
            classText.AppendLine($"import com.{_nameSpace}.data.SessionManager");
            classText.AppendLine($"import com.{_nameSpace}.data.model.LoggedInUser");
            classText.AppendLine("import kotlinx.coroutines.launch");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}ViewModel(private val {table.Name.Decapitalise()}Repository: {table.Name}Repository) : ViewModel() {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\tprivate val _loggedInUser = MutableLiveData<LoggedInUser?>()");
            classText.AppendLine($"\tprivate val _{table.Name.Decapitalise()}Form = MutableLiveData<{table.Name}FormState>()");
            classText.AppendLine($"\tval {table.Name.Decapitalise()}FormState: LiveData<{table.Name}FormState> = _{table.Name.Decapitalise()}Form");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tprivate val _{table.Name.Decapitalise()}Result = MutableLiveData<{table.Name}Result>()");
            classText.AppendLine($"\tval {table.Name.Decapitalise()}Result: LiveData<{table.Name}Result> = _{table.Name.Decapitalise()}Result");
            classText.AppendLine("\tval loggedInUser: LiveData<LoggedInUser?> = _loggedInUser");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tfun save{table.Name}({Library.TableColumnsCode(table, Library.ParameterNameAndType, false, true, true)}) {{");
            classText.AppendLine("\t\tviewModelScope.launch {");
            classText.Append($"\t\t\tval result = {table.Name.Decapitalise()}Repository.save{table.Name}(");
            classText.Append($"{Library.TableColumnsCode(table, Library.ColumnNameDecapitalised, false, true, true)}");
            classText.AppendLine(")");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t\t\tif (result is Result.Success) {");
            classText.AppendLine($"\t\t\t\t_{table.Name.Decapitalise()}Result.value =");
            classText.AppendLine($"\t\t\t\t\t{table.Name}Result(success = LoggedInUserView(displayName = result.data.displayName()))");
            classText.AppendLine("\t\t\t\t_loggedInUser.value = SessionManager.loggedInUser");
            classText.AppendLine("\t\t\t} else {");
            classText.AppendLine($"\t\t\t\t_{table.Name.Decapitalise()}Result.value = {table.Name}Result(error = R.string.{table.Name.Decapitalise()}_failed)");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tfun {table.Name.Decapitalise()}DataChanged({Library.TableColumnsCode(table, Library.ParameterNameAndType, false, true, true)}) {{");

            foreach(SQLTableColumn column in table.Columns)
            {
                if(column.IsToBeValidated)
                {
                    classText.AppendLine($"\t\tif (!is{column.Name}Valid({column.Name.Decapitalise()})) {{");
                    classText.AppendLine($"\t\t\t_{table.Name.Decapitalise()}Form.value = {table.Name}FormState({column.Name.Decapitalise()}Error = R.string.invalid_{column.Name.Decapitalise()})");
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
                    
                    classText.AppendLine("\t}");
                    classText.Append(Environment.NewLine);
                    /*classText.AppendLine("\t\treturn if (username.contains(\"@\")) {");
                    classText.AppendLine("\t\tPatterns.EMAIL_ADDRESS.matcher(username).matches()");
                    classText.AppendLine("\t\t} else {");
                    classText.AppendLine("\t\t\tusername.isNotBlank()");
                    classText.AppendLine("\t\t}");*/
                }
                /*classText.AppendLine("\t// A placeholder password validation check");
                classText.AppendLine("\tprivate fun isPasswordValid(password: String): Boolean {");
                classText.AppendLine("\t\treturn password.length > 5");
                classText.AppendLine("\t}");
                classText.AppendLine("}");
                classText.Append(Environment.NewLine);*/
            }

        }

    }
}
