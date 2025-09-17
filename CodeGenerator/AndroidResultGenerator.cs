using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidResultGenerator : Generator
    {
        public AndroidResultGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "Result";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.Decapitalise()}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"import com.{_nameSpace}.data.model.{table.Name}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"data class {table.Name}Result(");
            classText.AppendLine($"\tval success: String? = null,");
            classText.AppendLine("\tval error: Int? = null");
            classText.Append($")");
        }
    }
}
