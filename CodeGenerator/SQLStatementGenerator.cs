using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeGenerator
{

    public class SQLStatementGenerator : Generator
    {
        List<SQLTable> tablesToGenerate;
        string go = "GO";
        string end = "END";
        string sqlAs = "AS";
        string begin = "BEGIN";
        string setNoCount = "SET NOCOUNT ON;";
        string procedureDivider = Environment.NewLine + "--__________________________________________________" + Environment.NewLine;

        string _spUserName;

        public SQLStatementGenerator(List<SQLTable> tables) : base(tables, "", "")
        {
            tablesToGenerate = tables;
        }

        public void GenerateSQLStatements(List<SQLTable> tables, string destinationFile, string executeSPPermissionFile, string spUserName)
        {
            StringBuilder sqlStatement = new StringBuilder();
            StringBuilder executeSPPermission = new StringBuilder();
            _spUserName = spUserName;

            foreach (SQLTable table in tables)
            {
                sqlStatement.AppendLine("GO");
                sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "GetByID]" + Environment.NewLine);
                executeSPPermission.Append(ExecutePermissionStatement(table.Name + "GetByID"));
                sqlStatement.Append("@" + table.PrimaryKey.Name + " " + table.PrimaryKey.DataType.ToString() + Environment.NewLine);
                GenerateSelectStatement(table, sqlStatement);
                sqlStatement.AppendLine(sqlRowIdentifier(table.PrimaryKey.Name));
                sqlStatement.AppendLine("END");
                sqlStatement.Append(procedureDivider);
                sqlStatement.AppendLine("GO");
                sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "GetAll]" + Environment.NewLine);
                executeSPPermission.Append(ExecutePermissionStatement(table.Name + "GetAll"));
                GenerateSelectStatement(table, sqlStatement);
                sqlStatement.AppendLine("END");
                GenerateForeignKeyStatements(table, sqlStatement, executeSPPermission);
                GenerateInsertStatement(table, sqlStatement, executeSPPermission);
                sqlStatement.Append(procedureDivider);
                GenerateUpdateStatement(table, sqlStatement, executeSPPermission);
                sqlStatement.Append(procedureDivider);
                GenerateDeleteStatement(table, sqlStatement, executeSPPermission);
                sqlStatement.Append(procedureDivider);
                GenerateForeignKeyDeleteStatements(table, sqlStatement);
                
            }

            TextWriter writer = File.CreateText(destinationFile);

            writer.Write(sqlStatement.ToString());

            writer.Close();

            writer = File.CreateText(executeSPPermissionFile);

            writer.Write(executeSPPermission.ToString());

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

        void GenerateForeignKeyStatements(SQLTable table, StringBuilder sqlStatement, StringBuilder executeSPPermission)
        {
            List<SQLForeignKeyRelation> foreignKeys = sQLForeignKeyRelationsForTable(table);

            foreach (SQLForeignKeyRelation foreignKey in foreignKeys)
            {
                SQLTableColumn column = foreignKey.ReferencedTableColumn;

                sqlStatement.AppendLine(go);
                sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "GetBy"+ column.Name + "]" + Environment.NewLine);
                executeSPPermission.Append(ExecutePermissionStatement(table.Name + "GetBy" + column.Name));
                sqlStatement.Append("@" + column.Name + " " + column.DataType.ToString() + Environment.NewLine);
                GenerateSelectStatement(table, sqlStatement);

                sqlStatement.AppendLine(sqlRowIdentifier(column.Name));

                sqlStatement.AppendLine(end);
                sqlStatement.AppendLine(procedureDivider);
            }
            
        }
       
        void GenerateInsertStatement(SQLTable table, StringBuilder sqlStatement, StringBuilder executeSPPermission)
        {
            sqlStatement.AppendLine("GO");
            sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "Insert]" + Environment.NewLine);
            executeSPPermission.Append(ExecutePermissionStatement(table.Name + "Insert"));

            bool appendComma = false;

            //This part declares the constants for the stored procdure. The one parameter we don't want to have is a unique identifier as this will be generated later in the statement.
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!(column.PrimaryKey && column.DataType.ToString() == SQLDataTypes.uniqueIdentifier))
                {
                    if (appendComma)
                        sqlStatement.Append("," + Environment.NewLine);

                    sqlStatement.Append("@" + column.Name + " " + column.DataType.ToString());

                    sqlStatement.Append(column.SizeForSQLProcedureParameters);

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

        void GenerateUpdateStatement(SQLTable table, StringBuilder sqlStatement, StringBuilder executeSPPermission)
        {
            sqlStatement.AppendLine("GO");
            sqlStatement.Append("CREATE PROCEDURE [dbo].[" + table.Name + "Update]" + Environment.NewLine);
            executeSPPermission.Append(ExecutePermissionStatement($"{table.Name}Update"));

            bool appendComma = false;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (appendComma)
                    sqlStatement.Append("," + Environment.NewLine);

                sqlStatement.Append("@" + column.Name + " " + column.DataType.ToString());

                sqlStatement.Append(column.SizeForSQLProcedureParameters);

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

        void GenerateDeleteStatement(SQLTable table, StringBuilder sqlStatement, StringBuilder executeSPPermission)
        {
            sqlStatement.AppendLine(go);
            sqlStatement.AppendLine("CREATE PROCEDURE [dbo].[" + table.Name + "Delete]" + Environment.NewLine);
            executeSPPermission.Append(ExecutePermissionStatement(table.Name + "Delete"));

            sqlStatement.AppendLine($"@{table.PrimaryKey.Name} {table.PrimaryKey.DataType}");
            sqlStatement.AppendLine(sqlAs);
            sqlStatement.AppendLine(begin);
            sqlStatement.AppendLine(setNoCount);

            sqlStatement.AppendLine($"DELETE FROM [{table.Name}]");
            sqlStatement.AppendLine(sqlRowIdentifier(table.PrimaryKey.Name));

            sqlStatement.AppendLine(end);

        }

        void GenerateForeignKeyDeleteStatements(SQLTable table, StringBuilder sqlStatement)
        {
            List<SQLForeignKeyRelation> foreignKeys = sQLForeignKeyRelationsForTable(table);

            foreach (SQLForeignKeyRelation foreignKey in foreignKeys)
            {
                SQLTableColumn column = foreignKey.ReferencedTableColumn;

                sqlStatement.AppendLine(go);
                sqlStatement.AppendLine("CREATE PROCEDURE [dbo].[" + table.Name + "DeleteFor" + column.TableName + "]" + Environment.NewLine);

                sqlStatement.AppendLine($"@{column.Name} {column.DataType}");
                sqlStatement.AppendLine(sqlAs);
                sqlStatement.AppendLine(begin);
                sqlStatement.AppendLine(setNoCount);

                sqlStatement.AppendLine($"DELETE FROM [{table.Name}]");
                sqlStatement.AppendLine(sqlRowIdentifier(column.Name));

                sqlStatement.AppendLine(end);
            }
        }

        string sqlRowIdentifier(string columnName)
        {
            return Environment.NewLine + "WHERE [" + columnName + "] = @" + columnName;
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }

        string ExecutePermissionStatement(string storedProcedure)
        {
            return $"GRANT EXECUTE ON OBJECT::dbo.{storedProcedure}\nTO {_spUserName};\n";
        }
    }

}
