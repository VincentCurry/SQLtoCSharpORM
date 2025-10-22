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
            fileNameSuffix = "Repository";
            fileSuffix = "kt";
        }

        public new void GenerateClasses() 
        {
            string namespaceLoweredFirstCharacter = Library.LowerFirstCharacter(_nameSpace);
            classText = new StringBuilder();

            classText.AppendLine($"package com.example.{namespaceLoweredFirstCharacter}.database");
            classText.AppendLine("");

            classText.AppendLine($"import androidx.annotation.WorkerThread");
            classText.AppendLine($"import com.example.{namespaceLoweredFirstCharacter}.*");
            classText.AppendLine($"import com.example.{namespaceLoweredFirstCharacter}.dao.*");
            classText.AppendLine($"import com.example.{namespaceLoweredFirstCharacter}.entities.*");
                        classText.AppendLine($"import kotlinx.coroutines.flow.Flow");
            classText.AppendLine("");

            string constructorParameters = string.Join(", ", _sQLTables.Select(tab => $"private val {Library.LowerFirstCharacter(tab.Name)}Dao: {tab.Name}Dao"));
            string foreignKeyConstrutorParameters = string.Join(", ", _sQLTables.SelectMany(tab => sQLForeignKeyRelationsForTable(tab).Select(fk => $"private val {Library.LowerFirstCharacter(fk.AndroidClassName)}Dao: {fk.AndroidClassName}Dao")));

            classText.AppendLine($"class {_nameSpace}Repository ({constructorParameters}, {foreignKeyConstrutorParameters}){{");

            _sQLTables.ForEach(tab => classText.AppendLine($"\tval all{tab.Name}s: Flow<List<{tab.Name}>> = {Library.LowerFirstCharacter(tab.Name)}Dao.getAll()"));
            _sQLTables.ForEach(tab => sQLForeignKeyRelationsForTable(tab).ForEach(fk => classText.AppendLine(($"\tval all{fk.AndroidClassName}s: Flow<List<{fk.AndroidClassName}>> = {Library.LowerFirstCharacter(fk.AndroidClassName)}Dao.get{fk.ReferencedTableColumn.TableName}sWith{fk.ParentTableColum.TableName}s()"))));

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

            TextWriter writer = File.CreateText($"{_destinationFolder}{_nameSpace}{fileNameSuffix}.{fileSuffix}");

            writer.Write(classText.ToString());

            writer.Close();
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }
    }
}
