using System.Windows.Forms;

namespace SnipMan.Misc.Forms
{
    public partial class SqlConsoleWin : Form
    {
        public SqlConsoleWin(Components.TreeViewEx tv1)
        {
            InitializeComponent();
            sqlConsole1.RefreshTreeView = new Components.SqlConsole.RefreshTreeViewDelegate(tv1.RefreshTreeView);
        }
    }
}
