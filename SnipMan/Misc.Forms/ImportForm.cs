using System;
using System.Windows.Forms;

namespace SnipMan.Misc.Forms
{
    public partial class ImportForm : Form
    {
        private string _fileName;

        public ImportForm()
        {
            InitializeComponent();
        }

        public ImportForm(string filename)
        {
            InitializeComponent();
            _fileName = filename;
        }

        delegate void UpdateLabelTextDelegate(string newText);
        private void UpdateLabelText(string newText)
        {
            if (label1.InvokeRequired)
            {
                UpdateLabelTextDelegate del = new UpdateLabelTextDelegate(UpdateLabelText);
                label1.Invoke(del, new object[] { newText });
            }
            else
            {
                label1.Text = newText;
                progressBar1.Increment(1);
            }
        }

        delegate void CompleteProcessingDelegate();
        private void CompleteProcessing()
        {
            if (label1.InvokeRequired)
            {
                CompleteProcessingDelegate del = new CompleteProcessingDelegate(CompleteProcessing);
                label1.Invoke(del);
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
                progressBar1.Maximum = total;
            }
        }

        void j_NextSnippet(object sender, Methods.NextSnippetEventArgs data)
        {
            UpdateLabelText(data.SnippetNames);
        }

        private void ImportForm_Load(object sender, EventArgs e)
        {
            int id = Methods.SnippetDb.LastNodeId();
            Methods.SnippetDb.InsertNodeV1(id + 1, -1, "Imported_" + DateTime.Now.ToShortDateString());
            Methods.ImportS3Db j = new Methods.ImportS3Db(_fileName, id + 1);
            j.NextSnippet += new Methods.ImportS3Db.NextSnippetProcessed(j_NextSnippet);
            j.ProcessComplete += new Methods.ImportS3Db.CompleteProcessed(j_ProcessComplete);
            j.TotalSnippets += new Methods.ImportS3Db.TotalProcessed(j_TotalSnippets);
            j.Process();
        }

        void j_TotalSnippets(object sender, Methods.TotalCountEventArgs data)
        {
            UpdateTotalCount(data.SnippetTotal);
        }

        void j_ProcessComplete(object sender, EventArgs data)
        {
            CompleteProcessing();
        }
    }
}
