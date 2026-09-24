using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    /// <summary>
    /// Generates each table's list screen: a list of rows that pushes the detail screen,
    /// with pull to refresh, swipe to delete and a button that presents the "new" form.
    /// The screen expects to be shown inside a NavigationStack (see SwiftRootViewGenerator).
    /// </summary>
    public class SwiftListViewGenerator : SwiftGenerator
    {
        public SwiftListViewGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "ListView";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string name = table.Name;
            string plural = SwiftNaming.PluralLabel(table);
            string singular = SwiftNaming.Label(name);

            Line(0, "import SwiftUI");
            Line();

            Line(0, $"struct {name}ListView: View {{");
            Line(1, $"@StateObject private var viewModel: {name}ListViewModel");
            Line(1, "@State private var showingNewForm = false");
            Line(1, $"private let repository: {name}Repository");
            Line();
            Line(1, $"init(repository: {name}Repository) {{");
            Line(2, "self.repository = repository");
            Line(2, $"_viewModel = StateObject(wrappedValue: {name}ListViewModel(repository: repository))");
            Line(1, "}");
            Line();
            Line(1, "var body: some View {");
            Line(2, "content");
            Line(3, $".navigationTitle(\"{plural}\")");
            Line(3, ".toolbar {");
            Line(4, "ToolbarItem(placement: .primaryAction) {");
            Line(5, "Button {");
            Line(6, "showingNewForm = true");
            Line(5, "} label: {");
            Line(6, $"Label(\"Add {singular}\", systemImage: \"plus\")");
            Line(5, "}");
            Line(4, "}");
            Line(3, "}");
            Line(3, $".navigationDestination(for: {name}.self) {{ item in");
            Line(4, $"{name}DetailView(item: item, repository: repository) {{ reload() }}");
            Line(3, "}");
            Line(3, ".sheet(isPresented: $showingNewForm) {");
            Line(4, $"{name}FormView(repository: repository) {{ reload() }}");
            Line(3, "}");
            Line(3, ".alert(\"Something went wrong\", isPresented: showingError) {");
            Line(4, "Button(\"OK\", role: .cancel) { viewModel.errorMessage = nil }");
            Line(3, "} message: {");
            Line(4, "Text(viewModel.errorMessage ?? \"\")");
            Line(3, "}");
            Line(3, ".task { await viewModel.load() }");
            Line(1, "}");
            Line();
            Line(1, "@ViewBuilder");
            Line(1, "private var content: some View {");
            Line(2, "switch viewModel.state {");
            Line(2, "case .idle, .loading:");
            Line(3, "ProgressView(\"Loading...\")");
            Line(2, "case .failure(let error):");
            Line(3, "VStack(spacing: 12) {");
            Line(4, $"Text(\"Couldn't load {plural}\")");
            Line(5, ".font(.headline)");
            Line(4, "Text(error.localizedDescription)");
            Line(5, ".font(.footnote)");
            Line(5, ".foregroundStyle(.secondary)");
            Line(5, ".multilineTextAlignment(.center)");
            Line(4, "Button(\"Try Again\") { reload() }");
            Line(3, "}");
            Line(3, ".padding()");
            Line(2, "case .success:");
            Line(3, "if viewModel.items.isEmpty {");
            Line(4, $"Text(\"No {plural} yet. Tap + to add one.\")");
            Line(5, ".foregroundStyle(.secondary)");
            Line(3, "} else {");
            Line(4, "List {");
            Line(5, "ForEach(viewModel.items) { item in");
            Line(6, $"NavigationLink(value: item) {{ {name}Row(item: item) }}");
            Line(5, "}");
            Line(5, ".onDelete { offsets in");
            Line(6, "Task { await viewModel.delete(at: offsets) }");
            Line(5, "}");
            Line(4, "}");
            Line(4, ".refreshable { await viewModel.load() }");
            Line(3, "}");
            Line(2, "}");
            Line(1, "}");
            Line();
            Line(1, "private var showingError: Binding<Bool> {");
            Line(2, "Binding(");
            Line(3, "get: { viewModel.errorMessage != nil },");
            Line(3, "set: { if !$0 { viewModel.errorMessage = nil } }");
            Line(2, ")");
            Line(1, "}");
            Line();
            Line(1, "private func reload() {");
            Line(2, "Task { await viewModel.load() }");
            Line(1, "}");
            Line(0, "}");
            Line();

            WriteRow(table);
        }

        private void WriteRow(SQLTable table)
        {
            // Show up to two columns in each row, preferring text columns (e.g. a name or description) first.
            List<SQLTableColumn> shown = SwiftNaming.EditableColumns(table)
                .OrderBy(co => SwiftNaming.Kind(co) == SwiftFieldKind.Text ? 0 : 1)
                .Take(2)
                .ToList();

            Line(0, $"private struct {table.Name}Row: View {{");
            Line(1, $"let item: {table.Name}");
            Line();
            Line(1, "var body: some View {");

            if (shown.Count == 0)
            {
                Line(2, "Text(\"\\(item.id)\")");
            }
            else
            {
                Line(2, "VStack(alignment: .leading, spacing: 2) {");
                Line(3, $"Text({SwiftNaming.DisplayText(shown[0], "item." + SwiftNaming.Property(shown[0]))})");
                Line(4, ".font(.headline)");

                if (shown.Count > 1)
                {
                    Line(3, $"Text({SwiftNaming.DisplayText(shown[1], "item." + SwiftNaming.Property(shown[1]))})");
                    Line(4, ".font(.subheadline)");
                    Line(4, ".foregroundStyle(.secondary)");
                }

                Line(2, "}");
            }

            Line(1, "}");
            Line(0, "}");
        }
    }
}
