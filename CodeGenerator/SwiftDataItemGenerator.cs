using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftDataItemGenerator : Generator
    {
        public SwiftDataItemGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileSuffix = "swift";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"import Foundation");

            classText.AppendLine($"struct Location : Decodable, Identifiable");
            classText.AppendLine($" {{");

            foreach(SQLTableColumn column in table.Columns)
            {
                if (column.PrimaryKey)
                {
                    classText.AppendLine($"\tvar id: {column.iosDataType} {{ {Library.LowerFirstCharacter(column.Name)} }}");
                }
                else
                {
                    classText.AppendLine($"\tvar {Library.LowerFirstCharacter(column.Name)}: {column.iosDataType}");
                }
            }

            classText.AppendLine($"}}");
        }
    }
}
