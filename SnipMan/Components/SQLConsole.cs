using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SnipMan.Core;

namespace SnipMan.Components
{
    public partial class SqlConsole : UserControl
    {
        private BackgroundWorker _sqlWorker;
        private IDataProvider _sqlDriver;
        private DataTable _tempTable;
        private string _sqlCommand;

        public delegate void RefreshTreeViewDelegate();        
        public RefreshTreeViewDelegate RefreshTreeView;

        public SqlConsole()
        {
            InitializeComponent();
            InitConsole();
        }

        public void InitConsole()
        {
            sqlCon.Clear();
            _sqlDriver = DBase.Instance.Db;
            _sqlWorker = new BackgroundWorker();
            _sqlWorker.DoWork += new DoWorkEventHandler(sqlWorker_DoWork);
            _sqlWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(sqlWorker_RunWorkerCompleted);
            MessageBox.Show("Warning, this console is not supported and features severe memory leaks. Use is not recommended", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void sqlWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            sqlCon.AppendText(Environment.NewLine + PrintTable(_tempTable));
            sqlCon.SelectionStart = sqlCon.Text.Length;
            sqlCon.ScrollToCaret();
        }

        private static string PrintTable(DataTable dt)
        {
            DataTableReader dtReader = dt.CreateDataReader();
            StringBuilder result = new StringBuilder();
            while (dtReader.Read())
            {
                for (int i = 0; i < (dtReader.FieldCount > 20 ? 20 : dtReader.FieldCount); i++)
                {
                    result.AppendFormat("{0}: {1}",
                        dtReader.GetName(i).Trim(),
                        dtReader.GetValue(i).ToString().Trim());
                    result.AppendLine();
                }
                result.AppendLine();
            }
            dtReader.Close();
            return result.ToString();
        }

        private void sqlWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (_sqlDriver != null)
                {
                    _tempTable = _sqlDriver.PreparedQuery(_sqlCommand).Copy();
                }
            }
            catch (Exception ex)
            {
                new ErrorHandler(ex.Message.ToString(), ex.StackTrace.ToString());
            }
        }

        private void RunSql(bool noFocus)
        {
            _sqlCommand = sqlCon.SelectedText == "" ? sqlCon.Text : sqlCon.SelectedText;
            if(_sqlCommand.Trim().StartsWith("@")){
                // This is a console command
                switch(_sqlCommand.Trim()){
                    case "@refreshtree();":
                        RefreshTreeView();
                        break;
                }
            }else{
                if (_sqlCommand.Trim().EndsWith(";"))
                {
                    _sqlCommand = _sqlCommand.Trim().Substring(0, _sqlCommand.Length - 1);
                }
                _sqlWorker.RunWorkerAsync();
            }
            if (noFocus)
            {
                Focus();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            RunSql(false);
        }
    }
}
