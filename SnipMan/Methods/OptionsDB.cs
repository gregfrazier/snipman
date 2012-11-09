using System;
using System.Collections.Generic;
using SnipMan.Core;

namespace SnipMan.Methods
{
    class OptionsDb
    {
        private static DBase _db = DBase.Instance;

        private struct Queries
        {
            public static string SaveQueryStr = "UPDATE OPTIONS SET OPT_VAL_STR = @V where OPT_KEY = @K";
            public static string SaveQueryNum = "UPDATE OPTIONS SET OPT_VAL_NUM = @V where OPT_KEY = @K";

            public static string GetQueryStr = "SELECT OPT_VAL_STR FROM OPTIONS WHERE OPT_KEY = @K";
            public static string GetQueryNum = "SELECT OPT_VAL_NUM FROM OPTIONS WHERE OPT_KEY = @K";
        };

        public static bool SaveSyncType(string type)
        {
            return SaveSetting(new KeyValuePair<string, string>("sync_type", type));
        }

        public static string GetSettingStr(string h)
        {
            try
            {
                SqLiteTypes.SqLiteParam k;
                k.Param = "@K"; k.Value = h;
                return (string)_db.Db.PreparedScalar(Queries.GetQueryStr, k);
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return String.Empty;
            }
        }

        public static double? GetSettingNum(string h)
        {
            try
            {
                SqLiteTypes.SqLiteParam k;
                k.Param = "@K"; k.Value = h;
                return (double?)_db.Db.PreparedScalar(Queries.GetQueryNum, k);
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return null;
            }
        }

        public static bool SaveSetting(KeyValuePair<string, string> h)
        {
            try
            {
                SqLiteTypes.SqLiteParam v, k;
                v.Param = "@V"; v.Value = h.Value;
                k.Param = "@K"; k.Value = h.Key;
                _db.Db.PreparedNonQuery(Queries.SaveQueryStr, v, k);
                return true;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return false;
            }
        }

        public static bool SaveSetting(KeyValuePair<string, double> h)
        {
            try
            {
                SqLiteTypes.SqLiteParam v, k;
                v.Param = "@V"; v.Value = h.Key;
                k.Param = "@K"; k.Value = h.Value;
                _db.Db.PreparedNonQuery(Queries.SaveQueryNum, v, k);
                return true;
            }
            catch (System.Data.SQLite.SQLiteException x)
            {
                new ErrorHandler(x.Message, x.StackTrace);
                return false;
            }
        }
    }
}
