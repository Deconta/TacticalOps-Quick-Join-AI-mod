namespace TacticalOpsQuickJoin
{
    partial class FormIgnoredServers
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.ignoredServersListBox = new System.Windows.Forms.ListBox();
            this.removeButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ignoredServersListBox
            // 
            this.ignoredServersListBox.FormattingEnabled = true;
            this.ignoredServersListBox.ItemHeight = 15;
            this.ignoredServersListBox.Location = new System.Drawing.Point(12, 12);
            this.ignoredServersListBox.Name = "ignoredServersListBox";
            this.ignoredServersListBox.Size = new System.Drawing.Size(260, 184);
            this.ignoredServersListBox.TabIndex = 0;
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(12, 202);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 1;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(197, 202);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // FormIgnoredServers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 237);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.ignoredServersListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormIgnoredServers";
            this.Text = "Ignored Servers";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListBox ignoredServersListBox;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button closeButton;
    }
}
