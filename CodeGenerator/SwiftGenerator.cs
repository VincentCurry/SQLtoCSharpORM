using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    /// <summary>
    /// Base class for the SwiftUI generators. It sets the file suffix and gives the generators a
    /// small helper for writing indented lines, so the generator code reads like the Swift it produces.
    /// </summary>
    public abstract class SwiftGenerator : Generator
    {
        protected SwiftGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileSuffix = "swift";
            SwiftNaming.EnsureUsableTypeNames(tables);
        }

        protected SwiftGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "swift";
            SwiftNaming.EnsureUsableTypeNames(tables);
        }

        /// <summary>Appends a line of Swift, indented by <paramref name="indent"/> levels of four spaces.</summary>
        protected void Line(int indent, string text)
        {
            if (text.Length > 0)
                classText.Append(new string(' ', indent * 4));

            classText.AppendLine(text);
        }

        /// <summary>Appends a blank line.</summary>
        protected void Line()
        {
            classText.AppendLine();
        }

        /// <summary>
        /// Writes the views in <paramref name="items"/> one after another. SwiftUI's ViewBuilder has traditionally accepted
        /// at most ten views in one block, so a longer list is split into Group { } chunks. A Group counts as a single view,
        /// and List and Form treat its contents as ordinary rows, so the screen looks the same.
        /// </summary>
        protected void WriteViewItems(int indent, IList<Action<int>> items)
        {
            const int maximumViewsInBlock = 10;
            const int chunkSize = 8;

            if (items.Count <= maximumViewsInBlock)
            {
                foreach (Action<int> item in items)
                    item(indent);

                return;
            }

            for (int start = 0; start < items.Count; start += chunkSize)
            {
                Line(indent, "Group {");

                for (int i = start; i < Math.Min(start + chunkSize, items.Count); i++)
                    items[i](indent + 1);

                Line(indent, "}");
            }
        }

        /// <summary>
        /// For generators that write one file covering every table (rather than one file per table).
        /// </summary>
        protected void WriteSingleFile(string fileNameWithoutExtension)
        {
            string filePath = FileName(fileNameWithoutExtension);
            EnsureDirectoryExists(filePath);
            File.WriteAllText(filePath, classText.ToString());
        }
    }

    internal enum SwiftFieldKind
    {
        Text,
        Integer,
        Real,
        Bool,
        Date
    }

    /// <summary>
    /// Naming and type helpers shared by the Swift generators.
    /// </summary>
    internal static class SwiftNaming
    {
        private static readonly HashSet<string> reservedWords = new HashSet<string>
        {
            "associatedtype", "class", "deinit", "enum", "extension", "fileprivate", "func", "import", "init",
            "inout", "internal", "let", "open", "operator", "private", "precedencegroup", "protocol", "public",
            "rethrows", "static", "struct", "subscript", "typealias", "var", "break", "case", "catch", "continue",
            "default", "defer", "do", "else", "fallthrough", "for", "guard", "if", "in", "repeat", "return",
            "throw", "switch", "where", "while", "as", "false", "is", "nil", "self", "super", "throws", "true", "try"
        };

        // Type names that would shadow a Swift or SwiftUI type used by the generated code.
        private static readonly HashSet<string> clashingTypeNames = new HashSet<string>
        {
            "Task", "Group", "Section", "Form", "List", "ForEach", "Label", "Button", "Toggle", "Text", "DatePicker",
            "LabeledContent", "ProgressView", "NavigationStack", "NavigationLink", "Binding", "State", "Environment",
            "View", "Data", "Date", "URL", "UUID", "Error", "Set", "Array", "Dictionary", "String", "Bool", "Int",
            "Double", "Float", "Locale", "Decoder", "Encoder"
        };

        /// <summary>
        /// A table called "Task" or "Group" would become a Swift type that hides Swift's own Task or SwiftUI's Group,
        /// and the generated code wouldn't compile. Fail up front with a clear message instead.
        /// </summary>
        internal static void EnsureUsableTypeNames(IEnumerable<SQLTable> tables)
        {
            if (tables == null)
                return;

            foreach (SQLTable table in tables)
            {
                if (clashingTypeNames.Contains(table.Name))
                    throw new SwiftTypeNameClashException(table.Name);
            }
        }

        /// <summary>Columns the user can edit: everything except the primary key.</summary>
        internal static IEnumerable<SQLTableColumn> EditableColumns(SQLTable table)
        {
            return table.Columns.Where(co => !co.PrimaryKey);
        }

        /// <summary>The Swift property name for a column (backtick-escaped if it is a reserved word).</summary>
        internal static string Property(SQLTableColumn column)
        {
            string name = column.Name.DecapitaliseAndUpdateSwiftKeywords();

            return reservedWords.Contains(name) ? "`" + name + "`" : name;
        }

        /// <summary>The name of the "is this optional date switched on" property used by nullable date columns.</summary>
        internal static string HasProperty(SQLTableColumn column)
        {
            return "has" + Capitalise(column.Name);
        }

        /// <summary>
        /// The JSON property name the ASP.NET Core API uses for a column. This is the same rule as
        /// System.Text.Json's camelCase naming policy, so "URLPath" becomes "urlPath" and "StudentId" becomes "studentId".
        /// </summary>
        internal static string JsonKey(SQLTableColumn column)
        {
            string name = column.Name;

            if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
                return name;

            char[] chars = name.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                    break;

                bool hasNext = i + 1 < chars.Length;

                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    if (chars[i + 1] == ' ')
                        chars[i] = char.ToLowerInvariant(chars[i]);

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }

        internal static string Capitalise(string value)
        {
            return string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        /// <summary>Turns "ReceiptDate" into "Receipt Date" for use in labels and titles.</summary>
        internal static string Label(string pascalCaseName)
        {
            StringBuilder label = new StringBuilder();

            for (int i = 0; i < pascalCaseName.Length; i++)
            {
                char current = pascalCaseName[i];

                if (current == '_')
                {
                    label.Append(' ');
                    continue;
                }

                if (i > 0 && char.IsUpper(current))
                {
                    char previous = pascalCaseName[i - 1];
                    bool nextIsLower = i + 1 < pascalCaseName.Length && char.IsLower(pascalCaseName[i + 1]);

                    if (char.IsLower(previous) || char.IsDigit(previous) || (char.IsUpper(previous) && nextIsLower))
                        label.Append(' ');
                }

                label.Append(i == 0 ? char.ToUpperInvariant(current) : current);
            }

            return label.ToString();
        }

        internal static string Label(SQLTableColumn column)
        {
            return Label(column.Name);
        }

        internal static string PluralLabel(SQLTable table)
        {
            string label = Label(table.Name);
            string lower = label.ToLowerInvariant();

            if (lower.EndsWith("y") && label.Length > 1 && "aeiou".IndexOf(lower[lower.Length - 2]) < 0)
                return label.Substring(0, label.Length - 1) + "ies";

            if (lower.EndsWith("s") || lower.EndsWith("x") || lower.EndsWith("z") || lower.EndsWith("ch") || lower.EndsWith("sh"))
                return label + "es";

            return label + "s";
        }

        /// <summary>Makes a string usable as the start of a Swift type name, e.g. "my app" becomes "MyApp".</summary>
        internal static string Identifier(string value)
        {
            StringBuilder identifier = new StringBuilder();
            bool capitaliseNext = true;

            foreach (char c in value ?? "")
            {
                if (char.IsLetterOrDigit(c))
                {
                    identifier.Append(capitaliseNext ? char.ToUpperInvariant(c) : c);
                    capitaliseNext = false;
                }
                else
                {
                    capitaliseNext = true;
                }
            }

            if (identifier.Length == 0)
                return "App";

            return char.IsDigit(identifier[0]) ? "App" + identifier : identifier.ToString();
        }

        /// <summary>The Swift type of a column, including the optional marker.</summary>
        internal static string SwiftType(SQLTableColumn column)
        {
            return column.iosDataType + (column.Nullable ? "?" : "");
        }

        internal static SwiftFieldKind Kind(SQLTableColumn column)
        {
            switch (column.iosDataType)
            {
                case iosDataTypes.strings:
                    return SwiftFieldKind.Text;
                case iosDataTypes.integer:
                    return SwiftFieldKind.Integer;
                case iosDataTypes.floatNum:
                case iosDataTypes.doubleNum:
                    return SwiftFieldKind.Real;
                case iosDataTypes.boolean:
                    return SwiftFieldKind.Bool;
                case iosDataTypes.date:
                    return SwiftFieldKind.Date;
                default:
                    throw new SQLDBTypeNotSupported(column.DataType);
            }
        }

        /// <summary>Text, integer and decimal columns are typed into text fields, so they are the ones that can be invalid.</summary>
        internal static bool IsValidated(SQLTableColumn column)
        {
            SwiftFieldKind kind = Kind(column);

            return kind == SwiftFieldKind.Text || kind == SwiftFieldKind.Integer || kind == SwiftFieldKind.Real;
        }

        /// <summary>Name of the FormParsing function that reads this column's text into its Swift type.</summary>
        internal static string ParseFunction(SQLTableColumn column)
        {
            if (Kind(column) == SwiftFieldKind.Integer)
                return "int32";

            return column.iosDataType == iosDataTypes.floatNum ? "float" : "double";
        }

        /// <summary>The maximum length to validate for a text column, or null when there is no limit to enforce.</summary>
        internal static int? MaximumTextLength(SQLTableColumn column)
        {
            bool isSizedTextType = column.DataType == SQLDataTypes.varChar
                || column.DataType == SQLDataTypes.ncharData
                || column.DataType == SQLDataTypes.charType;

            if (isSizedTextType && column.MaximumLength > 0)
                return column.MaximumLength;

            return null;
        }

        internal static bool IsLongText(SQLTableColumn column)
        {
            bool isSizedTextType = column.DataType == SQLDataTypes.varChar
                || column.DataType == SQLDataTypes.ncharData
                || column.DataType == SQLDataTypes.charType;

            return Kind(column) == SwiftFieldKind.Text && isSizedTextType && (column.MaximumLength == -1 || column.MaximumLength > 255);
        }

        internal static bool IncludesTime(SQLTableColumn column)
        {
            return column.DataType != SQLDataTypes.date;
        }

        /// <summary>Swift expression that turns the value at <paramref name="expression"/> into display text.</summary>
        internal static string DisplayText(SQLTableColumn column, string expression)
        {
            if (Kind(column) == SwiftFieldKind.Date)
                return $"DisplayFormat.dateText({expression}, includeTime: {(IncludesTime(column) ? "true" : "false")})";

            return $"DisplayFormat.text({expression})";
        }

        internal static string BoolLiteral(bool value)
        {
            return value ? "true" : "false";
        }
    }

    /// <summary>
    /// Thrown when a table's name is also the name of a Swift or SwiftUI type used by the generated code.
    /// </summary>
    public class SwiftTypeNameClashException : Exception
    {
        private readonly string tableName;

        public SwiftTypeNameClashException(string tableName)
        {
            this.tableName = tableName;
        }

        public override string Message
        {
            get
            {
                return $"The table name '{tableName}' is also the name of a Swift or SwiftUI type used by the generated code, so the Swift types generated for it would hide the real ones. Rename the table (or exclude it) before generating Swift.";
            }
        }
    }
}
