using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftFormStateGenerator : Generator
    {
        public SwiftFormStateGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormState";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {

            classText.AppendLine($"struct {table.Name}FormState {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine(Library.TableColumnsCode(table.Columns, FormStateErrorPropertyDefinition, includePrimaryKey: false, appendCommas: true, singleLine: false));
            classText.AppendLine("}");
        }

        private string FormStateErrorPropertyDefinition(SQLTableColumn column)
        {
            if (column.IsToBeValidated)
            {
                return $"\tlet {column.Name.DecapitaliseAndUpdateSwiftKeywords()}: {column.iosDataType} = {column.iosDefaultValue} \r\n\tvar {column.Name.DecapitaliseAndUpdateSwiftKeywords()}Error: String? = nil";
            }
            else
            {
                return $"\tlet {column.Name.DecapitaliseAndUpdateSwiftKeywords()}: {column.iosDataType} = {column.iosDefaultValue}";
            }
        }

    }
}