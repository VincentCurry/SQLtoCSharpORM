using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates each table's detail screen: a read-only view of one row with Edit and Delete actions.
    /// </summary>
    public class SwiftDetailViewGenerator : SwiftGenerator
    {
        public SwiftDetailViewGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "DetailView";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string name = table.Name;
            string singular = SwiftNaming.Label(name);

            Line(0, "import SwiftUI");
            Line();

            Line(0, $"struct {name}DetailView: View {{");
            Line(1, $"@StateObject private var viewModel: {name}DetailViewModel");
            Line(1, "@Environment(\\.dismiss) private var dismiss");
            Line(1, "@State private var showingEditForm = false");
            Line(1, "@State private var confirmingDelete = false");
            Line(1, $"private let repository: {name}Repository");
            Line(1, "private let onChanged: () -> Void");
            Line();
            Line(1, "/// onChanged is called after the row is edited or deleted, so the list can refresh.");
            Line(1, $"init(item: {name}, repository: {name}Repository, onChanged: @escaping () -> Void = {{}}) {{");
            Line(2, "self.repository = repository");
            Line(2, "self.onChanged = onChanged");
            Line(2, $"_viewModel = StateObject(wrappedValue: {name}DetailViewModel(item: item, repository: repository))");
            Line(1, "}");
            Line();
            Line(1, "var body: some View {");
            Line(2, "Form {");
            Line(3, "Section {");

            List<Action<int>> rows = new List<Action<int>>();
            foreach (SQLTableColumn column in SwiftNaming.EditableColumns(table))
            {
                SQLTableColumn currentColumn = column;
                rows.Add(indent =>
                {
                    string value = SwiftNaming.DisplayText(currentColumn, "viewModel.item." + SwiftNaming.Property(currentColumn));
                    Line(indent, $"LabeledContent(\"{SwiftNaming.Label(currentColumn)}\", value: {value})");
                });
            }
            WriteViewItems(4, rows);

            Line(3, "}");
            Line(3, "Section {");
            Line(4, $"Button(\"Delete {singular}\", role: .destructive) {{");
            Line(5, "confirmingDelete = true");
            Line(4, "}");
            Line(3, "}");
            Line(2, "}");
            Line(2, $".navigationTitle(\"{singular}\")");
            Line(2, ".navigationBarTitleDisplayMode(.inline)");
            Line(2, ".toolbar {");
            Line(3, "ToolbarItem(placement: .primaryAction) {");
            Line(4, "Button(\"Edit\") { showingEditForm = true }");
            Line(3, "}");
            Line(2, "}");
            Line(2, ".sheet(isPresented: $showingEditForm) {");
            Line(3, $"{name}FormView(repository: repository, item: viewModel.item) {{");
            Line(4, "Task {");
            Line(5, "await viewModel.reload()");
            Line(5, "onChanged()");
            Line(4, "}");
            Line(3, "}");
            Line(2, "}");
            Line(2, $".confirmationDialog(\"Delete this {singular}?\", isPresented: $confirmingDelete, titleVisibility: .visible) {{");
            Line(3, "Button(\"Delete\", role: .destructive) {");
            Line(4, "Task {");
            Line(5, "if await viewModel.delete() {");
            Line(6, "onChanged()");
            Line(6, "dismiss()");
            Line(5, "}");
            Line(4, "}");
            Line(3, "}");
            Line(2, "}");
            Line(2, ".alert(\"Something went wrong\", isPresented: showingError) {");
            Line(3, "Button(\"OK\", role: .cancel) { viewModel.errorMessage = nil }");
            Line(2, "} message: {");
            Line(3, "Text(viewModel.errorMessage ?? \"\")");
            Line(2, "}");
            Line(1, "}");
            Line();
            Line(1, "private var showingError: Binding<Bool> {");
            Line(2, "Binding(");
            Line(3, "get: { viewModel.errorMessage != nil },");
            Line(3, "set: { if !$0 { viewModel.errorMessage = nil } }");
            Line(2, ")");
            Line(1, "}");
            Line(0, "}");
        }
    }
}
