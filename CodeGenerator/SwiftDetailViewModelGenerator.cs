using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates the view model behind each table's detail screen: refreshing and deleting the one row.
    /// </summary>
    public class SwiftDetailViewModelGenerator : SwiftGenerator
    {
        public SwiftDetailViewModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "DetailViewModel";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string name = table.Name;

            Line(0, "import Foundation");
            Line();

            Line(0, "@MainActor");
            Line(0, $"final class {name}DetailViewModel: ObservableObject {{");
            Line(1, $"@Published private(set) var item: {name}");
            Line(1, "@Published var errorMessage: String?");
            Line();
            Line(1, $"private let repository: {name}Repository");
            Line();
            Line(1, $"init(item: {name}, repository: {name}Repository) {{");
            Line(2, "self.item = item");
            Line(2, "self.repository = repository");
            Line(1, "}");
            Line();
            Line(1, "/// Re-reads the row from the server, e.g. after it has been edited.");
            Line(1, "func reload() async {");
            Line(2, "do {");
            Line(3, "item = try await repository.fetch(id: item.id)");
            Line(2, "} catch {");
            Line(3, "errorMessage = error.localizedDescription");
            Line(2, "}");
            Line(1, "}");
            Line();
            Line(1, "/// Deletes the row. Returns true if it was deleted.");
            Line(1, "func delete() async -> Bool {");
            Line(2, "do {");
            Line(3, "try await repository.delete(id: item.id)");
            Line(3, "return true");
            Line(2, "} catch {");
            Line(3, "errorMessage = error.localizedDescription");
            Line(3, "return false");
            Line(2, "}");
            Line(1, "}");
            Line(0, "}");
        }
    }
}
