using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    /// <summary>
    /// Generates the per-table enum of validated form fields and the validation rule for each
    /// (the equivalent of Android's FormField sealed class).
    /// </summary>
    public class SwiftFormFieldGenerator : SwiftGenerator
    {
        public SwiftFormFieldGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormField";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            List<SQLTableColumn> validatedColumns = SwiftNaming.EditableColumns(table).Where(SwiftNaming.IsValidated).ToList();

            Line(0, "import Foundation");
            Line();

            Line(0, $"/// The {table.Name} form fields that can be invalid: the ones typed into text fields.");
            Line(0, $"enum {table.Name}FormField: CaseIterable, Hashable {{");

            foreach (SQLTableColumn column in validatedColumns)
            {
                Line(1, $"case {SwiftNaming.Property(column)}");
            }

            Line();
            Line(1, "/// Returns why the field's current value is invalid, or nil when it is valid.");
            Line(1, $"func validationMessage(in state: {table.Name}FormState) -> String? {{");
            Line(2, "switch self {");

            foreach (SQLTableColumn column in validatedColumns)
            {
                Line(2, $"case .{SwiftNaming.Property(column)}:");
                Line(3, $"return {ValidationCall(column)}");
            }

            Line(2, "}");
            Line(1, "}");
            Line(0, "}");
        }

        private string ValidationCall(SQLTableColumn column)
        {
            string value = $"state.{SwiftNaming.Property(column)}";
            string label = $"label: \"{SwiftNaming.Label(column)}\"";
            string required = $"required: {SwiftNaming.BoolLiteral(!column.Nullable)}";

            switch (SwiftNaming.Kind(column))
            {
                case SwiftFieldKind.Integer:
                    return $"FormValidation.integer({value}, {label}, {required})";
                case SwiftFieldKind.Real:
                    return $"FormValidation.decimal({value}, {label}, {required})";
                default:
                    if (column.DataType == SQLDataTypes.uniqueIdentifier)
                        return $"FormValidation.guid({value}, {label}, {required})";

                    int? maximumLength = SwiftNaming.MaximumTextLength(column);

                    return maximumLength.HasValue
                        ? $"FormValidation.text({value}, {label}, {required}, maxLength: {maximumLength.Value})"
                        : $"FormValidation.text({value}, {label}, {required})";
            }
        }
    }
}
