namespace SnipMan.Misc.Forms
{
    partial class SqlConsoleWin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sqlConsole1 = new SnipMan.Components.SqlConsole();
            this.SuspendLayout();
            // 
            // sqlConsole1
            // 
            this.sqlConsole1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sqlConsole1.Location = new System.Drawing.Point(0, 0);
            this.sqlConsole1.Name = "sqlConsole1";
            this.sqlConsole1.Size = new System.Drawing.Size(427, 110);
            this.sqlConsole1.TabIndex = 0;
            // 
            // SQLConsoleWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 110);
            this.Controls.Add(this.sqlConsole1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "SqlConsoleWin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SQLite Console - Use at your own risk!";
            this.ResumeLayout(false);

        }

        #endregion

        private Components.SqlConsole sqlConsole1;
    }
}