using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    public class AndroidRepositoryGenerator : Generator
    {
        public AndroidRepositoryGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            filePrefix = "Repository";
            fileSuffix = "kt";
        }

        public new void GenerateClasses() 
        {
            classText = new StringBuilder();

            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}");
            classText.AppendLine("");

            classText.AppendLine($"import androidx.annotation.WorkerThread");
            classText.AppendLine($"import kotlinx.coroutines.flow.Flow");
            classText.AppendLine("");

            string constructorParameters = string.Join(", ", _sQLTables.Select(tab => $"private val {Library.LowerFirstCharacter(tab.Name)}Dao: {tab.Name}Dao"));

            classText.AppendLine($"class {_nameSpace}Repository ({constructorParameters}){{");

            foreach (SQLTable table in _sQLTables)
            {
                classText.AppendLine($"\tval all{table.Name}s: Flow<List<{table.Name}>> = {Library.LowerFirstCharacter(table.Name)}Dao.getAll()");
            }

            foreach (SQLTable table1 in _sQLTables)
            {
                classText.AppendLine("");
                classText.AppendLine($"\t@Suppress(\"RedundantSuspendModifier\")");
                classText.AppendLine($"\t@WorkerThread");
                classText.AppendLine($"\tsuspend fun insert({Library.LowerFirstCharacter(table1.Name)}: {table1.Name}){{");
                classText.AppendLine($"\t\t{Library.LowerFirstCharacter(table1.Name)}Dao.insert({Library.LowerFirstCharacter(table1.Name)})");
                classText.AppendLine("\t}");
            }

            classText.AppendLine("}");

            TextWriter writer = File.CreateText($"{_destinationFolder}{_nameSpace}{filePrefix}.{fileSuffix}");

            writer.Write(classText.ToString());

            writer.Close();
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }
    }
}
