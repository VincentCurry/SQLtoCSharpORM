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
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(co => !co.Nullable || co.kotlinDataType == kotlinDataTypes.strings), FormStateErrorPropertyDefinition, includePrimaryKey: false, appendCommas: true, singleLine: false));
            classText.AppendLine(") {");
            classText.AppendLine($"\tfun allErrors(): List<Int?> = listOf({Library.TableColumnsCode(table.Columns.Where(co => !co.Nullable || co.kotlinDataType == kotlinDataTypes.strings), Library.ColumnNameDecapitalisedWithError, includePrimaryKey: false, appendCommas: true, singleLine: true)})");
            classText.AppendLine("}");

        }

        private string FormStateErrorPropertyDefinition(SQLTableColumn column)
        {
            return $"\tval {column.Name.Decapitalise()}Error: Int? = null";
        }
    }
}
