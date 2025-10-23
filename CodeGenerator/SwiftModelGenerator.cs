using CodeGenerator;
using System.Collections.Generic;

public class SwiftModelGenerator : Generator
{
    public SwiftModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
    {
        fileSuffix = "swift";
    }

    internal override void GenerateFilePerTable(SQLTable table)
    {
        classText.AppendLine($"struct {table.Name}: Identifiable, Codable {{");

        classText.AppendLine(Library.TableColumnsCode(table.Columns, ModelPropertyDefinition, includePrimaryKey: true, appendCommas: false, singleLine: false));

        classText.AppendLine("}");
    }

    private string ModelPropertyDefinition(SQLTableColumn column)
    {
        if(column.PrimaryKey)
        {
            return $"\tlet {column.Name.DecapitaliseAndUpdateSwiftKeywords()}: {column.iosDataType}";
        }
        else
        {
            return $"\tvar {column.Name.DecapitaliseAndUpdateSwiftKeywords()}: {column.iosDataType}";
        }
    }
}
