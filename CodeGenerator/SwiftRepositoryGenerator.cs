using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// Generates a repository per table: a protocol the view models depend on, and an implementation
    /// that talks to the ASP.NET controller produced by MvcApiControllerGenerator
    /// (GET api/x, GET api/x/{id}, POST api/x, PUT api/x/{id}, DELETE api/x/{id}).
    /// </summary>
    public class SwiftRepositoryGenerator : SwiftGenerator
    {
        public SwiftRepositoryGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "Repository";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string keyType = table.PrimaryKey.iosDataType;

            Line(0, "import Foundation");
            Line();

            Line(0, $"protocol {table.Name}Repository {{");
            Line(1, $"func fetchAll() async throws -> [{table.Name}]");
            Line(1, $"func fetch(id: {keyType}) async throws -> {table.Name}");
            Line(1, "/// Creates a row and returns its new primary key.");
            Line(1, $"func create(_ input: {table.Name}Input) async throws -> {keyType}");
            Line(1, $"func update(id: {keyType}, with input: {table.Name}Input) async throws");
            Line(1, $"func delete(id: {keyType}) async throws");
            Line(0, "}");
            Line();

            Line(0, $"final class Api{table.Name}Repository: {table.Name}Repository {{");
            Line(1, $"private let resource = \"{table.Name.ToLower()}\"");
            Line(1, "private let client: APIClient");
            Line();
            Line(1, "init(client: APIClient) {");
            Line(2, "self.client = client");
            Line(1, "}");
            Line();
            Line(1, $"func fetchAll() async throws -> [{table.Name}] {{");
            Line(2, "try await client.get(resource)");
            Line(1, "}");
            Line();
            Line(1, $"func fetch(id: {keyType}) async throws -> {table.Name} {{");
            Line(2, "try await client.get(resource, id: \"\\(id)\")");
            Line(1, "}");
            Line();
            Line(1, $"func create(_ input: {table.Name}Input) async throws -> {keyType} {{");
            Line(2, "try await client.post(resource, body: input)");
            Line(1, "}");
            Line();
            Line(1, $"func update(id: {keyType}, with input: {table.Name}Input) async throws {{");
            Line(2, "try await client.put(resource, id: \"\\(id)\", body: input)");
            Line(1, "}");
            Line();
            Line(1, $"func delete(id: {keyType}) async throws {{");
            Line(2, "try await client.delete(resource, id: \"\\(id)\")");
            Line(1, "}");
            Line(0, "}");
        }
    }
}
