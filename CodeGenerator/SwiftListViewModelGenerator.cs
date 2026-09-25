using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates the view model behind each table's list screen: loading, refreshing and deleting rows.
    /// It relies on the ResultState enum written by SwiftSupportFilesGenerator.
    /// </summary>
    public class SwiftListViewModelGenerator : SwiftGenerator
    {
        public SwiftListViewModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "ListViewModel";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string name = table.Name;

            Line(0, "import Foundation");
            Line(0, "internal import Combine");
            Line();

            Line(0, "@MainActor");
            Line(0, $"final class {name}ListViewModel: ObservableObject {{");
            Line(1, $"@Published private(set) var items: [{name}] = []");
            Line(1, $"@Published private(set) var state: ResultState<[{name}]> = .idle");
            Line(1, "/// Set when a refresh or delete fails while there is still data on screen to keep showing.");
            Line(1, "@Published var errorMessage: String?");
            Line();
            Line(1, $"private let repository: {name}Repository");
            Line();
            Line(1, $"init(repository: {name}Repository) {{");
            Line(2, "self.repository = repository");
            Line(1, "}");
            Line();
            Line(1, "func load() async {");
            Line(2, "if items.isEmpty {");
            Line(3, "state = .loading");
            Line(2, "}");
            Line(2, "do {");
            Line(3, "let result = try await repository.fetchAll()");
            Line(3, "items = result");
            Line(3, "state = .success(result)");
            Line(2, "} catch {");
            Line(3, "if error is CancellationError || (error as? URLError)?.code == .cancelled {");
            Line(4, "return");
            Line(3, "}");
            Line(3, "if items.isEmpty {");
            Line(4, "state = .failure(error)");
            Line(3, "} else {");
            Line(4, "errorMessage = error.localizedDescription");
            Line(3, "}");
            Line(2, "}");
            Line(1, "}");
            Line();
            Line(1, "func delete(at offsets: IndexSet) async {");
            Line(2, "let doomed = offsets.map { items[$0] }");
            Line(2, "for item in doomed {");
            Line(3, "do {");
            Line(4, "try await repository.delete(id: item.id)");
            Line(4, "items.removeAll { $0.id == item.id }");
            Line(3, "} catch {");
            Line(4, "errorMessage = error.localizedDescription");
            Line(3, "}");
            Line(2, "}");
            Line(1, "}");
            Line(0, "}");
        }
    }
}
