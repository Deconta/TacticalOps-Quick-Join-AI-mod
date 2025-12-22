namespace TacticalOpsQuickJoin
{
    partial class FormSettings
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.generalTabPage = new System.Windows.Forms.TabPage();
            this.ignoredServersButton = new System.Windows.Forms.Button();
            this.autoRefreshIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.autoRefreshLabel = new System.Windows.Forms.Label();
            this.darkModeCheckBox = new System.Windows.Forms.CheckBox();
            this.closeOnJoinCheckBox = new System.Windows.Forms.CheckBox();
            this.pathsTabPage = new System.Windows.Forms.TabPage();
            this.browseTO35Button = new System.Windows.Forms.Button();
            this.pathTO35TextBox = new System.Windows.Forms.TextBox();
            this.labelTO35 = new System.Windows.Forms.Label();
            this.browseTO34Button = new System.Windows.Forms.Button();
            this.pathTO34TextBox = new System.Windows.Forms.TextBox();
            this.labelTO34 = new System.Windows.Forms.Label();
            this.browseTO22Button = new System.Windows.Forms.Button();
            this.pathTO22TextBox = new System.Windows.Forms.TextBox();
            this.labelTO22 = new System.Windows.Forms.Label();
            this.masterServersTabPage = new System.Windows.Forms.TabPage();
            this.masterServersDescriptionLabel = new System.Windows.Forms.Label();
            this.masterServersTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.generalTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoRefreshIntervalNumericUpDown)).BeginInit();
            this.pathsTabPage.SuspendLayout();
            this.masterServersTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.generalTabPage);
            this.tabControl.Controls.Add(this.pathsTabPage);
            this.tabControl.Controls.Add(this.masterServersTabPage);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(460, 250);
            this.tabControl.TabIndex = 0;
            // 
            // generalTabPage
            // 
            this.generalTabPage.Controls.Add(this.ignoredServersButton);
            this.generalTabPage.Controls.Add(this.autoRefreshIntervalNumericUpDown);
            this.generalTabPage.Controls.Add(this.autoRefreshLabel);
            this.generalTabPage.Controls.Add(this.darkModeCheckBox);
            this.generalTabPage.Controls.Add(this.closeOnJoinCheckBox);
            this.generalTabPage.Location = new System.Drawing.Point(4, 22);
            this.generalTabPage.Name = "generalTabPage";
            this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalTabPage.Size = new System.Drawing.Size(452, 224);
            this.generalTabPage.TabIndex = 0;
            this.generalTabPage.Text = "General";
            this.generalTabPage.UseVisualStyleBackColor = true;
            // 
            // ignoredServersButton
            // 
            this.ignoredServersButton.Location = new System.Drawing.Point(15, 120);
            this.ignoredServersButton.Name = "ignoredServersButton";
            this.ignoredServersButton.Size = new System.Drawing.Size(150, 23);
            this.ignoredServersButton.TabIndex = 4;
            this.ignoredServersButton.Text = "Ignored Servers...";
            this.ignoredServersButton.UseVisualStyleBackColor = true;
            this.ignoredServersButton.Click += new System.EventHandler(this.ignoredServersButton_Click);
            // 
            // autoRefreshIntervalNumericUpDown
            // 
            this.autoRefreshIntervalNumericUpDown.Location = new System.Drawing.Point(140, 68);
            this.autoRefreshIntervalNumericUpDown.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.autoRefreshIntervalNumericUpDown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.autoRefreshIntervalNumericUpDown.Name = "autoRefreshIntervalNumericUpDown";
            this.autoRefreshIntervalNumericUpDown.Size = new System.Drawing.Size(50, 20);
            this.autoRefreshIntervalNumericUpDown.TabIndex = 3;
            this.autoRefreshIntervalNumericUpDown.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // autoRefreshLabel
            // 
            this.autoRefreshLabel.AutoSize = true;
            this.autoRefreshLabel.Location = new System.Drawing.Point(12, 70);
            this.autoRefreshLabel.Name = "autoRefreshLabel";
            this.autoRefreshLabel.Size = new System.Drawing.Size(122, 13);
            this.autoRefreshLabel.TabIndex = 2;
            this.autoRefreshLabel.Text = "Auto-Refresh Interval (s):";
            // 
            // darkModeCheckBox
            // 
            this.darkModeCheckBox.AutoSize = true;
            this.darkModeCheckBox.Location = new System.Drawing.Point(15, 40);
            this.darkModeCheckBox.Name = "darkModeCheckBox";
            this.darkModeCheckBox.Size = new System.Drawing.Size(115, 17);
            this.darkModeCheckBox.TabIndex = 1;
            this.darkModeCheckBox.Text = "Enable Dark Mode";
            this.darkModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // closeOnJoinCheckBox
            // 
            this.closeOnJoinCheckBox.AutoSize = true;
            this.closeOnJoinCheckBox.Location = new System.Drawing.Point(15, 15);
            this.closeOnJoinCheckBox.Name = "closeOnJoinCheckBox";
            this.closeOnJoinCheckBox.Size = new System.Drawing.Size(167, 17);
            this.closeOnJoinCheckBox.TabIndex = 0;
            this.closeOnJoinCheckBox.Text = "Close app when joining server";
            this.closeOnJoinCheckBox.UseVisualStyleBackColor = true;
            // 
            // pathsTabPage
            // 
            this.pathsTabPage.Controls.Add(this.browseTO35Button);
            this.pathsTabPage.Controls.Add(this.pathTO35TextBox);
            this.pathsTabPage.Controls.Add(this.labelTO35);
            this.pathsTabPage.Controls.Add(this.browseTO34Button);
            this.pathsTabPage.Controls.Add(this.pathTO34TextBox);
            this.pathsTabPage.Controls.Add(this.labelTO34);
            this.pathsTabPage.Controls.Add(this.browseTO22Button);
            this.pathsTabPage.Controls.Add(this.pathTO22TextBox);
            this.pathsTabPage.Controls.Add(this.labelTO22);
            this.pathsTabPage.Location = new System.Drawing.Point(4, 22);
            this.pathsTabPage.Name = "pathsTabPage";
            this.pathsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.pathsTabPage.Size = new System.Drawing.Size(452, 224);
            this.pathsTabPage.TabIndex = 1;
            this.pathsTabPage.Text = "Paths";
            this.pathsTabPage.UseVisualStyleBackColor = true;
            // 
            // browseTO35Button
            // 
            this.browseTO35Button.Location = new System.Drawing.Point(360, 85);
            this.browseTO35Button.Name = "browseTO35Button";
            this.browseTO35Button.Size = new System.Drawing.Size(75, 23);
            this.browseTO35Button.TabIndex = 8;
            this.browseTO35Button.Text = "Browse...";
            this.browseTO35Button.UseVisualStyleBackColor = true;
            this.browseTO35Button.Click += new System.EventHandler(this.browseTO35Button_Click);
            // 
            // pathTO35TextBox
            // 
            this.pathTO35TextBox.Location = new System.Drawing.Point(140, 87);
            this.pathTO35TextBox.Name = "pathTO35TextBox";
            this.pathTO35TextBox.Size = new System.Drawing.Size(214, 20);
            this.pathTO35TextBox.TabIndex = 7;
            // 
            // labelTO35
            // 
            this.labelTO35.AutoSize = true;
            this.labelTO35.Location = new System.Drawing.Point(15, 90);
            this.labelTO35.Name = "labelTO35";
            this.labelTO35.Size = new System.Drawing.Size(119, 13);
            this.labelTO35.TabIndex = 6;
            this.labelTO35.Text = "Tactical Ops 3.5 Path:";
            // 
            // browseTO34Button
            // 
            this.browseTO34Button.Location = new System.Drawing.Point(360, 55);
            this.browseTO34Button.Name = "browseTO34Button";
            this.browseTO34Button.Size = new System.Drawing.Size(75, 23);
            this.browseTO34Button.TabIndex = 5;
            this.browseTO34Button.Text = "Browse...";
            this.browseTO34Button.UseVisualStyleBackColor = true;
            this.browseTO34Button.Click += new System.EventHandler(this.browseTO34Button_Click);
            // 
            // pathTO34TextBox
            // 
            this.pathTO34TextBox.Location = new System.Drawing.Point(140, 57);
            this.pathTO34TextBox.Name = "pathTO34TextBox";
            this.pathTO34TextBox.Size = new System.Drawing.Size(214, 20);
            this.pathTO34TextBox.TabIndex = 4;
            // 
            // labelTO34
            // 
            this.labelTO34.AutoSize = true;
            this.labelTO34.Location = new System.Drawing.Point(15, 60);
            this.labelTO34.Name = "labelTO34";
            this.labelTO34.Size = new System.Drawing.Size(119, 13);
            this.labelTO34.TabIndex = 3;
            this.labelTO34.Text = "Tactical Ops 3.4 Path:";
            // 
            // browseTO22Button
            // 
            this.browseTO22Button.Location = new System.Drawing.Point(360, 25);
            this.browseTO22Button.Name = "browseTO22Button";
            this.browseTO22Button.Size = new System.Drawing.Size(75, 23);
            this.browseTO22Button.TabIndex = 2;
            this.browseTO22Button.Text = "Browse...";
            this.browseTO22Button.UseVisualStyleBackColor = true;
            this.browseTO22Button.Click += new System.EventHandler(this.browseTO22Button_Click);
            // 
            // pathTO22TextBox
            // 
            this.pathTO22TextBox.Location = new System.Drawing.Point(140, 27);
            this.pathTO22TextBox.Name = "pathTO22TextBox";
            this.pathTO22TextBox.Size = new System.Drawing.Size(214, 20);
            this.pathTO22TextBox.TabIndex = 1;
            // 
            // labelTO22
            // 
            this.labelTO22.AutoSize = true;
            this.labelTO22.Location = new System.Drawing.Point(15, 30);
            this.labelTO22.Name = "labelTO22";
            this.labelTO22.Size = new System.Drawing.Size(119, 13);
            this.labelTO22.TabIndex = 0;
            this.labelTO22.Text = "Tactical Ops 2.2 Path:";
            // 
            // masterServersTabPage
            // 
            this.masterServersTabPage.Controls.Add(this.masterServersDescriptionLabel);
            this.masterServersTabPage.Controls.Add(this.masterServersTextBox);
            this.masterServersTabPage.Location = new System.Drawing.Point(4, 22);
            this.masterServersTabPage.Name = "masterServersTabPage";
            this.masterServersTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.masterServersTabPage.Size = new System.Drawing.Size(452, 224);
            this.masterServersTabPage.TabIndex = 2;
            this.masterServersTabPage.Text = "Master Servers";
            this.masterServersTabPage.UseVisualStyleBackColor = true;
            // 
            // masterServersDescriptionLabel
            // 
            this.masterServersDescriptionLabel.AutoSize = true;
            this.masterServersDescriptionLabel.Location = new System.Drawing.Point(15, 15);
            this.masterServersDescriptionLabel.Name = "masterServersDescriptionLabel";
            this.masterServersDescriptionLabel.Size = new System.Drawing.Size(232, 13);
            this.masterServersDescriptionLabel.TabIndex = 1;
            this.masterServersDescriptionLabel.Text = "Enter master servers, one per line (host:port).";
            // 
            // masterServersTextBox
            // 
            this.masterServersTextBox.AcceptsReturn = true;
            this.masterServersTextBox.Location = new System.Drawing.Point(15, 35);
            this.masterServersTextBox.Multiline = true;
            this.masterServersTextBox.Name = "masterServersTextBox";
            this.masterServersTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.masterServersTextBox.Size = new System.Drawing.Size(420, 180);
            this.masterServersTextBox.TabIndex = 0;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(235, 270);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(316, 270);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(397, 270);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // FormSettings
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 301);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.generalTabPage.ResumeLayout(false);
            this.generalTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoRefreshIntervalNumericUpDown)).EndInit();
            this.pathsTabPage.ResumeLayout(false);
            this.pathsTabPage.PerformLayout();
            this.masterServersTabPage.ResumeLayout(false);
            this.masterServersTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage generalTabPage;
        private System.Windows.Forms.TabPage pathsTabPage;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.CheckBox closeOnJoinCheckBox;
        private System.Windows.Forms.CheckBox darkModeCheckBox;
        private System.Windows.Forms.Label autoRefreshLabel;
        private System.Windows.Forms.NumericUpDown autoRefreshIntervalNumericUpDown;
        private System.Windows.Forms.Button ignoredServersButton;
        private System.Windows.Forms.Label labelTO22;
        private System.Windows.Forms.TextBox pathTO22TextBox;
        private System.Windows.Forms.Button browseTO22Button;
        private System.Windows.Forms.Label labelTO34;
        private System.Windows.Forms.TextBox pathTO34TextBox;
        private System.Windows.Forms.Button browseTO34Button;
        private System.Windows.Forms.Label labelTO35;
        private System.Windows.Forms.TextBox pathTO35TextBox;
        private System.Windows.Forms.Button browseTO35Button;
        private System.Windows.Forms.TabPage masterServersTabPage;
        private System.Windows.Forms.TextBox masterServersTextBox;
        private System.Windows.Forms.Label masterServersDescriptionLabel;
    }
}