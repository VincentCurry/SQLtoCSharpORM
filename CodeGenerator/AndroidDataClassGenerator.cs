using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidDataClassGenerator : GeneratorFromForeignKeys
    {
        const int removeLastCommaAndCarriageReturn = 3;
        public AndroidDataClassGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}.entities");
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

        
        internal override void GenerateFilePerForeignKey(SQLForeignKeyRelation foreignKeyRelation, string className)
        {

            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}.entities");
                    classText.AppendLine(Environment.NewLine);

                    classText.AppendLine($"import androidx.room.Embedded");
                    classText.AppendLine($"import androidx.room.Relation");
                    classText.AppendLine($"import com.example.receipt.{foreignKeyRelation.ReferencedTableColumn.TableName}");
                    classText.AppendLine($"import com.example.receipt.{foreignKeyRelation.ParentTableColum.TableName}");
                    classText.AppendLine(Environment.NewLine);
            
                    classText.AppendLine($"data class {className} (");
                    classText.AppendLine($"\t@Embedded val {Library.LowerFirstCharacter(foreignKeyRelation.ReferencedTableColumn.TableName)}: {foreignKeyRelation.ReferencedTableColumn.TableName},");

                    classText.AppendLine($"\t@Relation(");

                    classText.AppendLine($"\t\tparentColumn = \"{Library.LowerFirstCharacter(foreignKeyRelation.ReferencedTableColumn.Name)}\",");
                    classText.AppendLine($"\t\tentityColumn = \"{Library.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals(foreignKeyRelation.ParentTableColum.Name)}\"");
                    classText.AppendLine($"\t)");
                    classText.AppendLine($"\tval {Library.LowerFirstCharacter(foreignKeyRelation.ParentTableColum.TableName)}: List <{foreignKeyRelation.ParentTableColum.TableName}>");
                    classText.AppendLine($")");
        }
    }
}