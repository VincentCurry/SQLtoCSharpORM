using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CodeGenerator
{
    public class ReactFormGenerator : Generator
    {
        const int removeLastCommaAndCarriagReturn = 5;
        List<SQLForeignKeyRelation> foreignKeys;
        public ReactFormGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            filePrefix = "";
            fileSuffix = "js";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            foreignKeys = sQLForeignKeyRelationsForTable(table);

            classText.AppendLine("import React, { Component } from 'react'");

            classText.AppendLine(foreignKeys.Count > 0 ? "import { Dropdown } from 'semantic-ui-react'" : "");
            classText.AppendLine("");

            classText.AppendLine(ForeignKeysCode(foreignKeys, CreateLookupClass));
            classText.AppendLine("");

            classText.AppendLine($"export class {table.Name} extends Component {{");
            classText.AppendLine("\tconstructor(props) {");
            classText.AppendLine("\t\tsuper(props)");
            classText.AppendLine($"\t\tthis.state = {{");
            classText.Append(TableColumnsCode(table, CreateStateCode));
            classText.Append(ForeignKeysCode(foreignKeys, StateLookupStorage));
            classText.AppendLine("\t\t\tloading: true");
            
            classText.AppendLine("\t\t};");

            classText.AppendLine("\t\tthis.handleInputChange = this.handleInputChange.bind(this);");
            classText.AppendLine("\t}");
            classText.AppendLine("");

            classText.AppendLine("\tcomponentDidMount() {");

            classText.AppendLine(ForeignKeysCode(foreignKeys, GetLookupData));

                foreach (SQLTableColumn column in table.Columns)
                {
                    if (column.DataType == SQLDataTypes.dateTime)
                    {
                        classText.AppendLine($"\t\tthis.setDateControlToCurrentDateTime('{Library.LowerFirstCharacter(column.Name)}');");
                    }
                }
            classText.AppendLine("\t}");
            classText.AppendLine("");

            if (table.Columns.Where(co => co.DataType == SQLDataTypes.dateTime).Count() > 0)// there are dates in the selecction
            {
                classText.AppendLine("\tsetDateControlToCurrentDateTime(fieldIdentifier) {");
                classText.AppendLine($"\t\tvar tzoffset = (new Date()).getTimezoneOffset() * 60000;");
                classText.AppendLine($"\t\tvar localISOTime = (new Date(Date.now() - tzoffset)).toISOString().slice(0, -1);");
                classText.AppendLine($"\t\tvar localISOTimeWithoutSeconds = localISOTime.slice(0, 16);");
                classText.AppendLine("");
                classText.AppendLine("\t\tvar datePicker = document.querySelector('input[name=' + fieldIdentifier + ']');");
                classText.AppendLine("\t\tdatePicker.value = localISOTimeWithoutSeconds;");
                classText.AppendLine("\t\tthis.setState({");
                classText.AppendLine("\t\t\t[fieldIdentifier]: localISOTimeWithoutSeconds");
                classText.AppendLine("\t\t})");
                classText.AppendLine("\t}");
            }

            classText.AppendLine("");
            classText.AppendLine("\thandleInputChange = (event) => {");
            classText.AppendLine("\t\tconst target = event.target;");
            classText.AppendLine("\t\tconst value = target.type === 'checkbox' ? target.checked : target.type === 'number' ? parseInt(target.value) : target.value;");
            classText.AppendLine("\t\tconst name = target.name;");
            classText.AppendLine("");
            classText.AppendLine("\t\tthis.setState({");
            classText.AppendLine("\t\t\t[name]: value");
            classText.AppendLine("\t});");
            classText.AppendLine("\t}");
            classText.AppendLine("");

            classText.AppendLine("\thandleDropDownChange = (e, data) => {");
            classText.AppendLine("\t\tconst name = data.name;");

            classText.AppendLine("\t\tthis.setState({");
            classText.AppendLine("\t\t\t[name]: data.value");
            classText.AppendLine("\t\t})");
            classText.AppendLine("\t}");
            classText.AppendLine("");

            classText.AppendLine("\thandleSubmit = (event) => {");
            classText.AppendLine("");
            classText.AppendLine($"\t\tvar {Library.LowerFirstCharacter(table.Name)} = {{");
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

            classText.AppendLine(ForeignKeysCode(foreignKeys, GetLookupDataAndWriteToDropdown));

            classText.AppendLine("");
            classText.AppendLine("\trender() {");
            classText.AppendLine(ForeignKeysCode(foreignKeys, DropdownLoadingCodeForForeignKey));

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

            bool usedropdown = false;
            string tableName = "";
            foreach (SQLForeignKeyRelation foreignKeyRelation in foreignKeys)
            {
                if (foreignKeyRelation.ParentTableColum == column)
                {
                    usedropdown = true;
                    tableName = foreignKeyRelation.ReferencedTableColumn.TableName;
                }
            }

            if (!usedropdown)
            {
                inputsCode.AppendLine("\t\t\t\t\t" + column.Name + ":");
                inputsCode.AppendLine($"\t\t\t\t\t<input type=\"{column.htmlInputFormType}\" name=\"{loweredName}\" value={{this.state.{loweredName}}} onChange={{this.handleInputChange}} />");
            }
            else
            {
                loweredName = Library.LowerFirstCharacter(tableName);
                inputsCode.AppendLine($"\t\t\t\t\t{tableName}");
                inputsCode.AppendLine($"\t\t\t\t\t{{{loweredName}Dropdown}}");
            }

            inputsCode.AppendLine("\t\t\t\t</label>");

            return inputsCode.ToString();
        }

        private string CreateStateCode(SQLTableColumn column)
        {
            return $"\t\t\t{Library.LowerFirstCharacter(column.Name)}: {(column.htmlInputFormType == htmlFormValueType.text ? "''" : "null")},";
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

        private string DropdownLoadingCodeForForeignKey(SQLTableColumn column)
        {
            return $"\t\tlet {Library.LowerFirstCharacter(column.TableName)}Dropdown = this.state.loading ? <p><em>Loading</em></p> : this.Display{column.TableName}Dropdown();";
        }

        private string StateLookupStorage(SQLTableColumn column)
        {
            return $"\t\t\t{Library.LowerFirstCharacter(column.TableName)}s : [],";
        }

        private string GetLookupData(SQLTableColumn column)
        {
            return $"\t\tthis.get{column.TableName}Data();";
        }

        private string GetLookupDataAndWriteToDropdown(SQLTableColumn column)
        {
            StringBuilder lookupDataAndDropDown = new StringBuilder();

            string tableName = column.TableName;
            string loweredTableName = Library.LowerFirstCharacter(tableName);
            string primaryKey = Library.LowerFirstCharacter(column.ParentTable.PrimaryKey.Name);
            string firstColumn = Library.LowerFirstCharacter(column.ParentTable.Columns[1].Name);

            lookupDataAndDropDown.AppendLine($"\tasync get{tableName}Data() {{");
            lookupDataAndDropDown.AppendLine($"\t\tconst response = await fetch(process.env.REACT_APP_API_ENDPOINT + '{loweredTableName}');");
            lookupDataAndDropDown.AppendLine("\t\tconst data = await response.json();");
            lookupDataAndDropDown.AppendLine($"\t\tthis.setState({{ {loweredTableName}s: data.map(({loweredTableName}) => new {tableName}Option({loweredTableName}.{primaryKey}, {loweredTableName}.{primaryKey}, {loweredTableName}.{firstColumn})), loading: false }});");
            lookupDataAndDropDown.AppendLine("}");
            lookupDataAndDropDown.AppendLine("");
            lookupDataAndDropDown.AppendLine($"\tDisplay{tableName}Dropdown() {{");
            lookupDataAndDropDown.AppendLine("\t\tconst { value } = this.state;");
            lookupDataAndDropDown.AppendLine("\t\treturn (<Dropdown");
            lookupDataAndDropDown.AppendLine($"\t\t\tplaceholder='Select {tableName}'");
            lookupDataAndDropDown.AppendLine("\t\t\tfluid");
            lookupDataAndDropDown.AppendLine("\t\t\tselection");
            lookupDataAndDropDown.AppendLine("\t\t\tsearch");
            lookupDataAndDropDown.AppendLine($"\t\t\toptions={{this.state.{loweredTableName}s}}");
            lookupDataAndDropDown.AppendLine($"\t\t\tname=\"{primaryKey}\"");
            lookupDataAndDropDown.AppendLine("\t\t\tonChange={this.handleDropDownChange}");
            lookupDataAndDropDown.AppendLine("\t\t\tvalue={value}");
            lookupDataAndDropDown.AppendLine("\t\t/>");
            lookupDataAndDropDown.AppendLine("\t\t);");
            lookupDataAndDropDown.AppendLine("\t}");

            return lookupDataAndDropDown.ToString();
        }

        private string CreateLookupClass(SQLTableColumn column)
        {
            StringBuilder lookupClass = new StringBuilder();

            lookupClass.AppendLine($"export class {column.TableName}Option {{");

            lookupClass.AppendLine("\tconstructor(key, value, text) {");

            lookupClass.AppendLine("\t\tthis.text = text;");
            lookupClass.AppendLine("\t\tthis.key = key;");
            lookupClass.AppendLine("\t\tthis.value = value;");
            lookupClass.AppendLine("\t}");
            lookupClass.AppendLine("}");

            return lookupClass.ToString();
        }

        private string ForeignKeysCode(List<SQLForeignKeyRelation> sQLForeignKeyRelations,  CodeForColumn codeForForeignKeyColumn)
        {
            StringBuilder foreignKeysCode = new StringBuilder();

            foreach (SQLForeignKeyRelation foreignKeyRelation in foreignKeys)
            {
                foreignKeysCode.AppendLine(codeForForeignKeyColumn(foreignKeyRelation.ReferencedTableColumn));
            }

            return foreignKeysCode.ToString();
        }
    }
}
