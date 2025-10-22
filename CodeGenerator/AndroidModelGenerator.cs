using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidModelGenerator : Generator
    {
        const int removeLastCommaAndCarriageReturn = 3;
        public AndroidModelGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{Library.LowerFirstCharacter(_nameSpace)}.data.model");
            classText.AppendLine(Environment.NewLine);

            if (table.Columns.Any(col => col.DataType == SQLDataTypes.dateTime))
            {
                classText.AppendLine("import java.util.Date");
            }
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"data class {table.Name}(");



            foreach (SQLTableColumn column in table.Columns)
            {
                    classText.AppendLine($"\tval {Library.LowerFirstCharacter(column.Name)}: {column.kotlinDataType}{(column.Nullable ? "?" : "")},");
            }

            classText.Length -= removeLastCommaAndCarriageReturn;

            classText.AppendLine("");

            classText.AppendLine(")");
        }
    }
}
