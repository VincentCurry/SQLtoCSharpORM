using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeGenerator
{
    public class IOCDataClassesGenerator : Generator
    {
        public IOCDataClassesGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileNameSuffix = "";
        }
        public void GenerateRepositoryInterface()
        {
            StringBuilder repositoryText = new StringBuilder();

            repositoryText.AppendLine("using System.Collections.Generic;");
            repositoryText.AppendLine("");
            repositoryText.AppendLine($"namespace {_nameSpace}.Repository");
            repositoryText.AppendLine("{");
            repositoryText.AppendLine("\tpublic interface IRepository<T, I>");
            repositoryText.AppendLine("\t{");
            repositoryText.AppendLine("\t\tList<T> GetAll();");
            repositoryText.AppendLine("\t\tT GetByID(I id);");
            repositoryText.AppendLine("\t\tvoid Save(T item);");
            repositoryText.AppendLine("\t\tvoid Delete(I id);");
            repositoryText.AppendLine("\t}");
            repositoryText.AppendLine("}");

            TextWriter repoWriter = File.CreateText($"{_destinationFolder}IRepository.cs");

            repoWriter.Write(repositoryText.ToString());

            repoWriter.Close();

        }

        internal override void GenerateFilePerTable(SQLTable table)
        {

            classText.AppendLine("using System;");
            classText.AppendLine("using System.ComponentModel.DataAnnotations;");
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
                if (!column.Nullable && !column.PrimaryKey)
                    classText.AppendLine($"\t\t[Required(ErrorMessage= \"{column.Name} is required\")]");

                if (column.cSharpDataType == "string")
                    classText.AppendLine($"\t\t[MaxLength(length:{column.MaximumLength}, ErrorMessage = \"{column.Name} cannot be longer than {column.MaximumLength} characters\")]");

                classText.AppendLine($"\t\tpublic {column.cSharpDataType}{(column.Nullable && column.cSharpDataType != "string" ? "?" : "")} {column.Name} {{ get; set; }}");
            }

            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }
    }
}
