using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CodeGenerator
{
    public class IOCDataClassesGenerator : Generator
    {
        public IOCDataClassesGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        { }
        public void GenerateRepositoryInterface()
        {
            StringBuilder repositoryText = new StringBuilder();

            repositoryText.AppendLine("using System;");
            repositoryText.AppendLine("using System.Collections.Generic;");
            repositoryText.AppendLine("");
            repositoryText.AppendLine($"namespace {_nameSpace}.Repository");
            repositoryText.AppendLine("{");
            repositoryText.AppendLine("\tpublic interface IRepository<T>");
            repositoryText.AppendLine("\t{");
            repositoryText.AppendLine("\t\tList<T> GetAll();");
            repositoryText.AppendLine("\t\tT GetByID(Guid id);");
            repositoryText.AppendLine("\t\tvoid Save(T item);");
            repositoryText.AppendLine("\t\tvoid Delete(Guid id);");
            repositoryText.AppendLine("\t}");
            repositoryText.AppendLine("}");

            TextWriter repoWriter = File.CreateText($"{_destinationFolder}IRepository.cs");

            repoWriter.Write(repositoryText.ToString());

            repoWriter.Close();

        }

        internal override void GenerateFilePerTable(SQLTable table)
        {

            StringBuilder classText = new StringBuilder();

            classText.AppendLine("using System;");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"namespace {_nameSpace}.Repository");
            classText.AppendLine("{");
            classText.AppendLine($"\tpublic class {table.Name}");
            classText.AppendLine("\t{");

            classText.AppendLine("#region Constructors");

            if (table.PrimaryKey.DataType == SQLDataTypes.uniqueIdentifier)
                classText.AppendLine("\t\tpublic " + table.Name + "(Guid id)");
            else
                classText.AppendLine("\t\tpublic " + table.Name + "(int id)");

            classText.AppendLine("\t\t{");
            classText.AppendLine($"\t\t\tthis.{table.PrimaryKey.Name} = id;");
            classText.AppendLine("\t\t}");

            classText.AppendLine("\t\tpublic " + table.Name + "()");
            classText.AppendLine("\t\t{ }");

            classText.AppendLine("#endregion");

            foreach (SQLTableColumn column in table.Columns)
            {
                classText.AppendLine($"\t\tpublic {column.cSharpDataType} {column.Name} {{ get; set; }}");
            }

            classText.AppendLine("\t}");
            classText.AppendLine("}");

            TextWriter writer = File.CreateText($"{_destinationFolder}{table.Name}.cs");

            writer.Write(classText.ToString());

            writer.Close();
        }
    }
}
