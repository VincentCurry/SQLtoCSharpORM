using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates the view model behind each table's create/edit form (the equivalent of Android's SaveViewModel):
    /// it owns the form state, validates it and saves through the repository.
    /// </summary>
    public class SwiftFormViewModelGenerator : SwiftGenerator
    {
        public SwiftFormViewModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormViewModel";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string name = table.Name;

            Line(0, "import SwiftUI");
            Line(0, "internal import Combine");
            Line();

            Line(0, "@MainActor");
            Line(0, $"final class {name}FormViewModel: ObservableObject {{");
            Line(1, $"@Published var form: {name}FormState");
            Line(1, "@Published private(set) var saveState: SaveState = .idle");
            Line();
            Line(1, $"private let repository: {name}Repository");
            Line(1, $"private let editingItem: {name}?");
            Line();
            Line(1, "/// Pass an item to edit it; leave it nil to create a new one.");
            Line(1, $"init(repository: {name}Repository, item: {name}? = nil) {{");
            Line(2, "self.repository = repository");
            Line(2, "self.editingItem = item");
            Line(2, "if let item = item {");
            Line(3, $"self.form = {name}FormState(item: item)");
            Line(2, "} else {");
            Line(3, $"self.form = {name}FormState()");
            Line(2, "}");
            Line(1, "}");
            Line();
            Line(1, "var isEditing: Bool { editingItem != nil }");
            Line();
            Line(1, "var isSaving: Bool {");
            Line(2, "if case .saving = saveState { return true }");
            Line(2, "return false");
            Line(1, "}");
            Line();
            Line(1, "var failureMessage: String? {");
            Line(2, "if case .failed(let message) = saveState { return message }");
            Line(2, "return nil");
            Line(1, "}");
            Line();
            Line(1, "func clearFailure() {");
            Line(2, "saveState = .idle");
            Line(1, "}");
            Line();
            Line(1, "/// A two-way binding for a form value. When a field is given, editing the value also validates it.");
            Line(1, $"func binding<Value>(_ keyPath: WritableKeyPath<{name}FormState, Value>, validating field: {name}FormField? = nil) -> Binding<Value> {{");
            Line(2, "Binding(");
            Line(3, "get: { self.form[keyPath: keyPath] },");
            Line(3, "set: { newValue in");
            Line(4, "self.form[keyPath: keyPath] = newValue");
            Line(4, "if let field = field {");
            Line(5, "self.fieldEdited(field)");
            Line(4, "}");
            Line(3, "}");
            Line(2, ")");
            Line(1, "}");
            Line();
            Line(1, "/// The message to show under a field, once the user has edited it or tried to save.");
            Line(1, $"func errorMessage(for field: {name}FormField) -> String? {{");
            Line(2, "form.touchedFields.contains(field) ? form.validationErrors[field] : nil");
            Line(1, "}");
            Line();
            Line(1, $"private func fieldEdited(_ field: {name}FormField) {{");
            Line(2, "form.touchedFields.insert(field)");
            Line(2, "form.validationErrors[field] = field.validationMessage(in: form)");
            Line(1, "}");
            Line();
            Line(1, "/// Validates every field, showing all errors. Returns true when the form is valid.");
            Line(1, "@discardableResult");
            Line(1, "func validateAll() -> Bool {");
            Line(2, $"var errors: [{name}FormField: String] = [:]");
            Line(2, $"for field in {name}FormField.allCases {{");
            Line(3, "if let message = field.validationMessage(in: form) {");
            Line(4, "errors[field] = message");
            Line(3, "}");
            Line(2, "}");
            Line(2, "form.validationErrors = errors");
            Line(2, $"form.touchedFields = Set({name}FormField.allCases)");
            Line(2, "return errors.isEmpty");
            Line(1, "}");
            Line();
            Line(1, "/// Validates and saves the form. Returns true if it was saved.");
            Line(1, "func save() async -> Bool {");
            Line(2, "guard !isSaving else { return false }");
            Line(2, "guard validateAll(), let input = form.makeInput() else { return false }");
            Line();
            Line(2, "saveState = .saving");
            Line(2, "do {");
            Line(3, "if let item = editingItem {");
            Line(4, "try await repository.update(id: item.id, with: input)");
            Line(3, "} else {");
            Line(4, "_ = try await repository.create(input)");
            Line(3, "}");
            Line(3, "saveState = .saved");
            Line(3, "return true");
            Line(2, "} catch {");
            Line(3, "saveState = .failed(error.localizedDescription)");
            Line(3, "return false");
            Line(2, "}");
            Line(1, "}");
            Line(0, "}");
        }
    }
}
