using DataServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CodeGenerator
{
    public class SQLForeignKeyRelation
    {
        public SQLTableColumn ParentTableColum
        {
            get { return IdentifyColumn(parentObjectID, parentColumnID); }
        }
        public SQLTableColumn ReferencedTableColumn { get { return IdentifyColumn(referencedObjectID, referencedColumnID); } }
        private int parentObjectID;
        private int parentColumnID;
        private int referencedObjectID;
        private int referencedColumnID;
        public List<SQLTable> DatabaseTables;

        private SQLTableColumn IdentifyColumn(int tableID, int columnID)
        {
            foreach (SQLTable table in DatabaseTables)
            {
                if (table.id == tableID)
                {
                    foreach (SQLTableColumn column in table.Columns)
                        if (column.OrdinalPosition == columnID)
                            return column;
                }
            }
            return null;
        }

        public static List<SQLForeignKeyRelation> LoadForeignKeysForColumn(int tableID, int columnID, string connectionString, ref List<SQLTable> databaseTables)
        {

            List<SQLForeignKeyRelation> foreignKeys = new List<SQLForeignKeyRelation>();

            string selectStatement = "Select constraint_object_id, constraint_column_id, parent_object_id, parent_column_id, referenced_object_id, referenced_column_id from sys.foreign_key_columns where referenced_object_id = " + tableID + " and referenced_column_id = " + columnID;

            SqlDataReader dataReader = SQLDataServer.ExecuteSQLStringReturnDataReader(selectStatement, connectionString);

            while (dataReader.Read())
            {
                SQLForeignKeyRelation foreignKey = new SQLForeignKeyRelation();

                foreignKey.DatabaseTables = databaseTables;

                foreignKey.parentObjectID = Convert.ToInt32(dataReader["parent_object_id"]);
                foreignKey.parentColumnID = Convert.ToInt32(dataReader["parent_column_id"]);
                foreignKey.referencedObjectID = Convert.ToInt32(dataReader["referenced_object_id"]);
                foreignKey.referencedColumnID = Convert.ToInt32(dataReader["referenced_column_id"]);

                foreignKeys.Add(foreignKey);
            }

            dataReader.Close();

            return foreignKeys;
        }
    }
}
