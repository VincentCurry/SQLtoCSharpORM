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

            classText.AppendLine($"struct {table.Name} : Decodable, Identifiable");
            classText.AppendLine($" {{");

            foreach(SQLTableColumn column in table.Columns)
            {
                if (column.PrimaryKey)
                {
                    classText.AppendLine($"\tvar id: {column.iosDataType} {{ {Library.LowerFirstCharacter(column.Name)} }}");
                }

                string nullableOperator = column.Nullable ? "?" : "" ;
                classText.AppendLine($"\tvar {Library.LowerFirstCharacter(column.Name)}: {column.iosDataType}{nullableOperator}");
                
            }

            classText.AppendLine($"}}");
        }
    }
}
