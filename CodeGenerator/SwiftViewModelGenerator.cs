using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace CodeGenerator
{
    public class SwiftViewModelGenerator : Generator
    {
        public SwiftViewModelGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            fileNameSuffix = "ViewModel";
            fileSuffix = "swift";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine("import Foundation");
            classText.AppendLine();

            classText.AppendLine($"class {table.Name}ViewModel : ObservableObject {{");
            classText.AppendLine($"\t@Published var {Library.LowerFirstCharacter(table.Name)}s: [{table.Name}] = []");
            classText.AppendLine();

            classText.AppendLine($"\tfunc fetch{table.Name}s() {{");
            classText.AppendLine($"\t\tguard let url = URL(string: \"configuration.rootUrl/{table.Name}s\") else {{ return }}");
            classText.AppendLine();

            classText.AppendLine("\t\tURLSession.shared.dataTask(with: url) { data, response, error in");
            classText.AppendLine("\t\t\tif let data = data {");
            classText.AppendLine("\t\t\t\tdo {");
            classText.AppendLine($"\t\t\t\t\tlet decodedData = try JSONDecoder().decode([{table.Name}].self, from: data)");
            classText.AppendLine("\t\t\t\t\tDispatchQueue.main.async {");
            classText.AppendLine($"\t\t\t\t\t\tself.{Library.LowerFirstCharacter(table.Name)}s = decodedData");
            classText.AppendLine("\t\t\t\t\t}");
            classText.AppendLine("\t\t\t\t} catch {");
            classText.AppendLine("\t\t\t\t\tprint(\"Error decoding JSON: \\(error.localizedDescription)\")");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t\t} else if let error = error {");
            classText.AppendLine("\t\t\t\tprint(\"Network error: \\(error.localizedDescription)\")");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}.resume()");
            classText.AppendLine("\t}");
            classText.AppendLine();

            classText.AppendLine($"\tfunc save{table.Name} ({table.Name}: {Library.LowerFirstCharacter(table.Name)}) async {{");
            classText.AppendLine($"\t\tguard let encoded = try? JSONEncoder().encode({Library.LowerFirstCharacter(table.Name)}) else {{");
            classText.AppendLine($"\t\t\tprint(\"Failed to encode {Library.LowerFirstCharacter(table.Name)}\")");
            classText.AppendLine("\t\t\treturn");
            classText.AppendLine("\t\t}");
            classText.AppendLine();

            classText.AppendLine($"\t\tlet url = URL(string: \"https://reqres.in/api/{Library.LowerFirstCharacter(table.Name)}\")!");
            classText.AppendLine("\t\tvar request = URLRequest(url: url)");
            classText.AppendLine("\t\trequest.setValue(\"application/json\", forHTTPHeaderField: \"Content-Type\")");
            classText.AppendLine("\t\trequest.httpMethod = \"POST\"");
            classText.AppendLine("\t}");
            classText.AppendLine();

            classText.AppendLine("\tdo {");
            classText.AppendLine("\t\tlet (data, _) = try await URLSession.shared.upload(for: request, from: encoded)");
            classText.AppendLine("\t\t// handle the result");
            classText.AppendLine("\t} catch {");
            classText.AppendLine("\t\tprint(\"Saving failed: \\(error.localizedDescription)\")");
            classText.AppendLine("\t}");
            classText.AppendLine("}");
        }
    }
}
