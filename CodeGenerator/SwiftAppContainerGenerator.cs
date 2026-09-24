using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates AppContainer.swift, the composition root that creates the shared APIClient and one repository per table
    /// (the role Android's ViewModelFactory classes play). Like SwiftDataBaseGenerator it writes a single file for all tables.
    /// </summary>
    public class SwiftAppContainerGenerator : SwiftGenerator
    {
        public SwiftAppContainerGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }

        public new void GenerateClasses()
        {
            classText = new System.Text.StringBuilder();

            Line(0, "import Foundation");
            Line();
            Line(0, "/// Composition root: creates the shared API client and one repository per table.");
            Line(0, "/// Create a single AppContainer when the app launches and hand it to RootView.");
            Line(0, "final class AppContainer {");
            Line(1, "let apiClient: APIClient");

            foreach (SQLTable table in _sQLTables)
            {
                Line(1, $"let {RepositoryProperty(table)}: {table.Name}Repository");
            }

            Line();
            Line(1, "init(configuration: APIConfiguration) {");
            Line(2, "let client = APIClient(configuration: configuration)");
            Line(2, "self.apiClient = client");

            foreach (SQLTable table in _sQLTables)
            {
                Line(2, $"self.{RepositoryProperty(table)} = Api{table.Name}Repository(client: client)");
            }

            Line(1, "}");
            Line(0, "}");

            WriteSingleFile("AppContainer");
        }

        internal static string RepositoryProperty(SQLTable table)
        {
            return table.Name.Decapitalise() + "Repository";
        }
    }
}
