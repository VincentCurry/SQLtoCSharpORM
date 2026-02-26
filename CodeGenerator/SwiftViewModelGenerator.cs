using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftViewModelGenerator : Generator
    {
        public SwiftViewModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "ViewModel";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine("@MainActor");
            classText.AppendLine($"class {table.Name}ViewModel: ObservableObject {{");
            classText.AppendLine($"    @Published var items: [{table.Name}] = []");
            classText.AppendLine("    @Published var state: ResultState<["
                          + table.Name + "]> = .idle");
            classText.AppendLine();
            classText.AppendLine($"    private let repository: {table.Name}Repository");
            classText.AppendLine();
            classText.AppendLine($"    init(repository: {table.Name}Repository) {{");
            classText.AppendLine("        self.repository = repository;");
            classText.AppendLine("    }");
            classText.AppendLine();
            classText.AppendLine("    func load() async {");
            classText.AppendLine("        state = .loading");
            classText.AppendLine("        do {");
            classText.AppendLine("            let result = try await repository.fetchAll()");
            classText.AppendLine("            items = result");
            classText.AppendLine("            state = .success(result)");
            classText.AppendLine("        } catch {");
            classText.AppendLine("            state = .failure(error)");
            classText.AppendLine("        }");
            classText.AppendLine("    }");
            classText.AppendLine("}");
        }
    }
}
