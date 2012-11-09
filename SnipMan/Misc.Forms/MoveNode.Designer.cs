namespace SnipMan.Misc.Forms
{
    partial class MoveNode
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
            this.nodeListBox = new System.Windows.Forms.ListBox();
            this.MoveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // nodeListBox
            // 
            this.nodeListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nodeListBox.FormattingEnabled = true;
            this.nodeListBox.Location = new System.Drawing.Point(2, 2);
            this.nodeListBox.Name = "nodeListBox";
            this.nodeListBox.Size = new System.Drawing.Size(249, 93);
            this.nodeListBox.TabIndex = 0;
            // 
            // MoveButton
            // 
            this.MoveButton.Location = new System.Drawing.Point(176, 101);
            this.MoveButton.Name = "MoveButton";
            this.MoveButton.Size = new System.Drawing.Size(75, 23);
            this.MoveButton.TabIndex = 1;
            this.MoveButton.Text = "Move";
            this.MoveButton.UseVisualStyleBackColor = true;
            this.MoveButton.Click += new System.EventHandler(this.Move_Click);
            // 
            // MoveNode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 127);
            this.Controls.Add(this.MoveButton);
            this.Controls.Add(this.nodeListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MoveNode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Move...";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox nodeListBox;
        private System.Windows.Forms.Button MoveButton;
    }
}