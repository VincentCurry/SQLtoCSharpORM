using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    /// <summary>
    /// Generates the Swift model for each table: a Codable struct that mirrors the row as the API returns it,
    /// and an Encodable "Input" struct holding the writable fields (everything except the primary key) that is sent
    /// when creating or updating a row.
    /// </summary>
    public class SwiftModelGenerator : SwiftGenerator
    {
        public SwiftModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            SQLTableColumn key = table.PrimaryKey;
            List<SQLTableColumn> editableColumns = SwiftNaming.EditableColumns(table).ToList();

            Line(0, "import Foundation");
            Line();

            Line(0, $"/// A row of the {table.Name} table, as returned by the API.");
            Line(0, $"struct {table.Name}: Identifiable, Codable, Hashable {{");

            foreach (SQLTableColumn column in table.Columns)
            {
                Line(1, $"{(column.PrimaryKey ? "let" : "var")} {SwiftNaming.Property(column)}: {SwiftNaming.SwiftType(column)}");
            }

            if (SwiftNaming.Property(key) != "id")
            {
                Line();
                Line(1, $"var id: {key.iosDataType} {{ {SwiftNaming.Property(key)} }}");
            }

            Line();
            WriteCodingKeys(1, table.Columns);
            Line(0, "}");
            Line();

            Line(0, $"/// The writable fields of {table.Name}: every column except the primary key.");
            Line(0, "/// This is the body sent when creating or updating a row.");
            Line(0, $"struct {table.Name}Input: Encodable {{");

            foreach (SQLTableColumn column in editableColumns)
            {
                Line(1, $"var {SwiftNaming.Property(column)}: {SwiftNaming.SwiftType(column)}");
            }

            if (editableColumns.Count > 0)
            {
                Line();
                WriteCodingKeys(1, editableColumns);
            }

            Line(0, "}");
        }

        private void WriteCodingKeys(int indent, IEnumerable<SQLTableColumn> columns)
        {
            Line(indent, "enum CodingKeys: String, CodingKey {");

            foreach (SQLTableColumn column in columns)
            {
                string property = SwiftNaming.Property(column);
                string jsonKey = SwiftNaming.JsonKey(column);

                Line(indent + 1, property == jsonKey ? $"case {property}" : $"case {property} = \"{jsonKey}\"");
            }

            Line(indent, "}");
        }
    }
}
