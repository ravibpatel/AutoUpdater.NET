namespace AutoUpdaterTest
{
    partial class RemindLaterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemindLaterForm));
            this.labelTitle = new System.Windows.Forms.Label();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.radioButtonYes = new System.Windows.Forms.RadioButton();
            this.radioButtonNo = new System.Windows.Forms.RadioButton();
            this.comboBoxRemindLater = new System.Windows.Forms.ComboBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.tableLayoutPanel.SetColumnSpan(this.labelTitle, 2);
            this.labelTitle.Name = "labelTitle";
            // 
            // pictureBoxIcon
            // 
            resources.ApplyResources(this.pictureBoxIcon, "pictureBoxIcon");
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.tableLayoutPanel.SetRowSpan(this.pictureBoxIcon, 2);
            this.pictureBoxIcon.TabStop = false;
            // 
            // labelDescription
            // 
            resources.ApplyResources(this.labelDescription, "labelDescription");
            this.tableLayoutPanel.SetColumnSpan(this.labelDescription, 2);
            this.labelDescription.Name = "labelDescription";
            // 
            // radioButtonYes
            // 
            resources.ApplyResources(this.radioButtonYes, "radioButtonYes");
            this.radioButtonYes.Checked = true;
            this.radioButtonYes.Name = "radioButtonYes";
            this.radioButtonYes.TabStop = true;
            this.radioButtonYes.UseVisualStyleBackColor = true;
            this.radioButtonYes.CheckedChanged += new System.EventHandler(this.RadioButtonYesCheckedChanged);
            // 
            // radioButtonNo
            // 
            resources.ApplyResources(this.radioButtonNo, "radioButtonNo");
            this.tableLayoutPanel.SetColumnSpan(this.radioButtonNo, 2);
            this.radioButtonNo.Name = "radioButtonNo";
            this.radioButtonNo.UseVisualStyleBackColor = true;
            // 
            // comboBoxRemindLater
            // 
            resources.ApplyResources(this.comboBoxRemindLater, "comboBoxRemindLater");
            this.comboBoxRemindLater.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRemindLater.FormattingEnabled = true;
            this.comboBoxRemindLater.Items.AddRange(new object[] {
            resources.GetString("comboBoxRemindLater.Items"),
            resources.GetString("comboBoxRemindLater.Items1"),
            resources.GetString("comboBoxRemindLater.Items2"),
            resources.GetString("comboBoxRemindLater.Items3"),
            resources.GetString("comboBoxRemindLater.Items4"),
            resources.GetString("comboBoxRemindLater.Items5"),
            resources.GetString("comboBoxRemindLater.Items6")});
            this.comboBoxRemindLater.Name = "comboBoxRemindLater";
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.pictureBoxIcon, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.labelTitle, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.radioButtonNo, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.comboBoxRemindLater, 2, 2);
            this.tableLayoutPanel.Controls.Add(this.radioButtonYes, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.labelDescription, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.buttonOK, 2, 4);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // RemindLaterForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemindLaterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.RemindLaterFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.RadioButton radioButtonYes;
        private System.Windows.Forms.RadioButton radioButtonNo;
        private System.Windows.Forms.ComboBox comboBoxRemindLater;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    }
}