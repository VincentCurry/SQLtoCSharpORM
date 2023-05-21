using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftSQLTableExtensionGenerator : Generator
    {
        public SwiftSQLTableExtensionGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileSuffix = "Extension.swift";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"extension {table.Name}: SQLTable {{");
            classText.AppendLine($"\tstatic var createStatement: String {{");
            classText.AppendLine($"\t\treturn \"\"\"");
            classText.AppendLine($"\t\tCREATE TABLE IF NOT EXISTS {table.Name}(");

            bool prependComma = false;

            foreach (SQLTableColumn column in table.Columns)
            {
                if (prependComma)
                    classText.Append("," + Environment.NewLine);

                classText.Append("\t\t\t" + column.Name + " " + column.sqlLiteDataType + column.SizeForSQLProcedureParameters);

                classText.Append(column.PrimaryKey ? " PRIMARY KEY" : "");

                classText.Append(column.Nullable ? "" : " NOT NULL");

                prependComma = true;
            }
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\t);");
            classText.AppendLine($"\t\t\"\"\"");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"}}");
        }
    }
}
