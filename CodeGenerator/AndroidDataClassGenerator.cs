using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidDataClassGenerator : Generator
    {
        public AndroidDataClassGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileSuffix = "kt";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine("package com.example.receipt");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("import androidx.room.ColumnInfo");
            classText.AppendLine("import androidx.room.Entity");
            classText.AppendLine("import androidx.room.PrimaryKey");
            classText.AppendLine("import java.util.Date");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("@Entity");
            classText.AppendLine($"data class {table.Name}(");

            classText.AppendLine($"@PrimaryKey val {Library.LowerFirstCharacter(table.PrimaryKey.Name)}: {table.PrimaryKey.kotlinDataType},");

            foreach(SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                    classText.AppendLine($"\t@ColumnInfo(name=\"{Library.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals(column.Name)}\") val {Library.LowerFirstCharacter(column.Name)}: {column.kotlinDataType}{(column.Nullable ? "?" : "")},");
            }

            classText.AppendLine(")");
        }
    }
}