using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class AndroidFormFieldGenerator : Generator
    {
        public AndroidFormFieldGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "FormField";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.Decapitalise()}");
            classText.Append(Environment.NewLine);
            classText.AppendLine($"import com.{_nameSpace}.R");
            classText.Append(Environment.NewLine);
            classText.AppendLine($"typealias Validator<T> = (T) -> Int?");
            classText.Append(Environment.NewLine);
            classText.AppendLine($"sealed class {table.Name}FormField<T> (val validator: Validator<T>) {{");
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), ValidationFunction, false, appendCommas: false, singleLine: false));
            classText.AppendLine($"\tcompanion object {{");
            classText.AppendLine($"\t\t// Handy list of all fields (for iteration)");
            classText.AppendLine($"\t\tval allFields = listOf({Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), Library.ColumnName, includePrimaryKey: false, appendCommas: true, singleLine: true)})");
            classText.AppendLine("\t}");       
            classText.AppendLine("}");
        }

        private string ValidationFunction(SQLTableColumn column)
        {
            return $"object {column.Name}: {column.TableName}FormField<String>( validator = {{text -> if (text.isBlank()) R.string.invalid_{column.TableName.Decapitalise()}_{column.Name.Decapitalise()} else null}})";
        }
    }
}
