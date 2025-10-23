using System.Collections.Generic;
using System.Text;

namespace CodeGenerator
{
    public class SwiftDetailViewGenerator : Generator
    {
        public SwiftDetailViewGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "DetailView";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"struct {table.Name}DetailView: View {{");
            classText.AppendLine($"\tvar item: {table.Name}");
            classText.AppendLine();
            classText.AppendLine("\tvar body: some View {");
            classText.AppendLine("\t\tForm {");

            classText.AppendLine(Library.TableColumnsCode(table.Columns, DetailViewField, includePrimaryKey: false, appendCommas: false, singleLine: false));

            classText.AppendLine("\t\t}");
            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }

        private string DetailViewField(SQLTableColumn column)
        {
            StringBuilder detailViewFieldBuilder = new StringBuilder();

            detailViewFieldBuilder.AppendLine($"\t\t\tHStack {{");
            detailViewFieldBuilder.AppendLine($"\t\t\t\tText(\"{column.Name}:\")");
            detailViewFieldBuilder.AppendLine($"\t\t\t\tSpacer()");
            detailViewFieldBuilder.AppendLine($"\t\t\t\tText(\"\\(item.{column.Name.DecapitaliseAndUpdateSwiftKeywords()})\")");
            detailViewFieldBuilder.AppendLine("\t\t\t}");

            return detailViewFieldBuilder.ToString();
        }
    }
}
