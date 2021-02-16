using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace CodeGenerator
{
    public class SwiftDataBaseGenerator : Generator
    {
        public SwiftDataBaseGenerator(List<SQLTable> tables, string destinationFolder) : base(tables, destinationFolder)
        {
            filePrefix = "Database";
            fileSuffix = "swift";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            throw new NotImplementedException();
        }

        public new void GenerateClasses()
        {
            classText = new StringBuilder();

            classText.AppendLine($"import Foundation");
            classText.AppendLine($"import SQLite3");
            classText.AppendLine("");

            classText.AppendLine($"enum SQLiteError : Error {{");
  classText.AppendLine($"\tcase OpenDatabase(message: String)");
            classText.AppendLine($"\tcase Prepare(message: String)");
            classText.AppendLine($"\tcase Step(message: String)");
            classText.AppendLine($"\tcase Bind(message: String)");
            classText.AppendLine($"\t}}");
            classText.AppendLine("");

            classText.AppendLine($"class SQLiteDatabase {{");
            classText.AppendLine($"\tprivate let dbPointer: OpaquePointer?");
            classText.AppendLine($"\tprivate init(dbPointer: OpaquePointer?) {{");
            classText.AppendLine($"\t\tself.dbPointer = dbPointer");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"\tdeinit {{");
            classText.AppendLine($"\t\tsqlite3_close(dbPointer)");
            classText.AppendLine($"\t}}");

            classText.AppendLine("");

            classText.AppendLine($"\tstatic func open(path: String) throws->SQLiteDatabase {{");
            classText.AppendLine($"\t\tvar db: OpaquePointer?");
            classText.AppendLine($"\t\tif sqlite3_open(path, &db) == SQLITE_OK {{");
            classText.AppendLine($"\t\t\treturn SQLiteDatabase(dbPointer: db)");
            classText.AppendLine($"\t\t}} else {{");
            classText.AppendLine($"\t\t\tdefer {{");
            classText.AppendLine($"\t\t\t\tif db != nil {{");
            classText.AppendLine($"\t\t\t\t\tsqlite3_close(db)");
            classText.AppendLine($"\t\t\t\t}}");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine($"\t\t\tif let errorPointer = sqlite3_errmsg(db) {{");
            classText.AppendLine($"\t\t\t\tlet message = String(cString: errorPointer)");
            classText.AppendLine($"\t\t\t\tthrow SQLiteError.OpenDatabase(message: message)");
            classText.AppendLine($"\t\t\t}} else {{");

            classText.AppendLine($"\t\t\t\tthrow SQLiteError");
            classText.AppendLine($"\t\t\t\t\t.OpenDatabase(message: \"No error message provided from sqlite.\")");
            classText.AppendLine($"\t\t\t}}");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t}}");

            classText.AppendLine("");

            classText.AppendLine($"\tfileprivate var errorMessage: String {{");
            classText.AppendLine($"\t\tif let errorPointer = sqlite3_errmsg(dbPointer) {{");
            classText.AppendLine($"\t\t\tlet errorMessage = String(cString: errorPointer)");
            classText.AppendLine($"\t\t\treturn errorMessage");
            classText.AppendLine($"\t\t}} else {{");

            classText.AppendLine($"\t\t\treturn \"No error message provided from sqlite.\"");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"");

            classText.AppendLine($"\tfunc prepareStatement(sql: String) throws -> OpaquePointer? {{");
            classText.AppendLine($"\t\tvar statement: OpaquePointer?");

            classText.AppendLine($"\t\tguard sqlite3_prepare_v2(dbPointer, sql, -1, &statement, nil)");
            classText.AppendLine($"\t\t\t\t== SQLITE_OK else {{");
            classText.AppendLine($"\t\t\tthrow SQLiteError.Prepare(message: errorMessage)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\treturn statement");
            classText.AppendLine($"\t}}");
            classText.AppendLine("");

            classText.AppendLine($"\tfunc createTable(table: SQLTable.Type) throws {{");
            classText.AppendLine($"\t\tlet createTableStatement = try prepareStatement(sql: table.createStatement)");
            classText.AppendLine($"\t\tdefer {{");
            classText.AppendLine($"\t\t\tsqlite3_finalize(createTableStatement)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tguard sqlite3_step(createTableStatement) == SQLITE_DONE else {{");
            classText.AppendLine($"\t\t\tthrow SQLiteError.Step(message: errorMessage)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tprint(\"\\(table) table created.\")");
            classText.AppendLine($"\t}}");

            foreach(SQLTable table in _sQLTables)
            {
                classText.AppendLine("");
                GenerateInsertFunction(classText, table);
                classText.AppendLine("");
                GenerateSelectFunction(classText, table);
                classText.AppendLine("");
                GenerateSelectAllFunction(classText, table);
            }

            //end sql lite data base class.
            classText.AppendLine($"}}");

            classText.AppendLine("");

            classText.AppendLine($"protocol SQLTable {{");
            classText.AppendLine($"\tstatic var createStatement: String {{ get }}");
            classText.AppendLine($"}}");

            TextWriter writer = File.CreateText($"{_destinationFolder}{filePrefix}.{fileSuffix}");

            writer.Write(classText.ToString());

            writer.Close();
        }

        private void GenerateInsertFunction(StringBuilder classText, SQLTable table)
        {
            classText.AppendLine($"\tfunc insert{table.Name}({Library.LowerFirstCharacter(table.Name)}: {table.Name}) throws {{");
            classText.AppendLine($"\t\tlet insertSql = \"INSERT INTO {table.Name} ({string.Join(", ", table.Columns.Select(co => co.Name))}) VALUES ({string.Join(", ", table.Columns.Select(co => "?"))});\"");
            classText.AppendLine($"\t\tlet insertStatement = try prepareStatement(sql: insertSql)");
            classText.AppendLine($"\t\tdefer {{");
            classText.AppendLine($"\t\t\tsqlite3_finalize(insertStatement)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tguard");
            
            bool prependAmpresands = false;
            int columnCounter = 1;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (prependAmpresands)
                    classText.AppendLine(" &&");

                classText.Append($"\t\t\t{GenerateBindText("insertStatement", column, columnCounter, true)}");

                columnCounter += 1;
                prependAmpresands = true;
            }
            classText.AppendLine("");
            classText.AppendLine($"\t\t\telse {{");


            classText.AppendLine($"\t\t\t\tthrow SQLiteError.Bind(message: errorMessage)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tguard sqlite3_step(insertStatement) == SQLITE_DONE else {{");
            classText.AppendLine($"\t\t\tthrow SQLiteError.Step(message: errorMessage)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tprint(\"Successfully inserted row.\")");
            classText.AppendLine($"\t}}");
        }

        private void GenerateSelectFunction(StringBuilder classText, SQLTable table)
        {
            classText.AppendLine($"\tfunc {Library.LowerFirstCharacter(table.Name)}({Library.LowerFirstCharacter(table.PrimaryKey.Name)}: {table.PrimaryKey.iosDataType}) -> {table.Name}? {{");
            classText.AppendLine($"\t\tlet querySql = \"SELECT * FROM {table.Name} WHERE {table.PrimaryKey.Name} = ?;\"");
            classText.AppendLine($"\t\tguard let queryStatement = try? prepareStatement(sql: querySql) else {{");
            classText.AppendLine($"\t\t\treturn nil");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tdefer {{");
            classText.AppendLine($"\t\t\tsqlite3_finalize(queryStatement)");
            classText.AppendLine($"\t\t}}");

            classText.AppendLine($"\t\tguard {GenerateBindText("queryStatement", table.PrimaryKey, 1, false)} else {{");

            classText.AppendLine($"\t\t\treturn nil");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tguard sqlite3_step(queryStatement) == SQLITE_ROW else {{");
            classText.AppendLine($"\t\t\treturn nil");
            classText.AppendLine($"\t\t}}");

            int columnCounter = 0;
            foreach (SQLTableColumn column in table.Columns)
            {
                classText.AppendLine($"\t\t{GenerateReadQueryValueText(column, columnCounter)}");
                columnCounter++;
            }
            classText.AppendLine("");
            classText.AppendLine($"\t\treturn {table.Name}({string.Join(", ", table.Columns.Select(co => Library.LowerFirstCharacter(co.Name) + ": " + Library.LowerFirstCharacter(co.Name)))})");
            classText.AppendLine($"\t}}");
        }

        private void GenerateSelectAllFunction(StringBuilder classText, SQLTable table)
        {
            classText.AppendLine($"\tfunc all{table.Name}s() -> [{table.Name}]? {{");
            classText.AppendLine($"\t\tvar rows = [{table.Name}]()");
            classText.AppendLine($"\t\tlet querySql = \"SELECT * FROM {table.Name};\"");
            classText.AppendLine($"\t\tguard let queryStatement = try? prepareStatement(sql: querySql) else {{");
            classText.AppendLine($"\t\t\treturn nil");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tdefer {{");
            classText.AppendLine($"\t\t\tsqlite3_finalize(queryStatement)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine("");
            classText.AppendLine($"\t\tvar result = sqlite3_step(queryStatement)");
            classText.AppendLine("");
            classText.AppendLine($"\t\twhile result == SQLITE_ROW {{");

            int columnCounter = 0;
            foreach (SQLTableColumn column in table.Columns)
            {
                classText.AppendLine($"\t\t\t{GenerateReadQueryValueText(column, columnCounter)}");
                columnCounter++;
            }

            classText.AppendLine("");
            classText.AppendLine($"\t\t\tlet {Library.LowerFirstCharacter(table.Name)} = {table.Name}({string.Join(", ", table.Columns.Select(co => Library.LowerFirstCharacter(co.Name) + ": " + Library.LowerFirstCharacter(co.Name)))})");
            classText.AppendLine("");
            classText.AppendLine($"\t\t\trows.append({Library.LowerFirstCharacter(table.Name)})");
            classText.AppendLine($"\t\t\tresult = sqlite3_step(queryStatement)");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine("\t\treturn rows");
            classText.AppendLine("\t}");
        }
        private string GenerateBindText(string statementParameter, SQLTableColumn column, int columnCounter, bool includeClassNameInParameter)
        {
            string parameterToBind = $"{(includeClassNameInParameter ? Library.LowerFirstCharacter(column.TableName) + "." : "")}{Library.LowerFirstCharacter(column.Name)}";
 
            return column.sqlLiteDataType switch
            {
                sqlLiteStorageDataTypes.intStore => $"sqlite3_bind_int({statementParameter}, {columnCounter}, {parameterToBind}) == SQLITE_OK",
                sqlLiteStorageDataTypes.floatStore =>(column.DataType == SQLDataTypes.dateTime ? $"sqlite3_bind_double({statementParameter}, {columnCounter}, {parameterToBind}.timeIntervalSince1970) == SQLITE_OK": $"sqlite3_bind_double({statementParameter}, {columnCounter}, {parameterToBind}) == SQLITE_OK"),
                sqlLiteStorageDataTypes.textStore => $"sqlite3_bind_{column.sqlLiteBindType}({statementParameter}, {columnCounter}, NSString(string: {parameterToBind}).utf8String, -1, nil) == SQLITE_OK",
                sqlLiteStorageDataTypes.blobStore => throw new NotImplementedException("Blob storage bind not implemented"),
                _ => throw new SQLDataTypeNotHandledInSwiftDatabaseBindText(column.DataType)

            };
        }

        private string GenerateReadQueryValueText(SQLTableColumn column, int columnCounter)
        {
            return column.sqlLiteDataType switch
            {
                sqlLiteStorageDataTypes.intStore => $"let {Library.LowerFirstCharacter(column.Name)} = sqlite3_column_{column.sqlLiteBindType}(queryStatement, {columnCounter})",
                sqlLiteStorageDataTypes.floatStore => (column.DataType == SQLDataTypes.dateTime ? $"let {Library.LowerFirstCharacter(column.Name)} = NSDate(timeIntervalSince1970:  sqlite3_column_double(queryStatement, {columnCounter})) as Date" : $"let {Library.LowerFirstCharacter(column.Name)} = sqlite3_column_double(queryStatement, {columnCounter})"),
                sqlLiteStorageDataTypes.textStore => $"let {Library.LowerFirstCharacter(column.Name)} = String(cString: sqlite3_column_{column.sqlLiteBindType}(queryStatement, {columnCounter})) as String",
                _ => throw new SQLDataTypeNotHandledInSwiftDatabaseBindText(column.DataType)
            };
        }
    }

    public class SQLDataTypeNotHandledInSwiftDatabaseBindText : ArgumentOutOfRangeException
    {
        string _dataType;
        public SQLDataTypeNotHandledInSwiftDatabaseBindText(string dataType)
        {
            _dataType = dataType;
        }

        public override string Message => $"SQL data type {_dataType} needs to be handled.";
    }
}
