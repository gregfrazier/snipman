using System;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using SnipMan.Methods;

namespace SnipMan.Components
{
    public partial class TreeViewEx : TreeView
    {
        private System.Collections.Hashtable _nodelist;

        // Threading
        delegate void AddNodeRootCallback(TreeNode n);
        delegate void AddNodeCallback(TreeNode n, string key);

        // Events
        public delegate void NextSnippetProcessed(object sender, NextSnippetEventArgs data);
        public event NextSnippetProcessed NextSnippet;

        public delegate void TotalProcessed(object sender, TotalCountEventArgs data);
        public event TotalProcessed TotalSnippets;

        public delegate void CompleteProcessed(object sender, EventArgs data);
        public event CompleteProcessed ProcessComplete;

        public Int32 LastNode {
            get { return SnippetDb.LastNodeId(); }
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

        public TreeViewEx()
        {
            InitializeComponent();
            //this.LabelEdit
            _nodelist = new System.Collections.Hashtable();
        }

        protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
        {
            base.OnAfterLabelEdit(e);
            bool invalidText = false;

            if (e.Label != null)
            {
                if (e.Label.Length > 0)
                {
                    var h = e.Label;

                    if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) > -1)
                    {
                        invalidText = true;
                        Regex r = new Regex(@"[@\.\,!]");
                        h = r.Replace(e.Label, "_");
                        r = null;
                    }

                    if (e.Node.Text == String.Empty)
                    {
                        // Brand New Node, Doesn't Exist Yet.
                        e.Node.Text = h;
                        e.Node.EndEdit(false);
                        // Store in Database
                        SnippetDb.InsertNodeV1(
                            (e.Node != null ? Int32.Parse(((TreeNode)e.Node).NodeKey.ToString()) : 0),
                            (e.Node.Parent != null ? Int32.Parse(((TreeNode)e.Node.Parent).NodeKey.ToString()) : -1),
                            e.Node.Text
                        );
                    }
                    else
                    {
                        e.Node.Text = h;
                        e.Node.EndEdit(false);
                        // Update Database
                        SnippetDb.RenameNode(Int32.Parse(((TreeNode)e.Node).NodeKey), e.Node.Text);
                    }

                    if(invalidText)
                    {
                        MessageBox.Show("Invalid tree node label.\n" +
                          "The invalid characters are: '@','.', ',', '!', replaced with '_'",
                          "Node Label Edit");
                        return;
                    }
                }
                LabelEdit = false;
            }
        }

        public void AddNodeRoot(TreeNode n)
        {
            if (InvokeRequired)
            {
                AddNodeRootCallback d = new AddNodeRootCallback(AddNodeRoot);
                Invoke(d, new object[] { n });
            }
            else
            {
                CheckDisposed();
                Nodes.Add(n);
                _nodelist.Add(n.Key?.ToString(), n);
            }
        }

        public void AddNode(TreeNode n, string key)
        {
            if (InvokeRequired)
            {
                AddNodeCallback d = new AddNodeCallback(AddNode);
                Invoke(d, new object[] { n, key });
            }
            else
            {
                CheckDisposed();
                Node(key).Nodes.Add(n);
                _nodelist.Add(n.Key?.ToString(), n);
            }
        }

        public bool ContainsNode(string key)
        {
            CheckDisposed();
            return _nodelist != null && _nodelist.ContainsKey(key);
        }

        public TreeNode Node(string key)
        {
            CheckDisposed();
            if (_nodelist != null && _nodelist.ContainsKey(key))
            {
                return ((TreeNode)(_nodelist[key]));
            }
            return null;
        }

        public void DeleteNode(TreeNode n)
        {
            CheckDisposed();
            if(n == null){ throw new ArgumentNullException(); }
            _nodelist.Remove(n.Key);
            n.Remove();
        }

        public void RefreshTreeView(){
            Nodes.Clear();
            _nodelist.Clear();
            //this.PopulateTreeView(-1);
            PopulateTreeViewThreaded();
        }

        public void PopulateTreeViewThreaded()
        {
            try
            {
                LoadTree newLoading = new LoadTree();
                NextSnippet += new NextSnippetProcessed(newLoading.OnNextSnippet);
                ProcessComplete += new CompleteProcessed(newLoading.OnComplete);
                TotalSnippets += new TotalProcessed(newLoading.OnSendTotal);
                Thread popThread = new Thread(new ThreadStart(PopulateTreeViewMainThreaded));
                popThread.Start();
                newLoading.ShowDialog();
                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        private void PopulateTreeViewMainThreaded()
        {
            PopulateTreeView(-1);
        }

        public void PopulateTreeView(int parentNodeVal)
        {
            if (parentNodeVal == -1 || ContainsNode(parentNodeVal.ToString()))
            {
                using (DataTable y = SnippetDb.PopulateTreeQuery(parentNodeVal))
                {
                    OnSendTotal(this, new TotalCountEventArgs(y.Rows.Count));
                    PopulateTreeView(parentNodeVal, y);
                    OnComplete(this, new EventArgs());
                }
            }
        }

        private void PopulateTreeView(int parentNodeVal, DataTable dt)
        {   
            foreach (DataRow r in dt.Rows)
            {
                TreeNode t1 = new TreeNode();
                TreeNode pIndex = Node(r["PARENT"].ToString());
                t1.NodeKey = r["UID"].ToString();
                t1.Text = r["NODE"].ToString();
                OnNextSnippet(this, new NextSnippetEventArgs(t1.NodeKey));
                if (parentNodeVal != -1)
                {
                    // child node
                    if (pIndex != null)
                    {
                        AddNode(t1, pIndex.Key?.ToString());
                    }
                }
                else
                {
                    // It's a root node
                    AddNodeRoot(t1);
                }
                if (ContainsNode(r["UID"].ToString()))
                {
                    PopulateTreeView(Int32.Parse(r["UID"].ToString()), SnippetDb.PopulateTreeQuery(Int32.Parse(r["UID"].ToString())));
                }
            }
        }

        private void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("TreeViewEx");
            }
        }
    }
}
