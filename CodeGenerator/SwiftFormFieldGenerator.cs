using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftFormFieldGenerator : Generator
    {
        public SwiftFormFieldGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormFields";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"struct {table.Name}FormFields: View {{");
            classText.AppendLine($"\t@Binding var state: {table.Name}FormState");
            classText.AppendLine();
            classText.AppendLine("\tvar body: some View {");
            classText.AppendLine("\t\tForm {");


            classText.AppendLine(Library.TableColumnsCode(table, FormFieldGenerator, includePrimaryKey: false, appendCommas: false, singleLine: false));

            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }

        private string FormFieldGenerator(SQLTableColumn column)
        {

            if (column.iosDataType.StartsWith("String"))
            {
                return($"\t\t\tTextField(\"{column.Name}\", text: $state.{column.Name.DecapitaliseAndUpdateSwiftKeywords()})");
            }
            else if (column.iosDataType.StartsWith("Int"))
            {
                return($"\t\t\tTextField(\"{column.Name}\", value: $state.{column.Name.DecapitaliseAndUpdateSwiftKeywords()}, format: .number)");
            }
            else if (column.iosDataType == "Bool")
            {
                return($"\t\t\tToggle(\"{column.Name}\", isOn: $state.{column.Name.DecapitaliseAndUpdateSwiftKeywords()})");
            }
            else if (column.iosDataType == "Date")
            {
                return($"\t\t\tDatePicker(\"{column.Name}\", selection: $state.{column.Name.DecapitaliseAndUpdateSwiftKeywords()})");
            }
            else if (column.iosDataType == "Float")
            {
                return ($"\t\t\tTextField(\"{column.Name}\", value: $state.{column.Name.DecapitaliseAndUpdateSwiftKeywords()}, format: .number)");
            }
            else if (column.iosDataType == "Double")
            {
                return ($"\t\t\tTextField(\"{column.Name}\", value: $state.{column.Name.DecapitaliseAndUpdateSwiftKeywords()}, format: .number)");
            }

            throw new Exception("Data type not handled");
        }
    }
}
