using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    public class AndroidViewModelFactoryGenerator : Generator
    {
        public AndroidViewModelFactoryGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "ViewModelFactory";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.Decapitalise()}");
            classText.Append(Environment.NewLine);

            classText.AppendLine("import androidx.lifecycle.ViewModel");
            classText.AppendLine("import androidx.lifecycle.ViewModelProvider");
            classText.AppendLine($"import com.{_nameSpace}.data.{table.Name}DataSource");
            classText.AppendLine($"import com.{_nameSpace}.data.{table.Name}Repository");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}ViewModelFactory : ViewModelProvider.Factory {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t@Suppress(\"UNCHECKED_CAST\")");
            classText.AppendLine("\toverride fun <T : ViewModel> create(modelClass: Class<T>): T {");
            classText.AppendLine($"\t\tif (modelClass.isAssignableFrom({table.Name}ViewModel::class.java)) {{");
            classText.AppendLine($"\t\t\treturn {table.Name}ViewModel(");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Repository = {table.Name}Repository(");
            classText.AppendLine($"\t\t\t\t\tdataSource = {table.Name}DataSource()");
            classText.AppendLine("\t\t\t\t)");
            classText.AppendLine("\t\t\t) as T");
            classText.AppendLine("\t\t}");
            classText.AppendLine("\t\tthrow IllegalArgumentException(\"Unknown ViewModel class\")");
            classText.AppendLine("\t}");
            classText.AppendLine("}");


        }
    }
}
