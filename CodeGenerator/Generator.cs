using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    public abstract class Generator
    {
        internal string _destinationFolder;
        internal string _nameSpace;
        internal List<SQLTable> _sQLTables;
        internal StringBuilder classText;

        internal string newRegion = "#region ";
        internal string endRegion = "#endregion";
        internal string dataObjectClassIdentifier;
        internal string filePrefix;
        public Generator(List<SQLTable> tables, string destinationFolder, string nameSpace)
        {
            _destinationFolder = destinationFolder;
            _sQLTables = tables;
            _nameSpace = nameSpace;
        }

        public void GenerateClasses()
        {
            foreach (SQLTable table in _sQLTables)
            {
                dataObjectClassIdentifier = (table.Name == _nameSpace ? $"Repository.{table.Name}" : table.Name);

                classText = new StringBuilder();
                GenerateFilePerTable(table);
                
                TextWriter writer = File.CreateText($"{_destinationFolder}{table.Name}{filePrefix}.cs");

                writer.Write(classText.ToString());

                writer.Close();
            }
        }

        internal abstract void GenerateFilePerTable(SQLTable table);

        internal List<SQLForeignKeyRelation> sQLForeignKeyRelationsForTable(SQLTable table)
        {
            List<SQLForeignKeyRelation> foreignKeys = new List<SQLForeignKeyRelation>();

            foreach (SQLTable otherTable in _sQLTables.Where(tb => tb != table))
            {

                foreach (SQLTableColumn sQLTableColumn in otherTable.Columns)
                {
                    foreach (SQLForeignKeyRelation foreignKeyRelation in sQLTableColumn.ForeignKeys)
                    {
                        if (foreignKeyRelation.ParentTableColum.TableName == table.Name)
                        {
                            foreignKeys.Add(foreignKeyRelation);
                        }
                    }
                }
            }

            return foreignKeys;
        }

        internal string ClassName(string tableName)
        {
            return (tableName == _nameSpace ? $"Repository.{tableName}" : tableName);
        }
    }
}
