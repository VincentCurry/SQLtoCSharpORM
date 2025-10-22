using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidDataAccessObjectGenerator : GeneratorFromForeignKeys
    {

        public AndroidDataAccessObjectGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "Dao";
        }

        internal override void GenerateFilePerForeignKey(SQLForeignKeyRelation foreignKeyRelation, string className)
        {
            string primaryTableName = foreignKeyRelation.ParentTableColum.TableName;

            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}.dao");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"import androidx.room.Dao");
            classText.AppendLine($"import androidx.room.Query");
            classText.AppendLine($"import androidx.room.Transaction");
            classText.AppendLine($"import com.example.receipt.entities.{className}");
            classText.AppendLine($"import kotlinx.coroutines.flow.Flow");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"@Dao");
            classText.AppendLine($"interface {className}DAO {{");
            classText.AppendLine($"\t@Transaction");
            classText.AppendLine($"\t@Query(\"SELECT * FROM {foreignKeyRelation.ReferencedTableColumn.TableName}\")");
            classText.AppendLine($"\tfun get{foreignKeyRelation.ReferencedTableColumn.TableName}sWith{foreignKeyRelation.ParentTableColum.TableName}s(): Flow<List<{primaryTableName}s>>");
            classText.AppendLine($"}}");
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string className = table.Name;
            string primaryKey = table.PrimaryKey.Name;
            string objectName = Library.LowerFirstCharacter(table.Name);

            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}.dao");
            classText.AppendLine("");

            classText.AppendLine("import androidx.room.Dao");
            classText.AppendLine("import androidx.room.Delete");
            classText.AppendLine("import androidx.room.Insert");
            classText.AppendLine("import androidx.room.Query");
            classText.AppendLine("import kotlinx.coroutines.flow.Flow");

            classText.AppendLine("");
            classText.AppendLine("@Dao");
            classText.AppendLine($"interface {className}Dao {{");
            classText.AppendLine($"\t@Query(\"SELECT * FROM {className}\")");
            classText.AppendLine($"\tfun getAll(): Flow<List<{className}>>");

            classText.AppendLine($"\t@Query(\"SELECT * FROM {className} WHERE {primaryKey} = :{primaryKey}\")");
            classText.AppendLine($"\tfun getById({primaryKey}: String): Flow<{className}>");

            classText.AppendLine("\t@Insert");
            classText.AppendLine($"\tfun insertAll(vararg {objectName}s: {className})");

            classText.AppendLine("\t@Insert");
            classText.AppendLine($"\tfun insert(vararg {objectName}: {className})");

            classText.AppendLine("\t@Delete");
            classText.AppendLine($"\tfun delete({objectName}: {className})");
            classText.AppendLine("}");
        }
    }
}
