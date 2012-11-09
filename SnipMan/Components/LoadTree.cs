using System;
using System.Windows.Forms;

namespace SnipMan.Components
{
    public partial class LoadTree : Form
    {
        public LoadTree()
        {
            InitializeComponent();
        }

        public void OnSendTotal(object sender, Methods.TotalCountEventArgs data)
        {
            UpdateTotalCount(data.SnippetTotal);
        }

        public void OnNextSnippet(object sender, Methods.NextSnippetEventArgs data)
        {
            IncrementCount(data.SnippetNames);
        }

        public void OnComplete(object sender, EventArgs data)
        {
            CompleteProcessing();
        }

        delegate void IncrementCountDelegate(string newText);
        private void IncrementCount(string newText)
        {
            if (progressBar1.InvokeRequired)
            {
                IncrementCountDelegate del = new IncrementCountDelegate(IncrementCount);
                progressBar1.Invoke(del, new object[] { newText });
            }
            else
            {
                return;
                //progressBar1.Increment(1);
            }
        }

        delegate void CompleteProcessingDelegate();
        private void CompleteProcessing()
        {
            if (InvokeRequired)
            {
                CompleteProcessingDelegate del = new CompleteProcessingDelegate(CompleteProcessing);
                Invoke(del);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        delegate void TotalCountUpdateDelegate(int total);
        private void UpdateTotalCount(int total)
        {
            if (progressBar1.InvokeRequired)
            {
                TotalCountUpdateDelegate del = new TotalCountUpdateDelegate(UpdateTotalCount);
                progressBar1.Invoke(del, new object[] { total });
            }
            else
            {
                return;
                //progressBar1.Value = 0;
                //progressBar1.Maximum = total;
            }
        }
    }
}
