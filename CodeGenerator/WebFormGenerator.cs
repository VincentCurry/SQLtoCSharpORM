using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CodeGenerator
{
    public class WebFormGenerator
    {
        public void GenerateForms(List<SQLTable> tables, string destinationFolder, string masterFileName, string bodyContentControlID, string nameSpace, string omNameSpace)
        {
            foreach (SQLTable table in tables)
            {

                WritePageCode(table, true, masterFileName, destinationFolder, bodyContentControlID, nameSpace);
                WritePageCode(table, false, masterFileName, destinationFolder, bodyContentControlID, nameSpace);
                WriteReadOnlyCodeBehind(table, destinationFolder, nameSpace, omNameSpace);
                WriteEditCodeBehind(table, destinationFolder, nameSpace, omNameSpace);
            }
        }

        private void WritePageCode(SQLTable table, bool EditPage, string masterPageName, string destinationFolder, string bodyContentControlID, string nameSpace)
        {
            StringBuilder formText = new StringBuilder();
            StringBuilder designerText = new StringBuilder();


            AppendStartOfDesigner(designerText, nameSpace, table.Name);

            formText.Append("<%@ Page Title=\"" + table.Name + "\"  Language=\"C#\" MasterPageFile=\"~/" + masterPageName + "\" AutoEventWireup=\"true\" CodeFile=\"");
            if (EditPage)
            {
                formText.Append(table.Name + "Edit");
                formText.AppendLine(".aspx.cs\" Inherits=\"" + nameSpace + "." + table.Name + "Edit\" %>");
            }
            else
            {
                formText.Append(table.Name);
                formText.AppendLine(".aspx.cs\" Inherits=\"" + nameSpace + "." + table.Name + "\" %>");
            }

            formText.AppendLine("");
                formText.AppendLine("<asp:Content ID=\"headerContent\" ContentPlaceHolderID=\"" + bodyContentControlID + "\" Runat=\"Server\">");
                formText.AppendLine("");

                formText.AppendLine("<table style=\"width: 100%;\">");

                foreach (SQLTableColumn column in table.Columns)
                {
                    if (!column.PrimaryKey)
                    {
                        formText.AppendLine("<tr>");
                        formText.AppendLine("<td><asp:Label ID=\"lbl" + column.Name + "\" runat=\"server\" Text=\"" + column.Name + "\" CssClass=\"dataLabel\"></asp:Label></td>");
                        designerText.AppendLine("protected global::System.Web.UI.WebControls.Label lbl" + column.Name + ";");
                        if (EditPage)
                        {
                            formText.AppendLine("<td><asp:TextBox ID=\"txt" + column.Name + "\" runat=\"server\" MaxLength=\"" + column.MaximumLength + "\"></asp:TextBox>");
                            designerText.AppendLine("protected global::System.Web.UI.WebControls.TextBox txt" + column.Name + ";");
                            if (!column.Nullable)
                            {
                                formText.AppendLine("<asp:RequiredFieldValidator ID=\"rqd" + column.Name + "\" runat=\"server\" ControlToValidate=\"txt" + column.Name + "\" ErrorMessage=\"You must enter a " + column.Name + "\">*</asp:RequiredFieldValidator>");
                                designerText.AppendLine("protected global::System.Web.UI.WebControls.RequiredFieldValidator rqd" + column.Name + ";");
                            }
                        }
                        else
                        {
                            formText.AppendLine("<td><asp:Label ID=\"lblSaved" + column.Name + "\" runat=\"server\" Text=\"Saved " + column.Name + "\" ></asp:Label>");
                            designerText.AppendLine("protected global::System.Web.UI.WebControls.Label lblSaved" + column.Name + ";");
                        }
                        formText.AppendLine("</td>");
                        formText.AppendLine("</tr>");
                    }
                }
                formText.AppendLine("</table>");

                if (EditPage)
                {
                    formText.AppendLine("<asp:Button ID=\"btnSave" + table.Name + "\" runat=\"server\" onclick=\"btnSave" + table.Name + "_Click\" Text=\"Save " + table.Name + "\" />");
                    formText.AppendLine("<asp:Button ID=\"btnCancel\" runat=\"server\" Text=\"Cancel\" onclick=\"btnCancel_Click\" CausesValidation=\"false\" />");
                }
                formText.AppendLine("</asp:Content>");

                AppendEndOfCodeBehind(designerText);

                if (EditPage)
                {
                    WriteCodeToFile(destinationFolder + table.Name + "Edit.aspx", formText);
                    //WriteCodeToFile(destinationFolder + table.Name + "Edit.aspx.designer.cs", designerText);
                }
                else
                {
                    WriteCodeToFile(destinationFolder + table.Name + ".aspx", formText);
                   // WriteCodeToFile(destinationFolder + table.Name + ".aspx.designer.cs", designerText);
                }
        }

        private void WriteReadOnlyCodeBehind(SQLTable table, string destinationFolder, string nameSpace, string omNameSpace)
        {
            StringBuilder codeBehindText = new StringBuilder();

            AppendStartOfCodeBehind(codeBehindText, nameSpace, false, omNameSpace, table);

            if (table.PrimaryKey.DataType == SQLDataTypes.uniqueIdentifier)
            {
                codeBehindText.AppendLine("Guid " + Library.LowerFirstCharacter(table.PrimaryKey.Name) + " = new Guid(Request.QueryString[\"" + table.Name + "ID parameter\"]);");
            }
            else
            {
                codeBehindText.AppendLine(Library.LowerFirstCharacter(table.PrimaryKey.Name) + " = Convert.ToInt64(Request.QueryString[\"" + table.Name + "ID parameter\"]]);");
            }
            codeBehindText.AppendLine(Library.LowerFirstCharacter(table.Name) + " = " + omNameSpace + "." + table.Name + ".Get" + table.Name + "ByID(" + Library.LowerFirstCharacter(table.PrimaryKey.Name) + ");");
            codeBehindText.AppendLine("Write" + table.Name + "ToReadOnlyControls(" + Library.LowerFirstCharacter(table.Name) + ");");
            codeBehindText.AppendLine("}");
         
            codeBehindText.AppendLine("private void Write" + table.Name + "ToReadOnlyControls(" + omNameSpace + "." + table.Name + " " + Library.LowerFirstCharacter(table.Name) + ")");
            codeBehindText.AppendLine("{");

            foreach (SQLTableColumn column in table.Columns)
            {
                if(!column.PrimaryKey)
                    codeBehindText.AppendLine("lblSaved" + column.Name + ".Text = " + Library.LowerFirstCharacter(table.Name) + "." + column.Name + ";");
            }

            codeBehindText.AppendLine("}");

            AppendEndOfCodeBehind(codeBehindText);

            WriteCodeToFile(destinationFolder + table.Name + ".aspx.cs", codeBehindText);
        }

        private void WriteEditCodeBehind(SQLTable table, string destinationFolder, string nameSpace, string omNameSpace)
        {
            StringBuilder codeBehindText = new StringBuilder();


            AppendStartOfCodeBehind(codeBehindText, nameSpace, true, omNameSpace, table);

            codeBehindText.AppendLine("if (!IsPostBack)");
            codeBehindText.AppendLine("Write" + table.Name + "ToControls(" + Library.LowerFirstCharacter(table.Name) + ");");
            codeBehindText.AppendLine("}");


            codeBehindText.AppendLine("private void Write" + table.Name + "ToControls(" + omNameSpace + "." + table.Name + " " + Library.LowerFirstCharacter(table.Name) + ")");
            codeBehindText.AppendLine("{");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                    codeBehindText.AppendLine("txt" + column.Name + ".Text = " + Library.LowerFirstCharacter(table.Name) + "." + column.Name + ";");
            }

            codeBehindText.AppendLine("}");

            codeBehindText.AppendLine("protected void btnSave" + table.Name + "_Click(object sender, EventArgs e)");

            codeBehindText.AppendLine("{");
            codeBehindText.AppendLine("if (Page.IsValid)");



            codeBehindText.AppendLine("{");
            codeBehindText.AppendLine("if ( " + Library.LowerFirstCharacter(table.Name) + " == null)");
            codeBehindText.AppendLine("{");
            codeBehindText.AppendLine("     " + Library.LowerFirstCharacter(table.Name) + " = new " + omNameSpace + "." + table.Name + "();");
            codeBehindText.AppendLine("}");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                    codeBehindText.AppendLine(" " + Library.LowerFirstCharacter(table.Name) + "." + column.Name + " = txt" + column.Name + ".Text;");
            }

            codeBehindText.AppendLine(Library.LowerFirstCharacter(table.Name) + ".Save" + table.Name + "();");

            codeBehindText.AppendLine("}");
            codeBehindText.AppendLine("}");

            codeBehindText.AppendLine("protected void btnCancel_Click(object sender, EventArgs e)");
            codeBehindText.AppendLine("{");
            codeBehindText.AppendLine("Response.Redirect(\"~/Appropriate page to redirect to\");");
            codeBehindText.AppendLine("}");

            AppendEndOfCodeBehind(codeBehindText);

            WriteCodeToFile(destinationFolder + table.Name + "Edit.aspx.cs", codeBehindText);
        }

        private void WriteListPage(SQLTable table, string destinationFolder, string masterPageName, string bodyContentID, string nameSpace, string omNameSpace)
        {
            //Unfortunately this function isn't much use at the moment; it may be returned to at a later date.
            StringBuilder listPage = new StringBuilder();

            listPage.AppendLine("<%@ Page Title=\""+table.Name +"s\" Language=\"C#\" MasterPageFile=\"~/" + masterPageName + "\" AutoEventWireup=\"true\" CodeFile=\""+table.Name +"s.aspx.cs\" Inherits=\""+table.Name +"\" %>");

            listPage.AppendLine("<asp:Content ID=\"cntAdvertisersHead\" ContentPlaceHolderID=\"HeadContent\" Runat=\"Server\">");
            listPage.AppendLine("</asp:Content>");
            listPage.AppendLine("<asp:Content ID=\"cntAdvertisersBody\" ContentPlaceHolderID=\"MainContent\" Runat=\"Server\">");
            listPage.AppendLine("    <asp:ObjectDataSource ID=\"odsAdvertisers\" runat=\"server\" ");
            listPage.AppendLine("        SelectMethod=\"GetAllAdvertisers\" TypeName=\"InSizeMediaOM.Advertiser\"></asp:ObjectDataSource>");
            listPage.AppendLine("    <asp:GridView ID=\"gvwAdvertisers\" runat=\"server\" AutoGenerateColumns=\"False\" ");
            listPage.AppendLine("        DataSourceID=\"odsAdvertisers\">");
            listPage.AppendLine("        <Columns>");
            listPage.AppendLine("            <asp:TemplateField HeaderText=\"Name\" SortExpression=\"Name\">");
            listPage.AppendLine("                <ItemTemplate>");
            listPage.AppendLine("                   <asp:HyperLink ID=\"hlAdvertiser\" runat=\"server\" NavigateUrl='<%# \"advertiser.aspx?adv=\" + Eval(\"AdvertiserID\") %>' Text='<% #Bind(\"Name\") %>'></asp:HyperLink>");
            listPage.AppendLine("                </ItemTemplate>");
            listPage.AppendLine("            </asp:TemplateField>");
            listPage.AppendLine("            <asp:BoundField DataField=\"Name\" HeaderText=\"Name\" SortExpression=\"Name\" />");
            listPage.AppendLine("            <asp:TemplateField HeaderText=\"Name\"></asp:TemplateField>");
            listPage.AppendLine("        </Columns>");
            listPage.AppendLine("    </asp:GridView>");
            listPage.AppendLine("</asp:Content>");
        }

        private void AppendStartOfCodeBehind(StringBuilder codeToAppendTo, string nameSpace,  bool editFile, string omNameSpace, SQLTable table)
        {
            codeToAppendTo.AppendLine("using System;");
            codeToAppendTo.AppendLine("using System.Collections.Generic;");
            codeToAppendTo.AppendLine("using System.Linq;");
            codeToAppendTo.AppendLine("using System.Web;");
            codeToAppendTo.AppendLine("using System.Web.UI;");
            codeToAppendTo.AppendLine("using System.Web.UI.WebControls;");

            codeToAppendTo.AppendLine("namespace " + nameSpace);
            codeToAppendTo.AppendLine("{");
            
            if (editFile)
                codeToAppendTo.AppendLine("public partial class " + table.Name + "Edit : System.Web.UI.Page");
            else
                codeToAppendTo.AppendLine("public partial class " + table.Name + " : System.Web.UI.Page");

            codeToAppendTo.AppendLine("{");

            codeToAppendTo.AppendLine(table.PrimaryKey.cSharpDataType + " " + Library.LowerFirstCharacter(table.PrimaryKey.Name) + ";"); 
            codeToAppendTo.AppendLine("private " + omNameSpace + "." + table.Name + " " + Library.LowerFirstCharacter(table.Name) + ";");
            codeToAppendTo.AppendLine("protected void Page_Load(object sender, EventArgs e)");
            codeToAppendTo.AppendLine("{");
        }

        private void AppendEndOfCodeBehind(StringBuilder codeToAppendTo)
        {
            codeToAppendTo.AppendLine("}");
            codeToAppendTo.AppendLine("}");
        }

        private void AppendStartOfDesigner(StringBuilder designerCode, string nameSpace, string className)
        {

            designerCode.AppendLine("namespace " + nameSpace + " {");
            designerCode.AppendLine("public partial class " + className + " {");
        }

        private void WriteCodeToFile(string fileName, StringBuilder code)
        {
            TextWriter writer = File.CreateText(fileName);

            writer.Write(code.ToString());

            writer.Close();
        }
    }
}
