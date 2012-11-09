using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SnipMan.Core;
using System.Runtime.InteropServices;
using System.IO;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor;
using SnipMan.Components;
using SnipMan.JavaScript.Interfaces;

namespace SnipMan
{
    public partial class Form1 : Form
    {
        private static DBase _db = DBase.Instance;
        //private int NodeList = 0;
        private string _lastCriteria = String.Empty;
        private Queue<String> _findList;
        private int _findLocation = 0;
        private string _oldNode = String.Empty;
        private IMySettings _snipManSettings;
        private Stack<ToolStripMenuItem> _selectedHighlighting;

        // For Debugging Only
        private bool _currentNodeTextChanged = false;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// initialize/construct
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            _selectedHighlighting = new Stack<ToolStripMenuItem>();
            LoadSettings();
            SetEventHandlers();
            // This populates all Nodes and their Children with a Parent of -1 (Root Nodes)
            //this.treeView1.PopulateTreeView(-1);
            treeView1.RefreshTreeView();
        }

        private void LoadSettings()
        {
            // Registry Settings - Not Used Anymore
            //SnipManSettings = new SnipMan.Core.MyRegistry();
            //System.IO.Directory.GetDirectoryRoot(Application.ExecutablePath);
            string iniPath = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
            _snipManSettings = new Methods.IniSettings(iniPath + (!iniPath.EndsWith(@"\") ? @"\" : "") + "snipman.ini" );

            // Process Registry/INI Settings
            //richTextBox1.AcceptsTab = true;
            textEditorControl1.Font = new Font(_snipManSettings.DefaultFont, _snipManSettings.DefaultFontSize);
            textEditorControl1.ShowTabs = false;
            textEditorControl1.ShowSpaces = false;
            textEditorControl1.ShowEOLMarkers = false;            

            alwaysOnTopToolStripMenuItem.Checked = TopMost = _snipManSettings.AlwaysOnTop;
            wordWrapToolStripMenuItem.Checked = true;
            confirmOnDeleteToolStripMenuItem1.Checked = _snipManSettings.ConfirmDelete;
            verticalSplitToolStripMenuItem.Checked = _snipManSettings.VerticalOrient;
            splitContainer1.Orientation = _snipManSettings.VerticalOrient ? Orientation.Vertical : Orientation.Horizontal;
            splitContainer1.SplitterDistance = _snipManSettings.SplitterDistance;
            menuStrip1.Visible = _snipManSettings.ShowMenu;
            hideMenuToolStripMenuItem.Checked = !_snipManSettings.ShowMenu;
            tableLayoutPanel1.RowStyles[tableLayoutPanel1.GetRow(attachment1)] = new RowStyle(SizeType.Absolute, 0);
            autoSaveToolStripMenuItem.Checked = _snipManSettings.AutoSave;
            minimizeToSystemTrayToolStripMenuItem.Checked = _snipManSettings.MinimizeTray;

            //textEditorControl1.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("SQL");
            //ICSharpCode.TextEditor.Util.RtfWriter.
            
            //textEditorControl1.SetHighlighting("JavaScript");
            
            notifyIcon1.Text = "SnipMan";
        }

        private void SetEventHandlers()
        {
            textEditorControl1.Document.TextContentChanged += new EventHandler(Document_TextContentChanged);
            textEditorControl1.Document.DocumentChanged += new DocumentEventHandler(Document_DocumentChanged);
            //richTextBox1.TextChanged += new EventHandler(richTextBox1_TextChanged);
            //treeView1.AfterLabelEdit += new NodeLabelEditEventHandler(treeView1_AfterLabelEdit);
            //treeView1.Leave += new EventHandler(treeView1_Leave); // When it leaves the full treeview control, not just a node.
            //treeView1.BeforeSelect += new TreeViewCancelEventHandler(treeView1_BeforeSelect);
            //treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(treeView1_NodeMouseClick);
            treeView1.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            treeView1.BeforeSelect += new TreeViewCancelEventHandler(treeView1_BeforeSelect);
            toolStripComboBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(toolStripComboBox1_KeyDown);
            notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);
            Resize += new EventHandler(Form1_Resize);
        }

        void Document_DocumentChanged(object sender, DocumentEventArgs e)
        {
            _currentNodeTextChanged = true;
        }

        void Document_TextContentChanged(object sender, EventArgs e)
        {
            _currentNodeTextChanged = true;
        }

        //void richTextBox1_TextChanged(object sender, EventArgs e)
        //{
        //    CurrentNodeTextChanged = true;
        //}

        /// <summary>
        /// Un-minimize and show the window at the top
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Normal;
            if (_snipManSettings.MinimizeTray)
            {
                ShowWindow(Handle, ShowWindowCommand.Restore);
                SetForegroundWindow(Handle);
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// Minimizes to the systray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Form1_Resize(object sender, EventArgs e)
        {
            if (_snipManSettings.MinimizeTray)
            {
                if (WindowState != FormWindowState.Minimized)
                {
                    notifyIcon1.Visible = true;
                    ShowInTaskbar = true;
                }
                else
                {
                    notifyIcon1.Visible = true;
                    ShowInTaskbar = false;
                }
            }
        }

        /// <summary>
        /// Checks for Enter, if it has an enter, then it searches
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void toolStripComboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.KeyValue == 13)
            {
                e.SuppressKeyPress = true;
                toolStripButton4_Click(null, null);
            }
        }

        /// <summary>
        /// Adds a recently searched criteria to the combobox
        /// </summary>
        /// <param name="s"></param>
        private void AddtoComboBox(string s)
        {
            if (!toolStripComboBox1.Items.Contains(s))
            {
                toolStripComboBox1.Items.Add(s);
            }
            return;
        }

        /// <summary>
        /// Sets the Text Color of the Nodes, this also takes care of auto-saving
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            //((SnipMan.Components.TreeNode)e.Node).BackColor = Color.White;
            if (_oldNode != string.Empty && treeView1.ContainsNode(_oldNode))
            {
                treeView1.Node(_oldNode).ForeColor = Color.Black;
                if (_snipManSettings.AutoSave && _currentNodeTextChanged)
                {
                    // Do Save
                    if (MessageBox.Show("Would you like to save changes?", "Auto-Save Snippet", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Methods.SnippetDb.SaveNodeV1(
                            Int32.Parse(((Components.TreeNode)treeView1.Node(_oldNode)).NodeKey.ToString()),
                            textEditorControl1.Text.ToString()
                            );                        
                    }
                }
            }
        }

        /// <summary>
        /// Loads the Nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //richTextBox1.Clear();
            //richTextBox1.ResetText();
            textEditorControl1.ResetText();
            textEditorControl1.Invalidate(true);
            textEditorControl1.Text = Methods.SnippetDb.NodeValueV1((e.Node != null ? Int32.Parse(((Components.TreeNode)e.Node).NodeKey.ToString()) : -1));
            (e?.Node as Components.TreeNode).ForeColor = Color.Red;
            
            _oldNode = (e?.Node as Components.TreeNode)?.Key?.ToString();
            _currentNodeTextChanged = false;
            textEditorControl1.Invalidate(true);
            
            return;
        }

        /// <summary>
        /// After Label Edit, Save the Label Stuff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if(e.Label != null){
                if(e.Label.Length > 0){
                    if (e.Label.IndexOfAny(new char[]{'@', '.', ',', '!'}) == -1)
                    {
                        e.Node.Text = e.Label;
                        e.Node.EndEdit(false);
                        // Store in Database
                        Methods.SnippetDB.InsertNodeV1(
                            (e.Node != null ? Int32.Parse(((SnipMan.Components.TreeNode)e.Node).NodeKey.ToString()) : 0 ),
                            (e.Node.Parent != null ? Int32.Parse(((SnipMan.Components.TreeNode)e.Node.Parent).NodeKey.ToString()) : -1),
                            e.Node.Text
                        );
                        richTextBox1.Text = "";
                    }
                    else
                    {
                        e.CancelEdit = true;
                        MessageBox.Show("Invalid tree node label.\n" + 
                          "The invalid characters are: '@','.', ',', '!'", 
                          "Node Label Edit");
                        e.Node.BeginEdit();
                    }
                }
                this.treeView1.LabelEdit = false;
            }
        }*/

        /// <summary>
        /// Save the Text to the database 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveText();
        }

        /// <summary>
        /// Save the Text to the Database (yep, same as above, gotta love crappy code)
        /// </summary>
        private void SaveText()
        {
            if (treeView1.SelectedNode != null)
            {
                Methods.SnippetDb.SaveNodeV1(
                    Int32.Parse(((Components.TreeNode)treeView1.Node(_oldNode)).NodeKey.ToString()),
                    textEditorControl1.Text.ToString()
                    );
                _currentNodeTextChanged = false;
            }
            return;
        }

        /// <summary>
        /// Add Root Node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            Components.TreeNode treeNode1 = new Components.TreeNode
            {
                NodeKey = (treeView1.LastNode + 1).ToString()
            };
            //(++this.NodeList).ToString();
            treeView1.AddNodeRoot(treeNode1); //.Nodes.Add(treeNode1);
            treeView1.SelectedNode = treeNode1;
            treeView1.LabelEdit = true;
            if (!treeNode1.IsEditing)
            {
                treeNode1.BeginEdit();
            }
        }

        /// <summary>
        /// Add Snippet Node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Components.TreeNode treeNode1 = new Components.TreeNode
            {
                NodeKey = (treeView1.LastNode + 1).ToString()
            };
            treeView1.AddNode(treeNode1, ((Components.TreeNode)(treeView1.SelectedNode)).Key?.ToString()); //.Nodes.Add(treeNode1);
            treeView1.SelectedNode = treeNode1;
            treeView1.LabelEdit = true;
            if (!treeNode1.IsEditing)
            {
                treeNode1.BeginEdit();
            }
        }

        /// <summary>
        /// Delete Node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (_snipManSettings.ConfirmDelete)
            {
                if(MessageBox.Show("Are you sure you want to delete this node?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.No){
                    return;
                }
            }
            if(treeView1.SelectedNode != null){
                Int32 nodeKey = Int32.Parse(((Components.TreeNode)(treeView1.SelectedNode)).NodeKey.ToString());
                if (nodeKey == Methods.SnippetDb.DeleteNode(nodeKey))
                {
                    treeView1.DeleteNode((Components.TreeNode)(treeView1.SelectedNode));
                }
            }
        }

        /// <summary>
        /// Search for Text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            string searchAll = "SELECT UID FROM SNIPPETS WHERE DATA like @D";
            if(toolStripComboBox1.Text == string.Empty){ MessageBox.Show("Please enter search criteria."); return;}
            if (_lastCriteria != toolStripComboBox1.Text)
            {
                AddtoComboBox(toolStripComboBox1.Text);
                SqLiteTypes.SqLiteParam u;
                _lastCriteria = toolStripComboBox1.Text;
                u.Param = "@D"; u.Value = "%" + toolStripComboBox1.Text + "%";
                using (DataTable ans = _db.Db.PreparedQuery(searchAll, u))
                {
                    _findList = new Queue<string>();
                    if (ans == null) { new ErrorHandler("Unable to Query", "SearchSnippets"); return; }
                    foreach (DataRow r in ans.Rows)
                    {
                        _findList.Enqueue(r["UID"].ToString());
                    }
                    if (_findList.Any())
                    {
                        int y = Int32.Parse(_findList.Dequeue());
                        treeView1.SelectedNode = treeView1.Node(y.ToString());
                        //this.richTextBox1.Focus();
                        
                        // Put in seperate function
                        TextEditorSearcher search = new TextEditorSearcher
                        {
                            Document = textEditorControl1.Document,
                            LookFor = toolStripComboBox1.Text,
                            MatchCase = false,
                            MatchWholeWordOnly = false
                        };
                        Caret caret = textEditorControl1.ActiveTextAreaControl.Caret;
                        bool lastSearchLoopedAround;
                        int startFrom = caret.Offset;
                        TextRange range = search.FindNext(startFrom, false, out lastSearchLoopedAround);
                        if (range != null)
                        {
                            Point h1 = textEditorControl1.Document.OffsetToPosition(range.Offset);
                            Point h2 = textEditorControl1.Document.OffsetToPosition(range.Offset + range.Length);
                            textEditorControl1.ActiveTextAreaControl.SelectionManager.SetSelection(h1, h2);
                            textEditorControl1.ActiveTextAreaControl.Caret.Position = textEditorControl1.Document.OffsetToPosition(range.Offset + range.Length);
                            textEditorControl1.ActiveTextAreaControl.ScrollToCaret();
                            _findLocation = range.Offset + range.Length;
                        }
                        else
                        {
                            _findLocation = 0;
                        }
                        //int er = this.richTextBox1.Find(toolStripComboBox1.Text, RichTextBoxFinds.None);                        
                        //this.textEditorControl1.
                        //this.richTextBox1.Select(er, this.toolStripComboBox1.Text.Length);
                        //this.richTextBox1.ScrollToCaret();
                        //FindLocation = er + toolStripComboBox1.Text.Length;
                    }
                    else
                    {
                        MessageBox.Show("End of Search");
                        _lastCriteria = string.Empty;
                        _findLocation = 0;
                    }
                }
            }
            else
            {
                // Still in a Find, goto other places or files.
                //this.richTextBox1.Focus();
                //int er = this.richTextBox1.Find(toolStripComboBox1.Text, FindLocation, RichTextBoxFinds.None);
                TextEditorSearcher search = new TextEditorSearcher
                {
                    Document = textEditorControl1.Document,
                    LookFor = toolStripComboBox1.Text,
                    MatchCase = false,
                    MatchWholeWordOnly = false
                };
                Caret caret = textEditorControl1.ActiveTextAreaControl.Caret;
                int startFrom = caret.Offset;
                TextRange range = search.FindNext(startFrom, false, out var lastSearchLoopedAround);

                if (range != null && !lastSearchLoopedAround)
                {
                    Point h1 = textEditorControl1.Document.OffsetToPosition(range.Offset);
                    Point h2 = textEditorControl1.Document.OffsetToPosition(range.Offset + range.Length);
                    textEditorControl1.ActiveTextAreaControl.SelectionManager.SetSelection(h1, h2);
                    textEditorControl1.ActiveTextAreaControl.Caret.Position = textEditorControl1.Document.OffsetToPosition(range.Offset + range.Length);
                    textEditorControl1.ActiveTextAreaControl.ScrollToCaret();
                    _findLocation = range.Offset + range.Length;
                }
                else
                {
                    if (_findList.Count > 0)
                    {
                        _findLocation = 0;
                        int y = Int32.Parse(_findList.Dequeue());
                        treeView1.SelectedNode = treeView1.Node(y.ToString());
                        ////this.richTextBox1.Focus();
                        //er = this.richTextBox1.Find(toolStripComboBox1.Text, FindLocation, RichTextBoxFinds.None);
                        //this.richTextBox1.Select(er, this.toolStripComboBox1.Text.Length);
                        //this.richTextBox1.ScrollToCaret();
                        //FindLocation = er + toolStripComboBox1.Text.Length;
                        search = new TextEditorSearcher
                        {
                            Document = textEditorControl1.Document,
                            LookFor = toolStripComboBox1.Text,
                            MatchCase = false,
                            MatchWholeWordOnly = false
                        };
                        caret = textEditorControl1.ActiveTextAreaControl.Caret;
                        startFrom = caret.Offset;
                        range = search.FindNext(startFrom, false, out lastSearchLoopedAround);
                        if (range != null)
                        {
                            Point h1 = textEditorControl1.Document.OffsetToPosition(range.Offset);
                            Point h2 = textEditorControl1.Document.OffsetToPosition(range.Offset + range.Length);
                            textEditorControl1.ActiveTextAreaControl.SelectionManager.SetSelection(h1, h2);
                            textEditorControl1.ActiveTextAreaControl.Caret.Position = textEditorControl1.Document.OffsetToPosition(range.Offset + range.Length);
                            textEditorControl1.ActiveTextAreaControl.ScrollToCaret();
                            _findLocation = range.Offset + range.Length;
                        }
                        else
                        {
                            _findLocation = 0;
                        }
                    }
                    else
                    {
                        MessageBox.Show("End of Search");
                        _lastCriteria = "";
                        _findLocation = 0;
                    }
                }
            }
        }

        /// <summary>
        /// About Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            new About(TopMost).ShowDialog(this);
        }

        /// <summary>
        /// Always on Top Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(alwaysOnTopToolStripMenuItem.Checked == false){
                alwaysOnTopToolStripMenuItem.Checked = TopMost = true;
            }else{
                alwaysOnTopToolStripMenuItem.Checked = TopMost = false;
            }
        }

        enum ShowWindowCommand : int
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not actived.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNa = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        private void defaultFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowColor = false;
            //fontDialog1.Font = richTextBox1.Font;
            fontDialog1.Font = textEditorControl1.Font;
            if(fontDialog1.ShowDialog() != DialogResult.Cancel){
                textEditorControl1.Font = fontDialog1.Font;
                _snipManSettings.DefaultFont = fontDialog1.Font.Name;
                _snipManSettings.DefaultFontSize = fontDialog1.Font.Size;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _snipManSettings.AlwaysOnTop = alwaysOnTopToolStripMenuItem.Checked;
                _snipManSettings.SplitterDistance = splitContainer1.SplitterDistance;
                _snipManSettings.SaveSettings();
            }
            catch (Exception ex)
            {
                new ErrorHandler(ex.Message, ex.StackTrace);
            }
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if(wordWrapToolStripMenuItem.Checked == false){
            //    wordWrapToolStripMenuItem.Checked = richTextBox1.WordWrap = true;
            //}else{
            //    wordWrapToolStripMenuItem.Checked = richTextBox1.WordWrap = false;
            //}
        
        }

        private void pasteWoFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (richTextBox1.CanPaste(DataFormats.GetFormat(DataFormats.Text)))
            //{
            //    richTextBox1.Clear();
            //    richTextBox1.ResetText();
            //    richTextBox1.Paste(DataFormats.GetFormat(DataFormats.Text));
            //}
            //else
            //{
                new ErrorHandler("Unable to Paste from Clipboard", "");
            //}
        }

        private void verticalSplitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(verticalSplitToolStripMenuItem.Checked){
                splitContainer1.Orientation = Orientation.Horizontal;
                verticalSplitToolStripMenuItem.Checked = _snipManSettings.VerticalOrient = false;
            }else{
                splitContainer1.Orientation = Orientation.Vertical;
                verticalSplitToolStripMenuItem.Checked = _snipManSettings.VerticalOrient = true;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About(TopMost).ShowDialog(this);
        }

        private void hideMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hideMenuToolStripMenuItem.Checked)
            {
                menuStrip1.Visible = true;
                _snipManSettings.ShowMenu = true;
                hideMenuToolStripMenuItem.Checked = false;
            }
            else
            {
                menuStrip1.Visible = false;
                _snipManSettings.ShowMenu = false;
                hideMenuToolStripMenuItem.Checked = true;
            }
        }

        private void shrinkDatafileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileInfo g = new FileInfo(_db.DbFilename);
            var before = g.Length;
            _db.Db.ShrinkDatabase();
            g.Refresh();
            var after = g.Length;
            MessageBox.Show("Recovered " + (before - after).ToString() + " bytes.");
        }

        private void defaultFontToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowColor = false;
            fontDialog1.Font = textEditorControl1.Font;
            if (fontDialog1.ShowDialog() != DialogResult.Cancel)
            {
                textEditorControl1.Font = fontDialog1.Font;
                _snipManSettings.DefaultFont = fontDialog1.Font.Name;
                _snipManSettings.DefaultFontSize = fontDialog1.Font.Size;
            }
        }

        private void confirmOnDeleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            confirmOnDeleteToolStripMenuItem1.Checked = _snipManSettings.ConfirmDelete = !_snipManSettings.ConfirmDelete;
        }

        private void autoSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoSaveToolStripMenuItem.Checked = _snipManSettings.AutoSave = !_snipManSettings.AutoSave;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void createNewDatabaseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openSQLiteConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Misc.Forms.SqlConsoleWin(treeView1).Show();
        }

        private void relocateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Misc.Forms.MoveNode h =
                new Misc.Forms.MoveNode(((Components.TreeNode) (treeView1.SelectedNode)).Key?.ToString())
                {
                    RefreshTreeView =
                        new Misc.Forms.MoveNode.RefreshTreeViewDelegate(treeView1.RefreshTreeView)
                };
            h.ShowDialog();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.LabelEdit = true;
            if (!treeView1.SelectedNode.IsEditing)
            {
                treeView1.SelectedNode.BeginEdit();
            }
        }

        private void importDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                //MessageBox.Show("This process is NOT threaded and may temporarily lock-up SnipMan!", "SnipMan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*int id = Methods.SnippetDB.LastNodeId();
                Methods.SnippetDB.InsertNodeV1(id + 1, -1, "Imported_" + DateTime.Now.ToShortDateString());
                Methods.ImportS3DB j = new Methods.ImportS3DB(FileName, id + 1);
                j.Process();*/
                Misc.Forms.ImportForm h = new Misc.Forms.ImportForm(fileName);
                if(h.ShowDialog() == DialogResult.OK)
                {
                    treeView1.RefreshTreeView();
                }
            }
            openFileDialog1.Dispose();
            return;
        }

        private void testLINQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable y = DBase.Instance.Db.PreparedQuery("SELECT NULL FROM SNIPPETS");
            IDataListObj kdsfsd = new JavaScript.Implementations.JsDataListObj(y);
            kdsfsd.Row(0);
        }

        private void minimizeToSystemTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (minimizeToSystemTrayToolStripMenuItem.Checked)
            {
                minimizeToSystemTrayToolStripMenuItem.Checked = _snipManSettings.MinimizeTray = false;
            }
            else
            {
                minimizeToSystemTrayToolStripMenuItem.Checked = _snipManSettings.MinimizeTray = true;
            }
        }

        private void javaScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;
            
            javaScriptToolStripMenuItem1.Checked = true;
            _selectedHighlighting.Push(javaScriptToolStripMenuItem1);
            textEditorControl1.SetHighlighting("JavaScript");
            textEditorControl1.Invalidate(true);
        }

        private void cToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            cToolStripMenuItem1.Checked = true;
            _selectedHighlighting.Push(cToolStripMenuItem1);
            textEditorControl1.SetHighlighting("C#");
            textEditorControl1.Invalidate(true);
        }

        private void hTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            hTMLToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(hTMLToolStripMenuItem);
            textEditorControl1.SetHighlighting("HTML");
            textEditorControl1.Invalidate(true);
        }

        // ASP.NET
        private void javaScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            javaScriptToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(javaScriptToolStripMenuItem);
            textEditorControl1.SetHighlighting("ASP/XHTML");
            textEditorControl1.Invalidate(true);
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            cToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(cToolStripMenuItem);
            textEditorControl1.SetHighlighting("C++.NET");
            textEditorControl1.Invalidate(true);
        }

        private void batchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            batchToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(batchToolStripMenuItem);
            textEditorControl1.SetHighlighting("BAT");
            textEditorControl1.Invalidate(true);
        }

        private void booToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            booToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(booToolStripMenuItem);
            textEditorControl1.SetHighlighting("BOO");
            textEditorControl1.Invalidate(true);
        }

        private void javaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            javaToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(javaToolStripMenuItem);
            textEditorControl1.SetHighlighting("Java");
            textEditorControl1.Invalidate(true);
        }

        private void pHPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            pHPToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(pHPToolStripMenuItem);
            textEditorControl1.SetHighlighting("PHP");
            textEditorControl1.Invalidate(true);
        }

        private void vBNETToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            vBNETToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(vBNETToolStripMenuItem);
            textEditorControl1.SetHighlighting("VBNET");
            textEditorControl1.Invalidate(true);
        }

        private void xMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedHighlighting.Count > 0)
                (_selectedHighlighting.Pop()).Checked = false;

            xMLToolStripMenuItem.Checked = true;
            _selectedHighlighting.Push(xMLToolStripMenuItem);
            textEditorControl1.SetHighlighting("XML");
            textEditorControl1.Invalidate(true);
        }

        private void syncOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Misc.Forms.FrmSyncOpt h = new Misc.Forms.FrmSyncOpt
            {
                StartPosition = FormStartPosition.CenterParent
            };
            h.ShowDialog(this);
        }
    }
}
