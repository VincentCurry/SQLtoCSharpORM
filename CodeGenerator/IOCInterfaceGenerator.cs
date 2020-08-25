﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CodeGenerator
{
    public class IOCInterfaceGenerator
    {
        public void GenerateClasses(List<SQLTable> tables, string destinationFolder, string nameSpace)
        {
            foreach (SQLTable table in tables)
            {
                StringBuilder classText = new StringBuilder();

                classText.AppendLine("using System;");
                classText.AppendLine(Environment.NewLine);

                classText.AppendLine("namespace " + nameSpace);
                classText.AppendLine("{");
                classText.AppendLine("public interface I" + table.Name);
                classText.AppendLine("{");

                foreach (SQLTableColumn column in table.Columns)
                {
                    classText.AppendLine($"{column.cSharpDataType} {column.Name} {{get; set; }}");
                }

                classText.AppendLine("}");
                classText.AppendLine("}");

                TextWriter writer = File.CreateText(destinationFolder + table.Name + ".cs");

                writer.Write(classText.ToString());

                writer.Close();

            }
        }
    }
}
