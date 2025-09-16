using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidFormStateGenerator : Generator
    {
        public AndroidFormStateGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "FormState";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.Decapitalise()}");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"data class {table.Name}FormState(");
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey && (!column.Nullable || column.kotlinDataType == kotlinDataTypes.strings))
                {
                    classText.AppendLine($"\tval {column.Name.Decapitalise()}Error: Int? = null,");
                }
            }
            classText.AppendLine(") {");
            classText.AppendLine($"\tfun allErrors(): List<Int?> = listOf({Library.TableColumnsCode(table.Columns.Where(co => !co.Nullable || co.kotlinDataType == kotlinDataTypes.strings), Library.ColumnNameDecapitalised, includePrimaryKey: false, appendCommas: true, singleLine: true)})");
            classText.AppendLine("}");

        }
    }
}
