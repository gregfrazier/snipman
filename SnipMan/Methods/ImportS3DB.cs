using System;
using SnipMan.Core;
using System.Threading;

namespace SnipMan.Methods
{
    public class NextSnippetEventArgs : EventArgs
    {
        public string SnippetNames { get; internal set; }
        public NextSnippetEventArgs(string snipname)
        {
            SnippetNames = snipname;
        }
    }

    public class TotalCountEventArgs : EventArgs
    {
        public int SnippetTotal { get; internal set; }
        public TotalCountEventArgs(int total)
        {
            SnippetTotal = total;
        }
    }

    class ImportS3Db
    {
        private static DBase _destination = DBase.Instance;
        private IDataProvider _source;
        private string _sqLiteImportFile;
        private int _destinationNode;
        private int _maxKey;

        public delegate void NextSnippetProcessed(object sender, NextSnippetEventArgs data);
        public event NextSnippetProcessed NextSnippet;

        public delegate void TotalProcessed(object sender, TotalCountEventArgs data);
        public event TotalProcessed TotalSnippets;

        public delegate void CompleteProcessed(object sender, EventArgs data);
        public event CompleteProcessed ProcessComplete;

        public ImportS3Db(string filename, int destNode)
        {
            _sqLiteImportFile = filename;
            _source = new SqLiteProvider("Data Source=" + filename);
            _destinationNode = destNode;
        }

        private void OnSendTotal(object sender, TotalCountEventArgs data)
        {
            TotalSnippets?.Invoke(this, data);
        }

        private void OnNextSnippet(object sender, NextSnippetEventArgs data)
        {
            NextSnippet?.Invoke(this, data);
        }

        private void OnComplete(object sender, EventArgs data)
        {
            ProcessComplete?.Invoke(this, data);
        }

        private void PreProcess()
        {
            try
            {
                string sqlGetMax = "SELECT MAX(UID) FROM TREEVIEW";
                _maxKey = Int32.Parse(_destination.Db.PreparedScalar(sqlGetMax).ToString()) + 1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Process()
        {
            try
            {
                Thread procThread = new Thread(new ThreadStart(ProcessThread));
                procThread.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ProcessThread()
        {
            try
            {
                PreProcess();
                string sqlTree = "SELECT UID, PARENT, NODE FROM TREEVIEW";
                string sqlSnip = "SELECT UID, DATA FROM SNIPPETS";
                System.Data.DataTable dt = _source.PreparedQuery(sqlTree);
                System.Data.DataTable dt2 = _source.PreparedQuery(sqlSnip);
                int totalCount = dt.Rows.Count + dt2.Rows.Count;
                OnSendTotal(this, new TotalCountEventArgs(totalCount));
                foreach (System.Data.DataRow r in dt.Rows)
                {
                    int nodeKey = Int32.Parse(r["UID"].ToString()) + _maxKey;
                    string nodeNode = r["NODE"].ToString();
                    int nodeParent = 0;
                    OnNextSnippet(this, new NextSnippetEventArgs(nodeNode));

                    if (r["PARENT"].ToString() != "-1")
                    {
                        nodeParent = Int32.Parse(r["PARENT"].ToString()) + _maxKey;
                    }
                    else
                    {
                        nodeParent = _destinationNode;
                    }

                    if (nodeKey > _maxKey && nodeParent >= _destinationNode)
                    {
                        SqLiteTypes.SqLiteParam i, p, n;
                        i.Param = "@U";
                        i.Value = nodeKey.ToString();
                        p.Param = "@P";
                        p.Value = nodeParent.ToString();
                        n.Param = "@N";
                        n.Value = nodeNode;
                        _destination.Db.PreparedNonQuery("INSERT INTO TREEVIEW (UID, PARENT, NODE) VALUES (@U, @P, @N)", i, p, n);
                    }
                }
                dt.Dispose();
                foreach (System.Data.DataRow r in dt2.Rows)
                {
                    int nodeKey = Int32.Parse(r["UID"].ToString()) + _maxKey;
                    string nodeNode = r["DATA"].ToString();
                    OnNextSnippet(this, new NextSnippetEventArgs(nodeKey.ToString()));

                    if (nodeKey > _maxKey)
                    {
                        SqLiteTypes.SqLiteParam i, n;
                        i.Param = "@U";
                        i.Value = nodeKey.ToString();
                        n.Param = "@N";
                        n.Value = nodeNode;
                        _destination.Db.PreparedNonQuery("INSERT INTO SNIPPETS (UID, DATA) VALUES (@U, @N)", i, n);
                    }
                }
                dt2.Dispose();
                OnComplete(this, new EventArgs());
                return; // true;
            }
            catch (Exception)
            {
                OnComplete(this, new EventArgs());
                return; // false;
            }
        }
    }
}
