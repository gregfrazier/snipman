using System;
using System.Linq;
using System.Data;

namespace SnipMan.JavaScript.Implementations
{
    public class JsDataListObj : Interfaces.IDataListObj, IDisposable
    {
        DataTable _dt;

        public JsDataListObj(DataTable t)
        {
            _dt = t.Copy();
        }

        public string[] Row(int yIndex)
        {
            var item = from r in _dt.Rows[yIndex].ItemArray
                       select r.ToString();
            return item.ToArray<string>();
        }

        public string ColumnName(int xIndex)
        {
            return _dt.Columns[xIndex].ColumnName;
        }

        public int Count()
        {
            return _dt.Rows.Count;
        }

        public void Dispose()
        {
            if (_dt != null)
            {
                _dt.Dispose();
                _dt = null;
            }
        }
    }
}
