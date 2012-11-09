using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SnipMan.Methods;

namespace SnipMan.Misc.Forms
{
    public partial class FrmSyncOpt : Form
    {
        private Dictionary<string, string> _sets;

        public FrmSyncOpt()
        {
            InitializeComponent();
            PopulateSyncList();
            CreateSettingInput();
        }

        private void PopulateSyncList()
        {
            comboBox1.Items.Add(new Methods.Sync.Ftp());
            comboBox1.SelectedIndex = 0;
        }

        private void CreateSettingInput()
        {
            foreach (var f in comboBox1.Items)
            {
                TableLayoutPanel tlp = new TableLayoutPanel();
                tabControl1.TabPages.Add(f.ToString(), f.ToString());
                tabControl1.TabPages[f.ToString()].Controls.Add(tlp);
                tlp.ColumnCount = 2; tlp.RowCount = 1;
                tlp.Dock = DockStyle.Fill;
                tlp.AutoScroll = true;
                _sets = ((Methods.Sync.ISync)f).Settings();
                foreach (KeyValuePair<string, string> setting in ((Methods.Sync.ISync)f).Settings())
                {                    
                    Label tmpLbl = new Label();
                    TextBox tmpTxt = new TextBox();
                    tmpLbl.Text = setting.Key;
                    tmpTxt.Text = setting.Value;
                    tmpTxt.TextChanged += delegate(object sender, EventArgs e)
                    {
                        if (_sets.ContainsKey(tmpLbl.Text))
                            ((Dictionary<string, string>)_sets)[tmpLbl.Text] = tmpTxt.Text;
                    }; 
                    tlp.Controls.Add(tmpLbl, 0, tlp.RowCount);
                    tlp.Controls.Add(tmpTxt, 1, tlp.RowCount);
                    tlp.RowCount++;
                }
            }
        }

        private void SaveSyncOptions()
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                string type = comboBox1.Items[comboBox1.SelectedIndex].ToString();
                OptionsDb.SaveSyncType(type);
                ((Methods.Sync.ISync)comboBox1.Items[comboBox1.SelectedIndex]).WriteSettings(_sets);
                ((Methods.Sync.ISync)comboBox1.Items[comboBox1.SelectedIndex]).SaveSettings();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveSyncOptions();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
