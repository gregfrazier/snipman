using System.Collections;

namespace SnipMan.Components
{
    public class TreeNode : System.Windows.Forms.TreeNode, IDictionaryEnumerator
    {
        private DictionaryEntry _nodeEntry;
        private IEnumerator _enumerator;

        public TreeNode(){
            _enumerator = Nodes.GetEnumerator();
        }

        public TreeNode(string name){
            if (name == null) { name = string.Empty; }
            _enumerator = Nodes.GetEnumerator();
            Name = name;
        }

        public string NodeKey{
            get{return _nodeEntry.Key.ToString();}
            set{_nodeEntry.Key = value;}
        }

        public object NodeValue{
            get{return _nodeEntry.Value;}
            set{_nodeEntry.Value = value;}
        }


        #region IDictionaryEnumerator Members

        public DictionaryEntry Entry
        {
            get { return _nodeEntry; }
        }

        public object Key
        {
            get { return _nodeEntry.Key; }
        }

        public object Value
        {
            get { return _nodeEntry.Value; }
        }

        #endregion

        #region IEnumerator Members

        public object Current
        {
            get { return _enumerator.Current; }
        }

        public bool MoveNext()
        {
            var success = _enumerator.MoveNext();
            return success;
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        #endregion
    }
}
