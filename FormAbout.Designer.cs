namespace TacticalOpsQuickJoin {
    partial class FormAbout {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.linkLabelWebsite = new System.Windows.Forms.LinkLabel();
            this.linlLabelSteam = new System.Windows.Forms.LinkLabel();
            this.lblAboutInfo = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblModInfo = new System.Windows.Forms.Label(); // NEU: Instanzierung
            this.SuspendLayout();
            // 
            // linkLabelWebsite
            // 
            this.linkLabelWebsite.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.linkLabelWebsite.Location = new System.Drawing.Point(92, 280);
            this.linkLabelWebsite.Name = "linkLabelWebsite";
            this.linkLabelWebsite.Size = new System.Drawing.Size(100, 13);
            this.linkLabelWebsite.TabIndex = 1;
            this.linkLabelWebsite.TabStop = true;
            this.linkLabelWebsite.Text = "Visit jildert.com";
            this.linkLabelWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelWebsite.Visible = false;

            // 
            // linlLabelSteam
            // 
            this.linlLabelSteam.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.linlLabelSteam.Location = new System.Drawing.Point(92, 255);
            this.linlLabelSteam.Margin = new System.Windows.Forms.Padding(0);
            this.linlLabelSteam.Name = "linlLabelSteam";
            this.linlLabelSteam.Size = new System.Drawing.Size(100, 13);
            this.linlLabelSteam.TabIndex = 2;
            this.linlLabelSteam.TabStop = true;
            this.linlLabelSteam.Text = "Visit steamprofile";
            this.linlLabelSteam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linlLabelSteam.Visible = false;

            // 
            // lblAboutInfo
            // 
            this.lblAboutInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAboutInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAboutInfo.Location = new System.Drawing.Point(0, 9);
            this.lblAboutInfo.Name = "lblAboutInfo";
            this.lblAboutInfo.Size = new System.Drawing.Size(284, 120);
            this.lblAboutInfo.TabIndex = 3;
            this.lblAboutInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(244, 278);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(37, 13);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "v1.1.4";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblVersion.Click += new System.EventHandler(this.lblVersion_Click);
            // 
            // lblModInfo
            // 
            this.lblModInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModInfo.Location = new System.Drawing.Point(0, 140);
            this.lblModInfo.Name = "lblModInfo";
            this.lblModInfo.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.lblModInfo.Size = new System.Drawing.Size(284, 120);
            this.lblModInfo.TabIndex = 5;
            this.lblModInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(284, 300);
            this.Controls.Add(this.lblModInfo); 
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblAboutInfo);
            this.Controls.Add(this.linlLabelSteam);
            this.Controls.Add(this.linkLabelWebsite);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "About";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.LinkLabel linkLabelWebsite;
        private System.Windows.Forms.LinkLabel linlLabelSteam;
        private System.Windows.Forms.Label lblAboutInfo;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblModInfo; // Endgültige Deklaration
    }
}