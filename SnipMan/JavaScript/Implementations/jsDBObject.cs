using System;
using System.Collections.Generic;
using SnipMan.JavaScript.Interfaces;
using System.Data;
using SnipMan.Core;

namespace SnipMan.JavaScript.Implementations
{
    public class JsDbObject : IDbObject
    {
        public string ExecuteScalar(string sql)
        {
            var output = DBase.Instance.Db.PreparedScalar(sql);
            return (output != DBNull.Value ? output.ToString() : "");
        }

        public IDataListObj ExecuteQuery(string sql)
        {
            using (DataTable dt = DBase.Instance.Db.PreparedQuery(sql))
            {
                List<object> h = dt.ToCollection<object>();
            }
            return null;
        }
    }
}
