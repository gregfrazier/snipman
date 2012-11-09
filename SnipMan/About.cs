using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnipMan
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        public About(bool ontop){
            InitializeComponent();
            if(ontop){
                TopMost = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void About_Load(object sender, EventArgs e)
        {
            ProgramLabel.Text = Core.Config.ApplicationName + " " + Core.Config.ApplicationVersion + " (" + Core.Config.ApplicationCompileTarget + ")";
            linkLabel2.Text = "The Snippet Manager\n" +
                              "(c)2009-2012 - Greg Frazier of epic monstrosity\n" +
                              "This software is freeware.\n\n" +
                              "Some Icons are Copyright © Yusuke Kamiyamane.\nAll rights reserved.\n\n" +
                              "Uses ICSharpCode.TextEditor Library from SharpDevelop.\n" +
                              "Source is available at http://www.icsharpcode.net/OpenSource/SD/";
            linkLabel2.Links.Add(35, 32, "http://epicmonstrosity.com/");
            linkLabel2.Links.Add(123, 17, "http://p.yusukekamiyamane.com/");
            linkLabel2.Links.Add(205, 12, "http://www.icsharpcode.net/"); //62
            linkLabel2.Links.Add(267, 41, "http://www.icsharpcode.net/OpenSource/SD/");
            transparentLabel1.Text = "Build Date: " + Version.BuildDate;
            transparentLabel1.TextAlign = ContentAlignment.MiddleRight;
            transparentLabel1.ForeColor = Color.White;
            
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }
}
