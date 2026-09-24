using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    /// <summary>
    /// Generates the editable state behind each table's form (the equivalent of Android's FormState).
    /// Text and numeric columns are held as strings so half-typed input can be validated, and
    /// nullable date columns get a "has" switch, like the Kotlin form's date switch.
    /// </summary>
    public class SwiftFormStateGenerator : SwiftGenerator
    {
        public SwiftFormStateGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormState";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            List<SQLTableColumn> editableColumns = SwiftNaming.EditableColumns(table).ToList();

            Line(0, "import Foundation");
            Line();

            Line(0, $"/// The values being edited on the {table.Name} form, plus their validation state.");
            Line(0, "///");
            Line(0, "/// Text and numeric columns are held as strings so that half-typed input can be validated.");
            Line(0, "/// A nullable date column has a matching \"has\" switch: when it is off, nil is sent.");
            Line(0, $"struct {table.Name}FormState: Equatable {{");

            foreach (SQLTableColumn column in editableColumns)
            {
                string property = SwiftNaming.Property(column);

                switch (SwiftNaming.Kind(column))
                {
                    case SwiftFieldKind.Text:
                    case SwiftFieldKind.Integer:
                    case SwiftFieldKind.Real:
                        Line(1, $"var {property}: String = \"\"");
                        break;
                    case SwiftFieldKind.Bool:
                        Line(1, $"var {property}: Bool = false");
                        break;
                    case SwiftFieldKind.Date:
                        Line(1, $"var {property}: Date = Date()");
                        if (column.Nullable)
                            Line(1, $"var {SwiftNaming.HasProperty(column)}: Bool = false");
                        break;
                }
            }

            Line();
            Line(1, "/// The current validation message for each invalid field.");
            Line(1, $"var validationErrors: [{table.Name}FormField: String] = [:]");
            Line(1, "/// Fields the user has edited, or that a save attempt has checked. Errors are only shown for these.");
            Line(1, $"var touchedFields: Set<{table.Name}FormField> = []");
            Line();

            Line(1, "/// An empty form, for creating a new row.");
            Line(1, "init() {}");
            Line();

            Line(1, "/// A form filled in from an existing row, for editing it.");
            Line(1, $"init(item: {table.Name}) {{");
            foreach (SQLTableColumn column in editableColumns)
            {
                WriteInitialisation(column);
            }
            Line(1, "}");
            Line();

            Line(1, "/// Converts the form into the request body. Returns nil if a numeric field can't be parsed,");
            Line(1, "/// which validation normally prevents.");
            Line(1, $"func makeInput() -> {table.Name}Input? {{");
            foreach (SQLTableColumn column in editableColumns.Where(co => !co.Nullable && (SwiftNaming.Kind(co) == SwiftFieldKind.Integer || SwiftNaming.Kind(co) == SwiftFieldKind.Real)))
            {
                string property = SwiftNaming.Property(column);
                Line(2, $"guard let {property} = FormParsing.{SwiftNaming.ParseFunction(column)}(self.{property}) else {{ return nil }}");
            }
            Line(2, $"return {table.Name}Input(");
            for (int i = 0; i < editableColumns.Count; i++)
            {
                Line(3, InputArgument(editableColumns[i]) + (i < editableColumns.Count - 1 ? "," : ""));
            }
            Line(2, ")");
            Line(1, "}");

            Line(0, "}");
        }

        private void WriteInitialisation(SQLTableColumn column)
        {
            string property = SwiftNaming.Property(column);

            switch (SwiftNaming.Kind(column))
            {
                case SwiftFieldKind.Text:
                    Line(2, column.Nullable ? $"self.{property} = item.{property} ?? \"\"" : $"self.{property} = item.{property}");
                    break;
                case SwiftFieldKind.Integer:
                case SwiftFieldKind.Real:
                    Line(2, column.Nullable
                        ? $"self.{property} = item.{property}.map {{ FormParsing.string(from: $0) }} ?? \"\""
                        : $"self.{property} = FormParsing.string(from: item.{property})");
                    break;
                case SwiftFieldKind.Bool:
                    Line(2, column.Nullable ? $"self.{property} = item.{property} ?? false" : $"self.{property} = item.{property}");
                    break;
                case SwiftFieldKind.Date:
                    if (column.Nullable)
                    {
                        Line(2, $"if let value = item.{property} {{");
                        Line(3, $"self.{property} = value");
                        Line(3, $"self.{SwiftNaming.HasProperty(column)} = true");
                        Line(2, "}");
                    }
                    else
                    {
                        Line(2, $"self.{property} = item.{property}");
                    }
                    break;
            }
        }

        private string InputArgument(SQLTableColumn column)
        {
            string property = SwiftNaming.Property(column);

            switch (SwiftNaming.Kind(column))
            {
                case SwiftFieldKind.Text:
                    return column.Nullable ? $"{property}: FormParsing.nilIfBlank(self.{property})" : $"{property}: self.{property}";
                case SwiftFieldKind.Integer:
                case SwiftFieldKind.Real:
                    return column.Nullable ? $"{property}: FormParsing.{SwiftNaming.ParseFunction(column)}(self.{property})" : $"{property}: {property}";
                case SwiftFieldKind.Date:
                    return column.Nullable ? $"{property}: self.{SwiftNaming.HasProperty(column)} ? self.{property} : nil" : $"{property}: self.{property}";
                default:
                    return $"{property}: self.{property}";
            }
        }
    }
}
