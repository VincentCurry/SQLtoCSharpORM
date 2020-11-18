using System.Collections.Generic;
using System.Text;

namespace CodeGenerator
{
    public class ReactFormGenerator : Generator
    {
        const int removeLastCommaAndCarriagReturn = 5;
        public ReactFormGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            filePrefix = "";
            fileSuffix = "js";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine("import React, { Component } from 'react'");
            classText.AppendLine("import { extend }  from 'jquery'");

            classText.AppendLine($"export class {table.Name} extends Component {{");
            classText.AppendLine("\tconstructor(props) {");
            classText.AppendLine("\t\tsuper(props)");
            classText.AppendLine($"\t\tthis.state = {{");
            classText.AppendLine(TableColumnsCode(table, CreateStateCode));
            classText.Length -= removeLastCommaAndCarriagReturn;
            classText.AppendLine("");
            classText.AppendLine("\t\t};");


            classText.AppendLine("\t\tthis.handleInputChange = this.handleInputChange.bind(this);");
            classText.AppendLine("\t}");

            classText.AppendLine("");
            classText.AppendLine("\thandleInputChange = (event) => {");
            classText.AppendLine("\t\tconst target = event.target;");
            classText.AppendLine("\t\tconst value = target.type === 'checkbox' ? target.checked : target.type === 'number' ? parseInt(target.value) : target.value;");
            classText.AppendLine("\t\tconst name = target.name;");
            classText.AppendLine("");
            classText.AppendLine("\t\tthis.setState({");
            classText.AppendLine("\t\t\t[name]: value");
            classText.AppendLine("\t\t});");
            classText.AppendLine("\t}");
            classText.AppendLine("");

            classText.AppendLine("\thandleSubmit = (event) => {");
            classText.AppendLine("");
            classText.AppendLine($"\t\tvar {Library.LowerFirstCharacter(table.Name)} {{");
            classText.AppendLine(TableColumnsCode(table, CreateClassCode));
            classText.Length -= removeLastCommaAndCarriagReturn;
            classText.AppendLine("");
            classText.AppendLine("\t\t}");

            classText.AppendLine($"\t\tfetch(process.env.REACT_APP_API_ENDPOINT + '{Library.LowerFirstCharacter(table.Name)}', {{");
            classText.AppendLine($"\t\t\tmethod: 'POST',");
            classText.AppendLine($"\t\t\tbody: JSON.stringify({Library.LowerFirstCharacter(table.Name)}),");
            classText.AppendLine($"\t\t\theaders: {{");
            classText.AppendLine($"\t\t\t\t'Content-Type': 'application/json'");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine($"\t\t\t}}).then(function(response) {{");
            classText.AppendLine($"\t\t\tconsole.log(response)");
            classText.AppendLine($"\t\t\treturn response.json();");
            classText.AppendLine($"\t\t}});");
            classText.AppendLine("\t\tevent.preventDefault();");
            classText.AppendLine("\t}");
            classText.AppendLine("");

            classText.AppendLine("\trender() {");
            classText.AppendLine("\t\treturn (");
            classText.AppendLine("\t\t\t<form onSubmit={this.handleSubmit}>");

            classText.AppendLine(TableColumnsCode(table, CreateInputsCode));
            

            classText.AppendLine("\t\t\t\t<input type=\"submit\" value=\"Save\" />");


            classText.AppendLine("\t\t\t</form>");
            classText.AppendLine("\t\t);");
            classText.AppendLine("\t}");
            classText.AppendLine("}");

        }

        private delegate string CodeForColumn(SQLTableColumn column);

        private string CreateInputsCode(SQLTableColumn column)
        {
            StringBuilder inputsCode = new StringBuilder();

            string loweredName = Library.LowerFirstCharacter(column.Name);

            inputsCode.AppendLine("\t\t\t\t<label>");
            inputsCode.AppendLine("\t\t\t\t\t" + column.Name + ":");
            inputsCode.AppendLine($"\t\t\t\t\t<input type=\"{column.htmlInputFormType}\" name=\"{loweredName}\" value={{this.state.{loweredName}}} onChange={{this.handleInputChange}} />");
            inputsCode.AppendLine("\t\t\t\t</label>");

            return inputsCode.ToString();
        }

        private string CreateStateCode(SQLTableColumn column)
        {
            return $"\t\t\t{Library.LowerFirstCharacter(column.Name)}: {(column.htmlInputFormType==htmlFormValueType.text ? "''" : "null")},";
        }

        private string CreateClassCode(SQLTableColumn column)
        {
            return $"\t\t\t{Library.LowerFirstCharacter(column.Name)}: this.state.{Library.LowerFirstCharacter(column.Name)},";
        }

        private string TableColumnsCode(SQLTable table, CodeForColumn codeFunction)
        {
            StringBuilder columnsCode = new StringBuilder();

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    columnsCode.AppendLine(codeFunction(column));
                }
            }

            return columnsCode.ToString();
        }
    }
}
