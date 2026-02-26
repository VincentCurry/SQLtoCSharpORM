using System.Collections.Generic;

namespace CodeGenerator
{
    public class SwiftFormViewGenerator : Generator
    {
        public SwiftFormViewGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "FormView";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"struct {table.Name}FormView: View {{");
            classText.AppendLine($"    @State private var formState = {table.Name}FormState()");
            classText.AppendLine();
            classText.AppendLine("    var body: some View {");
            classText.AppendLine($"        {table.Name}FormFields(state: $formState)");
            classText.AppendLine("    }");
            classText.AppendLine("}");
        }
    }
}
