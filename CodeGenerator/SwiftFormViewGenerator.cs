using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates each table's create/edit form, presented as a sheet with its own NavigationStack.
    /// One control is generated per column: a text field, toggle or date picker depending on the column's type.
    /// </summary>
    public class SwiftFormViewGenerator : SwiftGenerator
    {
        public SwiftFormViewGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormView";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string name = table.Name;
            string singular = SwiftNaming.Label(name);

            Line(0, "import SwiftUI");
            Line();

            Line(0, $"struct {name}FormView: View {{");
            Line(1, $"@StateObject private var viewModel: {name}FormViewModel");
            Line(1, "@Environment(\\.dismiss) private var dismiss");
            Line(1, "private let onSaved: () -> Void");
            Line();
            Line(1, "/// Pass an item to edit it; leave it nil to create a new one. onSaved is called after a successful save.");
            Line(1, $"init(repository: {name}Repository, item: {name}? = nil, onSaved: @escaping () -> Void = {{}}) {{");
            Line(2, "self.onSaved = onSaved");
            Line(2, $"_viewModel = StateObject(wrappedValue: {name}FormViewModel(repository: repository, item: item))");
            Line(1, "}");
            Line();
            Line(1, "var body: some View {");
            Line(2, "NavigationStack {");
            Line(3, "Form {");
            Line(4, "Section {");

            List<Action<int>> fields = new List<Action<int>>();
            foreach (SQLTableColumn column in SwiftNaming.EditableColumns(table))
            {
                SQLTableColumn currentColumn = column;
                fields.Add(indent => WriteField(indent, currentColumn));
            }
            WriteViewItems(5, fields);

            Line(4, "}");
            Line(3, "}");
            Line(3, $".navigationTitle(viewModel.isEditing ? \"Edit {singular}\" : \"New {singular}\")");
            Line(3, ".navigationBarTitleDisplayMode(.inline)");
            Line(3, ".toolbar {");
            Line(4, "ToolbarItem(placement: .cancellationAction) {");
            Line(5, "Button(\"Cancel\") { dismiss() }");
            Line(4, "}");
            Line(4, "ToolbarItem(placement: .confirmationAction) {");
            Line(5, "if viewModel.isSaving {");
            Line(6, "ProgressView()");
            Line(5, "} else {");
            Line(6, "Button(\"Save\") { save() }");
            Line(5, "}");
            Line(4, "}");
            Line(3, "}");
            Line(3, ".alert(\"Couldn't save\", isPresented: showingError) {");
            Line(4, "Button(\"OK\", role: .cancel) { viewModel.clearFailure() }");
            Line(3, "} message: {");
            Line(4, "Text(viewModel.failureMessage ?? \"\")");
            Line(3, "}");
            Line(2, "}");
            Line(1, "}");
            Line();
            Line(1, "private var showingError: Binding<Bool> {");
            Line(2, "Binding(");
            Line(3, "get: { viewModel.failureMessage != nil },");
            Line(3, "set: { if !$0 { viewModel.clearFailure() } }");
            Line(2, ")");
            Line(1, "}");
            Line();
            Line(1, "private func save() {");
            Line(2, "Task {");
            Line(3, "if await viewModel.save() {");
            Line(4, "onSaved()");
            Line(4, "dismiss()");
            Line(3, "}");
            Line(2, "}");
            Line(1, "}");
            Line(0, "}");
        }

        private void WriteField(int indent, SQLTableColumn column)
        {
            string property = SwiftNaming.Property(column);
            string label = SwiftNaming.Label(column);

            switch (SwiftNaming.Kind(column))
            {
                case SwiftFieldKind.Text:
                    Line(indent, "FormTextField(");
                    Line(indent + 1, $"title: \"{label}\",");
                    Line(indent + 1, $"text: viewModel.binding(\\.{property}, validating: .{property}),");
                    Line(indent + 1, $"error: viewModel.errorMessage(for: .{property}){(SwiftNaming.IsLongText(column) ? "," : "")}");
                    if (SwiftNaming.IsLongText(column))
                        Line(indent + 1, "multiline: true");
                    Line(indent, ")");
                    break;

                case SwiftFieldKind.Integer:
                case SwiftFieldKind.Real:
                    Line(indent, "FormTextField(");
                    Line(indent + 1, $"title: \"{label}\",");
                    Line(indent + 1, $"text: viewModel.binding(\\.{property}, validating: .{property}),");
                    Line(indent + 1, $"error: viewModel.errorMessage(for: .{property}),");
                    Line(indent + 1, "keyboard: .numbersAndPunctuation");
                    Line(indent, ")");
                    break;

                case SwiftFieldKind.Bool:
                    Line(indent, $"Toggle(\"{label}\", isOn: viewModel.binding(\\.{property}))");
                    break;

                case SwiftFieldKind.Date:
                    string components = SwiftNaming.IncludesTime(column) ? "[.date, .hourAndMinute]" : ".date";

                    if (column.Nullable)
                    {
                        // Wrapped in a Group so the switch and the picker count as one view (see WriteViewItems).
                        string has = SwiftNaming.HasProperty(column);

                        Line(indent, "Group {");
                        Line(indent + 1, $"Toggle(\"{label}\", isOn: viewModel.binding(\\.{has}))");
                        Line(indent + 1, $"if viewModel.form.{has} {{");
                        Line(indent + 2, $"DatePicker(\"{label}\", selection: viewModel.binding(\\.{property}), displayedComponents: {components})");
                        Line(indent + 1, "}");
                        Line(indent, "}");
                    }
                    else
                    {
                        Line(indent, $"DatePicker(\"{label}\", selection: viewModel.binding(\\.{property}), displayedComponents: {components})");
                    }
                    break;
            }
        }
    }
}
