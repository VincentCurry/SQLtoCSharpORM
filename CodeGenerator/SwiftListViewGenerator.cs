using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public class SwiftListViewGenerator : Generator
    {
        public SwiftListViewGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "ListView";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"struct {table.Name}ListView: View {{");
            classText.AppendLine($"    @StateObject private var viewModel = {table.Name}ViewModel(repository: Api{table.Name}Repository())");
            classText.AppendLine();
            classText.AppendLine("    var body: some View {");
            classText.AppendLine("        NavigationView {");
            classText.AppendLine($"            List(viewModel.items) {{ item in");
            classText.AppendLine("                VStack(alignment: .leading) {");

            foreach (var col in table.Columns.Take(2)) // show first 2 cols
            {
                string propertyName = char.ToLowerInvariant(col.Name[0]) + col.Name.Substring(1);
                classText.AppendLine($"                    Text(\"\\(item.{propertyName})\")");
            }

            classText.AppendLine("                }");
            classText.AppendLine("            }");
            classText.AppendLine($"            .navigationTitle(\"{table.Name}s\")");
            classText.AppendLine("            .task { await viewModel.load() }");
            classText.AppendLine("        }");
            classText.AppendLine("    }");
            classText.AppendLine("}");
        }
    }
}
