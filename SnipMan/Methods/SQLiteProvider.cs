using System;
using System.Data.SQLite;
using SnipMan.Core;

namespace SnipMan.Methods
{
    class SqLiteProvider : IDataProvider
    {
        private string _sqlConnectionString;

        #region DataProvider Members

        public void SetConnection(string connectionString)
        {
            _sqlConnectionString = connectionString;
        }

        public SqLiteProvider(string connectionString)
        {
            SetConnection(connectionString);
        }

        public object CreateParameter(object obj, object conn)
        {
            SQLiteParameter s = new SQLiteParameter
            {
                ParameterName = ((SqLiteTypes.SqLiteParam) obj).Param,
                Value = ((SqLiteTypes.SqLiteParam) obj).Value,
                Direction = System.Data.ParameterDirection.Input
            };
            return s;
        }

        public object CreateParameterBind(string bindname, object obj, object conn)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable PreparedQuery(string sql, params object[] parameters)
        {
            try{
                using(SQLiteConnection conn = new SQLiteConnection(_sqlConnectionString)){
                    using(SQLiteCommand cmd = new SQLiteCommand(sql, conn)){
                        using(System.Data.DataTable dt = new System.Data.DataTable()){
                            conn.Open();
                            foreach (object o in parameters){
                                cmd.Parameters.Add((SQLiteParameter)CreateParameter(o, conn));
                            }
                            SQLiteDataReader reader = cmd.ExecuteReader();
                            dt.Load(reader);
                            reader.Close(); reader.Dispose();
                            conn.Close();
                            return dt;
                        }
                    }
                }
            }catch (SQLiteException ex){                
                new ErrorHandler(ex.Message.ToString(), ex.StackTrace.ToString());
                return null;
            }
        }

        public void PreparedNonQuery(string sql, params object[] parameters)
        {
            try{
                using(SQLiteConnection conn = new SQLiteConnection(_sqlConnectionString)){
                    using(SQLiteCommand cmd = new SQLiteCommand(sql, conn)){
                        foreach (object o in parameters){
                            cmd.Parameters.Add((SQLiteParameter)CreateParameter(o, conn));
                        }
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return;
                    }
                }
            }catch (SQLiteException ex){
                new ErrorHandler(ex.Message.ToString(), ex.StackTrace.ToString());
                return;
            }
        }

        public System.Data.DataTable PreparedQueryBind(string sql, System.Collections.Hashtable parameters)
        {
            throw new NotImplementedException();
        }

        public object PreparedStatement(string sql, string retVal, object retType, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public object PreparedScalar(string sql, params object[] parameters)
        {
            try{
                using(SQLiteConnection conn = new SQLiteConnection(_sqlConnectionString)){
                    using(SQLiteCommand cmd = new SQLiteCommand(sql, conn)){
                        conn.Open();
                        foreach (object o in parameters){
                            cmd.Parameters.Add((SQLiteParameter)CreateParameter(o, conn));
                        }
                        object val = cmd.ExecuteScalar();
                        conn.Close();
                        return val;
                    }
                }
            }catch (SQLiteException ex){                
                new ErrorHandler(ex.Message.ToString(), ex.StackTrace.ToString());
                return null;
            }
        }

        public object StoredFunction(string funcName, object retType, params object[] oracleParameters)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable StoredProcedure(string procedureName, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int UpdateQuery(string sql, params object[] parameters)
        {
            try{
                using(SQLiteConnection conn = new SQLiteConnection(_sqlConnectionString)){
                    using(SQLiteCommand cmd = new SQLiteCommand(sql, conn)){
                        foreach (object o in parameters){
                            cmd.Parameters.Add((SQLiteParameter)CreateParameter(o, conn));
                        }
                        conn.Open();
                        var rowsAffected = cmd.ExecuteNonQuery();
                        conn.Close();
                        return rowsAffected;
                    }
                }
            }catch (SQLiteException ex){                
                new ErrorHandler(ex.Message.ToString(), ex.StackTrace.ToString());
                return 0;
            }
        }

        public string GetConnString(string path, string filename)
        {
            return "Data Source=" + path + "\\" + filename;
            //throw new NotImplementedException();
        }

        #endregion

        public bool ShrinkDatabase()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(_sqlConnectionString))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("VACUUM", conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                new ErrorHandler(ex.Message.ToString(), ex.StackTrace.ToString());
                return false;
            }
        }
    }
}
