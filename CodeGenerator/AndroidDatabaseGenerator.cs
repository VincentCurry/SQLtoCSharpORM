using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    public class AndroidDatabaseGenerator : Generator
    {
        public AndroidDatabaseGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            filePrefix = "Database";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }

        public new void GenerateClasses()
        {
            classText = new StringBuilder();

            classText.AppendLine($"package com.example.receipt");
            classText.AppendLine("");

            classText.AppendLine($"import android.content.Context");
            classText.AppendLine($"import androidx.room.* ");
            classText.AppendLine($"import kotlinx.coroutines.CoroutineScope");
            classText.AppendLine("");

            string entities = string.Join(", ", _sQLTables.Select(tab => $"{tab.Name}::class"));
            classText.AppendLine($"@Database(version = 1, entities = [{entities} ])");
            classText.AppendLine($"@TypeConverters(Converters::class)");
            classText.AppendLine($"abstract class {_nameSpace}Database : RoomDatabase(){{");

            foreach (SQLTable table in _sQLTables)
            {
                classText.AppendLine($"\tabstract fun {Library.LowerFirstCharacter(table.Name)}Dao(): {table.Name}Dao");
            }

            classText.AppendLine("");

            classText.AppendLine($"\tcompanion object{{");
            classText.AppendLine($"\t\t@Volatile");
            classText.AppendLine($"\t\tprivate var INSTANCE: ReceiptDatabase?=null");
            classText.AppendLine("");

            classText.AppendLine($"\t\tfun getDatabase(context: Context, scope: CoroutineScope): ReceiptDatabase{{");
            classText.AppendLine($"\t\t\treturn INSTANCE?: synchronized(this){{");
            classText.AppendLine($"\t\t\t\tval instance = Room.databaseBuilder(");
            classText.AppendLine($"\t\t\t\t\tcontext.applicationContext, ");
            classText.AppendLine($"\t\t\t\t\tReceiptDatabase::class.java,");
            classText.AppendLine($"\t\t\t\t\t\"receipt_database\"");
            classText.AppendLine($"\t\t\t\t).build()");
            classText.AppendLine($"\t\t\t\tINSTANCE = instance");
            classText.AppendLine($"\t\t\t\tinstance");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"}}");

            TextWriter writer = File.CreateText($"{_destinationFolder}{_nameSpace}{filePrefix}.{fileSuffix}");

            writer.Write(classText.ToString());

            writer.Close();
        }
    }
}
