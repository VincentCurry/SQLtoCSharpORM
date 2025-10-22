using System.Collections.Generic;

namespace CodeGenerator
{
    public abstract class GeneratorFromForeignKeys : Generator
    {
        public GeneratorFromForeignKeys(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        { }

        public GeneratorFromForeignKeys(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        { }

        internal abstract void GenerateFilePerForeignKey(SQLForeignKeyRelation foreignKeyRelation, string className);

        public void GenerateClassesForForeignKeys()
        {
            foreach (SQLTable sQLTable in _sQLTables)
            {
                foreach (SQLForeignKeyRelation foreignKeyRelation in sQLForeignKeyRelationsForTable(sQLTable))
                {
                    string foreignKeyClassName = $"{foreignKeyRelation.ReferencedTableColumn.TableName}With{foreignKeyRelation.ParentTableColum.TableName}s";

                    classText = new System.Text.StringBuilder();
                    GenerateFilePerForeignKey(foreignKeyRelation, foreignKeyClassName);

                    System.IO.TextWriter writer = System.IO.File.CreateText($"{_destinationFolder}{foreignKeyClassName}{fileNameSuffix}.{fileSuffix}");

                    writer.Write(classText.ToString());

                    writer.Close();
                
                }
            }
        }
    }
}
