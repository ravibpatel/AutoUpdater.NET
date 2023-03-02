namespace AutoUpdaterTest
{
    partial class FormMain
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
            this.buttonCheckForUpdateViaHttp = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.buttonCheckForUpdateViaSftp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCheckForUpdateViaHttp
            // 
            this.buttonCheckForUpdateViaHttp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCheckForUpdateViaHttp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCheckForUpdateViaHttp.Location = new System.Drawing.Point(12, 54);
            this.buttonCheckForUpdateViaHttp.Name = "buttonCheckForUpdateViaHttp";
            this.buttonCheckForUpdateViaHttp.Size = new System.Drawing.Size(161, 40);
            this.buttonCheckForUpdateViaHttp.TabIndex = 0;
            this.buttonCheckForUpdateViaHttp.Text = "Check for update via HTTP";
            this.buttonCheckForUpdateViaHttp.UseVisualStyleBackColor = true;
            this.buttonCheckForUpdateViaHttp.Click += new System.EventHandler(this.ButtonCheckForUpdateViaHttp_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.Location = new System.Drawing.Point(9, 23);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(130, 15);
            this.labelVersion.TabIndex = 1;
            this.labelVersion.Text = "Current version : 1.0.0.0";
            // 
            // buttonCheckForUpdateViaSftp
            // 
            this.buttonCheckForUpdateViaSftp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCheckForUpdateViaSftp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCheckForUpdateViaSftp.Location = new System.Drawing.Point(203, 54);
            this.buttonCheckForUpdateViaSftp.Name = "buttonCheckForUpdateViaSftp";
            this.buttonCheckForUpdateViaSftp.Size = new System.Drawing.Size(161, 40);
            this.buttonCheckForUpdateViaSftp.TabIndex = 2;
            this.buttonCheckForUpdateViaSftp.Text = "Check for update via SFTP";
            this.buttonCheckForUpdateViaSftp.UseVisualStyleBackColor = true;
            this.buttonCheckForUpdateViaSftp.Click += new System.EventHandler(this.buttonCheckForUpdateViaSftp_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 106);
            this.Controls.Add(this.buttonCheckForUpdateViaSftp);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonCheckForUpdateViaHttp);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMain";
            this.ShowIcon = false;
            this.Text = "AutoUpdaterTest";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCheckForUpdateViaHttp;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonCheckForUpdateViaSftp;
    }
}