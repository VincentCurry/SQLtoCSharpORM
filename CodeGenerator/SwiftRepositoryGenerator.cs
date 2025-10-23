using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftRepositoryGenerator : Generator
    {
        public SwiftRepositoryGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "Repository";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"protocol {table.Name}Repository {{");
            classText.AppendLine($"    func fetchAll() async throws -> [{table.Name}]");
            classText.AppendLine($"    func fetchById(id: Int) async throws -> {table.Name}?");
            classText.AppendLine($"    func save(item: {table.Name}) async throws");
            classText.AppendLine($"    func delete(id: Int) async throws");
            classText.AppendLine("}");
            classText.AppendLine();
            classText.AppendLine($"class Api{table.Name}Repository: {table.Name}Repository {{");
            classText.AppendLine($"    func fetchAll() async throws -> [{table.Name}] {{");
            classText.AppendLine("        // TODO: Implement API call");
            classText.AppendLine("        return []");
            classText.AppendLine("    }");
            classText.AppendLine("}");
        }
    }
}
