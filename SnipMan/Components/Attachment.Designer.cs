namespace SnipMan.Components
{
    partial class Attachment
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Attachment));
            this.ddlAttachList = new System.Windows.Forms.ComboBox();
            this.btnDeleteAttach = new System.Windows.Forms.Button();
            this.btnAddAttach = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ddlAttachList
            // 
            this.ddlAttachList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlAttachList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ddlAttachList.FormattingEnabled = true;
            this.ddlAttachList.Location = new System.Drawing.Point(71, 5);
            this.ddlAttachList.Name = "ddlAttachList";
            this.ddlAttachList.Size = new System.Drawing.Size(185, 21);
            this.ddlAttachList.TabIndex = 5;
            // 
            // btnDeleteAttach
            // 
            this.btnDeleteAttach.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDeleteAttach.FlatAppearance.BorderSize = 0;
            this.btnDeleteAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteAttach.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteAttach.Image")));
            this.btnDeleteAttach.Location = new System.Drawing.Point(34, 2);
            this.btnDeleteAttach.Name = "btnDeleteAttach";
            this.btnDeleteAttach.Size = new System.Drawing.Size(31, 23);
            this.btnDeleteAttach.TabIndex = 6;
            this.btnDeleteAttach.UseVisualStyleBackColor = true;
            // 
            // btnAddAttach
            // 
            this.btnAddAttach.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAddAttach.FlatAppearance.BorderSize = 0;
            this.btnAddAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddAttach.Image = ((System.Drawing.Image)(resources.GetObject("btnAddAttach.Image")));
            this.btnAddAttach.Location = new System.Drawing.Point(3, 3);
            this.btnAddAttach.Name = "btnAddAttach";
            this.btnAddAttach.Size = new System.Drawing.Size(31, 23);
            this.btnAddAttach.TabIndex = 4;
            this.btnAddAttach.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(262, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(31, 23);
            this.button1.TabIndex = 7;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Attachment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDeleteAttach);
            this.Controls.Add(this.ddlAttachList);
            this.Controls.Add(this.btnAddAttach);
            this.Name = "Attachment";
            this.Size = new System.Drawing.Size(303, 30);
            this.Load += new System.EventHandler(this.Attachment_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDeleteAttach;
        private System.Windows.Forms.ComboBox ddlAttachList;
        private System.Windows.Forms.Button btnAddAttach;
        private System.Windows.Forms.Button button1;
    }
}
