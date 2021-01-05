using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidDataAccessObjectGenerator : Generator
    {

        public AndroidDataAccessObjectGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            filePrefix = "Dao";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            string className = table.Name;
            string primaryKey = table.PrimaryKey.Name;
            string objectName = Library.LowerFirstCharacter(table.Name);

            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("import androidx.room.Dao");
            classText.AppendLine("import androidx.room.Delete");
            classText.AppendLine("import androidx.room.Insert");
            classText.AppendLine("import androidx.room.Query");
            classText.AppendLine("import kotlinx.coroutines.flow.Flow");

            classText.AppendLine(Environment.NewLine);
            classText.AppendLine("@Dao");
            classText.AppendLine($"interface {className}Dao {{");
            classText.AppendLine($"\t@Query(\"SELECT * FROM {className}\")");
            classText.AppendLine($"\tfun getAll(): Flow<List<{className}>>");

            classText.AppendLine($"\t@Query(\"SELECT * FROM {className} WHERE {primaryKey} = :{primaryKey}\")");
            classText.AppendLine($"\tfun getById({primaryKey}: String): Flow<{className}>");

            classText.AppendLine("\t@Insert");
            classText.AppendLine($"\tsuspend fun insertAll(vararg {objectName}s: {className})");

            classText.AppendLine("\t@Insert");
            classText.AppendLine($"\tsuspend fun insert(vararg {objectName}: {className})");

            classText.AppendLine("\t@Delete");
            classText.AppendLine($"\tsuspend fun delete({objectName}: {className})");
            classText.AppendLine("}");
    }
    }
}
