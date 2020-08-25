using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CodeGenerator
{
    public class IOCInterfaceGenerator
    {
        public void GenerateClasses(List<SQLTable> tables, string destinationFolder, string nameSpace)
        {
            StringBuilder repositoryText = new StringBuilder();
            
            repositoryText.AppendLine("using System;");
            repositoryText.AppendLine("using System.Collections.Generic;");
            repositoryText.AppendLine("");
            repositoryText.AppendLine("namespace " + nameSpace);
            repositoryText.AppendLine("{");
            repositoryText.AppendLine("\tpublic interface IRepository<T>");
            repositoryText.AppendLine("\t{");
            repositoryText.AppendLine("\t\tList<T> GetAll();");
            repositoryText.AppendLine("\t\tT GetByID(Guid id);");
            repositoryText.AppendLine("\t\tvoid Save(T item);");
            repositoryText.AppendLine("\t\tvoid Delete(Guid id);");
            repositoryText.AppendLine("\t}");
            repositoryText.AppendLine("}");

            TextWriter repoWriter = File.CreateText(destinationFolder + "IRepository.cs");

            repoWriter.Write(repositoryText.ToString());

            repoWriter.Close();

            foreach (SQLTable table in tables)
            {
                StringBuilder classText = new StringBuilder();

                classText.AppendLine("using System;");
                classText.AppendLine(Environment.NewLine);

                classText.AppendLine("namespace " + nameSpace);
                classText.AppendLine("{");
                classText.AppendLine("\tpublic interface I" + table.Name);
                classText.AppendLine("\t{");

                foreach (SQLTableColumn column in table.Columns)
                {
                    classText.AppendLine($"\t\t{column.cSharpDataType} {column.Name} {{get; set; }}");
                }

                classText.AppendLine("\t}");
                classText.AppendLine("}");

                TextWriter writer = File.CreateText(destinationFolder + table.Name + ".cs");

                writer.Write(classText.ToString());

                writer.Close();

            }
        }
    }
}
