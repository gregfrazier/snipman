using System;
using SnipMan.Core;
using System.Data;

namespace SnipMan.Methods
{
    static class SnippetDb
    {
        private static DBase _db = DBase.Instance;
        //private static int NodeList = 0;

        private struct Queries
        {
            public static string DeleteQuerySnippet = "DELETE FROM SNIPPETS WHERE UID = @U";
            public static string DeleteQueryTreeView = "DELETE FROM TREEVIEW WHERE UID = @U";
            public static string VersionCheckQuery = "SELECT IFNULL(ID, 1) FROM VERSION";
            public static string VersionTableCheckQuery = "select count(*) from sqlite_master where name = 'VERSION'";

            public static string SaveQuery = "UPDATE SNIPPETS SET DATA = @D where UID = @U";
            public static string RenameQuery = "UPDATE TREEVIEW SET NODE = @N WHERE UID= @U";
            public static string InsertQueryTreeView = "INSERT INTO TREEVIEW (UID, PARENT, NODE) VALUES (@U, @P, @N)";
            public static string InsertQuerySnippet = "INSERT INTO SNIPPETS (UID, DATA) VALUES (@U, '')";

            public static string LoadNodeQuery = "select data from SNIPPETS where UID = @U";
            public static string PopulateTreeQuery = "SELECT UID, PARENT, NODE FROM TREEVIEW WHERE PARENT = @P";

            public static string SearchAllQuery = "SELECT UID FROM SNIPPETS WHERE DATA like @D";
            public static string LastNodeIdQuery = "SELECT IFNULL(MAX(UID), 0) FROM SNIPPETS";

            public static string GetAllNodesQuery = "SELECT UID, NODE FROM TREEVIEW ORDER BY NODE";
            public static string MoveNodeUnderQuery = "UPDATE TREEVIEW SET PARENT = @P WHERE UID = @U";
        };

        public static Int32 SaveNodeV1(Int32 nodeKey, string data)
        {
            try
            {
                SqLiteTypes.SqLiteParam u, d;
                u.Param = "@U"; u.Value = nodeKey;
                d.Param = "@D"; d.Value = data;
                _db.Db.PreparedNonQuery(Queries.SaveQuery, d, u);
                return nodeKey;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return -255;
            }
        }

        public static Int32 MoveNode(Int32 nodeKey, Int32 parent)
        {
            try
            {
                SqLiteTypes.SqLiteParam u, p;
                u.Param = "@U"; u.Value = nodeKey;
                p.Param = "@P"; p.Value = parent;
                _db.Db.PreparedNonQuery(Queries.MoveNodeUnderQuery, p, u);
                return nodeKey;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return -255;
            }
        }
        
        public static Int32 DeleteNode(Int32 nodeKey)
        {
            try
            {
                SqLiteTypes.SqLiteParam u;
                u.Param = "@U";
                u.Value = nodeKey;
                _db.Db.PreparedNonQuery(Queries.DeleteQuerySnippet, u);
                _db.Db.PreparedNonQuery(Queries.DeleteQueryTreeView, u);
                return nodeKey;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return -255;
            }
        }

        public static Int32 RenameNode(Int32 nodeKey, string newName)
        {
            try
            {
                SqLiteTypes.SqLiteParam u, n;
                u.Param = "@U";
                u.Value = nodeKey;
                n.Param = "@N";
                n.Value = newName;
                _db.Db.PreparedNonQuery(Queries.RenameQuery, n, u);
                return nodeKey;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return -255;
            }
        }

        public static string NodeValueV1(Int32 nodeKey)
        {
            try
            {
                if (nodeKey == -1) { return String.Empty; }
                SqLiteTypes.SqLiteParam u;
                u.Param = "@U";
                u.Value = nodeKey;
                return (_db.Db.PreparedScalar(Queries.LoadNodeQuery, u) ?? String.Empty).ToString();
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return "Error retrieving node data";
            }
        }

        public static Int32 InsertNodeV1(Int32 nodeKey, Int32 parentKey, string data)
        {
            try
            {
                SqLiteTypes.SqLiteParam u, t, r;
                u.Param = "@U"; u.Value = nodeKey;
                t.Param = "@P"; t.Value = parentKey;
                r.Param = "@N"; r.Value = data;
                _db.Db.PreparedNonQuery(Queries.InsertQueryTreeView, u, t, r);
                _db.Db.PreparedNonQuery(Queries.InsertQuerySnippet, u);
                return nodeKey;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return -255;
            }
        }

        public static Int32 CheckVersion()
        {
            try
            {
                string versionExist = _db.Db.PreparedScalar(Queries.VersionTableCheckQuery).ToString();
                if (versionExist == "0") { return 1; }
                string versionNumber = _db.Db.PreparedScalar(Queries.VersionCheckQuery).ToString();
                return Int32.Parse(versionNumber);
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return 0;
            }
        }

        /// <summary>
        /// Creates the Tree of Snippets
        /// </summary>
        /// <param name="n"></param>
        public static DataTable PopulateTreeQuery(int n)
        {
            SqLiteTypes.SqLiteParam u;
            u.Param = "@P";
            u.Value = n.ToString();
            string sql = Queries.PopulateTreeQuery; //"SELECT UID, PARENT, NODE FROM TREEVIEW WHERE PARENT = @P";
            using (DataTable dt = _db.Db.PreparedQuery(sql, u))
            {
                if (dt == null)
                {
                    new ErrorHandler("Unable to Query", "PopulateTreeView");
                    return null;
                }
                return dt;
            }
        }

        public static DataTable ReturnAllNodes()
        {
            string sql = Queries.GetAllNodesQuery;
            using (DataTable dt = _db.Db.PreparedQuery(sql))
            {
                if (dt == null)
                {
                    new ErrorHandler("Unable to Query", "ReturnAllNodes");
                    return null;
                }
                return dt;
            }
        }

        public static Int32 LastNodeId()
        {
            try
            {
                string lastNodeId = _db.Db.PreparedScalar(Queries.LastNodeIdQuery).ToString();
                return Int32.Parse(lastNodeId);
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return 0;
            }
        }

        
    }
}
