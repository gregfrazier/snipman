using System;
using System.Data;
using System.Windows.Forms;

namespace SnipMan.Misc.Forms
{
    public partial class MoveNode : Form
    {
        private class NodeItem
        {
            public string Tuid, Tname;

            public NodeItem(string uid, string name)
            {
                Tuid = uid;
                Tname = name;
            }

            public string Value()
            {
                return Tuid;
            }

            public override string ToString()
            {
                return Tname;
            }
        }

        private string _nodeKey;

        // Delegate to Refresh the TreeView
        public delegate void RefreshTreeViewDelegate();
        public RefreshTreeViewDelegate RefreshTreeView;

        public MoveNode(string currentNodeKey)
        {
            InitializeComponent();
            _nodeKey = currentNodeKey;
            Init();
        }

        public void Init()
        {
            nodeListBox.Items.Add(new NodeItem("-1", "[SET AS ROOT NODE]"));
            using (DataTable g = Methods.SnippetDb.ReturnAllNodes())
            {
                foreach (DataRow node in g.Rows)
                {
                    nodeListBox.Items.Add(new NodeItem(node["UID"].ToString(), node["NODE"].ToString()));
                }
            }
        }

        private void Move_Click(object sender, EventArgs e)
        {
            Int32 parent = Int32.Parse(((NodeItem)nodeListBox.SelectedItem).Value());
            Methods.SnippetDb.MoveNode(Int32.Parse(_nodeKey), parent);
            RefreshTreeView();
            Close();
        }
    }
}
