using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidDataClassGenerator : Generator
    {
        const int removeLastCommaAndCarriageReturn = 3;
        public AndroidDataClassGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("import androidx.room.ColumnInfo");
            classText.AppendLine("import androidx.room.Entity");
            classText.AppendLine("import androidx.room.PrimaryKey");
            if(table.Columns.Any(col => col.DataType== SQLDataTypes.dateTime))
            {
                classText.AppendLine("import java.util.Date");
            }
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("@Entity");
            classText.AppendLine($"data class {table.Name}(");

            classText.AppendLine($"\t@PrimaryKey val {Library.LowerFirstCharacter(table.PrimaryKey.Name)}: {table.PrimaryKey.kotlinDataType},");

            foreach(SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                    classText.AppendLine($"\t@ColumnInfo(name=\"{Library.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals(column.Name)}\") val {Library.LowerFirstCharacter(column.Name)}: {column.kotlinDataType}{(column.Nullable ? "?" : "")},");
            }

            classText.Length -= removeLastCommaAndCarriageReturn;

            classText.AppendLine("");

            classText.AppendLine(")");
        }
    }
}