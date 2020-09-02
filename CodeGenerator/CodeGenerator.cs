using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using DataServer;
using System.IO;

namespace CodeGenerator
{

    public class SQLStatementGenerator
    {
        List<SQLTable> tablesToGenerate;
        string go = "GO";
        string end = "END";
        string sqlAs = "AS";
        string begin = "BEGIN";
        string setNoCount = "SET NOCOUNT ON;";
        string procedureDivider = Environment.NewLine + "--__________________________________________________" + Environment.NewLine;

        public SQLStatementGenerator(List<SQLTable> tables)
        {
            tablesToGenerate = tables;
        }

        public void GenerateSQLStatements(List<SQLTable> tables, string destinationFile)
        {
            StringBuilder sqlStatement = new StringBuilder();

            foreach (SQLTable table in tables)
            {
                sqlStatement.AppendLine("GO");
                sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "GetByID]" + Environment.NewLine);
                sqlStatement.Append("@" + table.PrimaryKey.Name + " " + table.PrimaryKey.DataType.ToString() + Environment.NewLine);
                GenerateSelectStatement(table, sqlStatement);
                sqlStatement.AppendLine(sqlRowIdentifier(table.PrimaryKey.Name));
                sqlStatement.AppendLine("END");
                sqlStatement.Append(procedureDivider);
                sqlStatement.AppendLine("GO");
                sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "GetAll]" + Environment.NewLine);
                GenerateSelectStatement(table, sqlStatement);
                sqlStatement.AppendLine("END");
                GenerateInsertStatement(table, sqlStatement);
                sqlStatement.Append(procedureDivider);
                GenerateUpdateStatement(table, sqlStatement);
                sqlStatement.Append(procedureDivider);
                GenerateDeleteStatement(table, sqlStatement);
                sqlStatement.Append(procedureDivider);
                
            }

            TextWriter writer = File.CreateText(destinationFile);

            writer.Write(sqlStatement.ToString());

            writer.Close();
            
        }

        void GenerateSelectStatement(SQLTable table, StringBuilder sqlStatement)
        {

            sqlStatement.Append("AS" + Environment.NewLine);
            sqlStatement.Append("BEGIN" + Environment.NewLine);

            sqlStatement.Append("SET NOCOUNT ON;	" + Environment.NewLine);

            sqlStatement.Append("SELECT	" + Environment.NewLine);

            bool appendComma = false;

            foreach (SQLTableColumn column in table.Columns)
            {
                if (appendComma)
                    sqlStatement.Append("," + Environment.NewLine);

                sqlStatement.Append("[" + column.Name + "]");

                appendComma = true;
            }

            sqlStatement.Append(Environment.NewLine + "FROM [" + table.Name+ "]" + Environment.NewLine );
            sqlStatement.AppendLine("");
        }

       /* void GenerateForeignKeyStatements(SQLTable table, StringBuilder sqlStatement)
        {
            foreach (SQLTableColumn tableColumn in table.Columns)
            {
                foreach (SQLForeignKeyRelation foreignKey in tableColumn.ForeignKeys)
                {
                    GenerateSelectStatement(foreignKey., sqlStatement)
                }
            }
        }
        */
        void GenerateInsertStatement(SQLTable table, StringBuilder sqlStatement)
        {
            sqlStatement.AppendLine("GO");
            sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "Insert]" + Environment.NewLine);

            bool appendComma = false;

            //This part declares the constants for the stored procdure. The one parameter we don't want to have is a unique identifier as this will be generated later in the statement.
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!(column.PrimaryKey && column.DataType.ToString() == SQLDataTypes.uniqueIdentifier))
                {
                    if (appendComma)
                        sqlStatement.Append("," + Environment.NewLine);

                    sqlStatement.Append("@" + column.Name + " " + column.DataType.ToString());

                    if (column.DataType == SQLDataTypes.varChar)
                        sqlStatement.Append(" (" + column.MaximumLength.ToString() + ")");

                    //set primary key integers as output parameters
                    if (column.PrimaryKey)
                        sqlStatement.Append(" OUTPUT");

                    appendComma = true;
                }
            }

            sqlStatement.Append(Environment.NewLine + "AS" + Environment.NewLine + "BEGIN" + Environment.NewLine);

            if (table.PrimaryKey.DataType.ToString() == SQLDataTypes.uniqueIdentifier)
            {
                sqlStatement.Append("DECLARE @" + table.PrimaryKey.Name + " uniqueidentifier" + Environment.NewLine);
                sqlStatement.Append("SET @" + table.PrimaryKey.Name + " = NEWID()" + Environment.NewLine);
            }

            sqlStatement.Append("INSERT INTO [" + table.Name + "]" + Environment.NewLine + "(");

            appendComma = false;
            
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey || column.DataType.ToString() == SQLDataTypes.uniqueIdentifier)
                {
                    if (appendComma)
                        sqlStatement.Append("," + Environment.NewLine);

                    sqlStatement.Append("[" + column.Name + "]");

                    appendComma = true;
                }
            }

            sqlStatement.Append(Environment.NewLine + ")" + Environment.NewLine + "VALUES" + Environment.NewLine + "(" + Environment.NewLine);

            appendComma = false;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey || column.DataType.ToString() == SQLDataTypes.uniqueIdentifier)
                {
                    if (appendComma)
                        sqlStatement.Append("," + Environment.NewLine);

                    sqlStatement.Append("@" + column.Name);

                    appendComma = true;
                }
            }

            sqlStatement.Append(Environment.NewLine + ")" + Environment.NewLine);

            if (table.PrimaryKey.DataType.ToString() == SQLDataTypes.uniqueIdentifier)
            {
                sqlStatement.AppendLine("SELECT @" + table.PrimaryKey.Name);
            }
            else
            {
                sqlStatement.AppendLine("SELECT @" + table.PrimaryKey.Name + " = @@IDENTITY");
                sqlStatement.AppendLine("RETURN @" + table.PrimaryKey.Name);
            }

            sqlStatement.AppendLine("END");
        }

        void GenerateUpdateStatement(SQLTable table, StringBuilder sqlStatement)
        {
            sqlStatement.AppendLine("GO");
            sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "Update]" + Environment.NewLine);

            bool appendComma = false;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (appendComma)
                    sqlStatement.Append("," + Environment.NewLine);

                sqlStatement.Append("@" + column.Name + " " + column.DataType.ToString());

                if (column.DataType == SQLDataTypes.varChar)
                    sqlStatement.Append(" (" + column.MaximumLength.ToString() + ")");

                appendComma = true;
            }

            sqlStatement.Append(Environment.NewLine + "AS" + Environment.NewLine + "BEGIN" + Environment.NewLine);

            sqlStatement.Append("UPDATE [" + table.Name + "]" + Environment.NewLine);
            sqlStatement.Append("SET" + Environment.NewLine);

            appendComma = false;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    if (appendComma)
                        sqlStatement.Append("," + Environment.NewLine);

                    sqlStatement.Append("[" + column.Name + "] = @" + column.Name);

                    appendComma = true;
                }
            }

            sqlStatement.AppendLine(sqlRowIdentifier(table.PrimaryKey.Name));
            sqlStatement.AppendLine("END");
        }

        void GenerateDeleteStatement(SQLTable table, StringBuilder sqlStatement)
        {
            sqlStatement.AppendLine(go);
            sqlStatement.AppendLine("CREATE PROCEDURE [dbo].[" + table.Name + "Delete]" + Environment.NewLine);

            sqlStatement.AppendLine($"@{table.PrimaryKey.Name} {table.PrimaryKey.DataType}");
            sqlStatement.AppendLine(sqlAs);
            sqlStatement.AppendLine(begin);
            sqlStatement.AppendLine(setNoCount);

            sqlStatement.AppendLine($"DELETE FROM [{table.Name}]");
            sqlStatement.AppendLine(sqlRowIdentifier(table.PrimaryKey.Name));

            sqlStatement.AppendLine(end);

        }

        string sqlRowIdentifier(string columnName)
        {
            return Environment.NewLine + "WHERE [" + columnName + "] = @" + columnName;
        }
    }

}
