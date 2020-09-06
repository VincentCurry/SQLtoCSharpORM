using System.Collections.Generic;

namespace CodeGenerator
{
    public abstract class Generator
    {
        internal string _destinationFolder;
        internal string _nameSpace;
        internal List<SQLTable> _sQLTables;


        internal string newRegion = "#region ";
        internal string endRegion = "#endregion";
        internal string dataObjectClassIdentifier;
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
                GenerateFilePerTable(table);
            }
        }

        internal abstract void GenerateFilePerTable(SQLTable table);
    }
}
