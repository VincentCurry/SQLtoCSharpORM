using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates RootView.swift: the NavigationStack that lists every table and pushes its list screen.
    /// </summary>
    public class SwiftRootViewGenerator : SwiftGenerator
    {
        public SwiftRootViewGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }

        public new void GenerateClasses()
        {
            string appName = SwiftNaming.Identifier(_nameSpace);
            string title = _nameSpace == "NOTUSED" ? "Home" : SwiftNaming.Label(appName);

            classText = new System.Text.StringBuilder();

            Line(0, "import SwiftUI");
            Line();
            Line(0, "// To use it, show RootView from your @main App:");
            Line(0, "//");
            Line(0, "//     @main");
            Line(0, $"//     struct {appName}App: App {{");
            Line(0, "//         private let container = AppContainer(");
            Line(0, "//             configuration: APIConfiguration(baseURL: URL(string: \"https://your-server.example\")!)");
            Line(0, "//         )");
            Line(0, "//");
            Line(0, "//         var body: some Scene {");
            Line(0, "//             WindowGroup { RootView(container: container) }");
            Line(0, "//         }");
            Line(0, "//     }");
            Line();
            Line(0, "struct RootView: View {");
            Line(1, "let container: AppContainer");
            Line();
            Line(1, "var body: some View {");
            Line(2, "NavigationStack {");
            Line(3, "List {");

            List<Action<int>> links = new List<Action<int>>();
            foreach (SQLTable table in _sQLTables)
            {
                SQLTable currentTable = table;
                links.Add(indent =>
                {
                    Line(indent, $"NavigationLink(\"{SwiftNaming.PluralLabel(currentTable)}\") {{");
                    Line(indent + 1, $"{currentTable.Name}ListView(repository: container.{SwiftAppContainerGenerator.RepositoryProperty(currentTable)})");
                    Line(indent, "}");
                });
            }
            WriteViewItems(4, links);

            Line(3, "}");
            Line(3, $".navigationTitle(\"{title}\")");
            Line(2, "}");
            Line(1, "}");
            Line(0, "}");

            WriteSingleFile("RootView");
        }
    }
}
