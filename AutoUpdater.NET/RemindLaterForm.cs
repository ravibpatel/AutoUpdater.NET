using System;
using System.Windows.Forms;

namespace AutoUpdaterDotNET
{
    public partial class RemindLaterForm : Form
    {
        public AutoUpdater.RemindLaterFormat RemindLaterFormat { get; set; }

        public int RemindLaterAt { get; set; }

        public RemindLaterForm(string appTitle)
        {
            InitializeComponent();
            labelDescription.Text = string.Format("You should download updates now. This only takes few minutes depending on your internet connection and ensures you have latest version of {0}.", appTitle);
        }

        private void RemindLaterFormLoad(object sender, EventArgs e)
        {
            comboBoxRemindLater.SelectedIndex = 0;
            radioButtonYes.Checked = true;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (radioButtonYes.Checked)
            {
                switch (comboBoxRemindLater.SelectedIndex)
                {
                    case 0:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Minutes;
                        RemindLaterAt = 30;
                        break;
                    case 1:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Hours;
                        RemindLaterAt = 12;
                        break;
                    case 2:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Days;
                        RemindLaterAt = 1;
                        break;
                    case 3:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Days;
                        RemindLaterAt = 2;
                        break;
                    case 4:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Days;
                        RemindLaterAt = 4;
                        break;
                    case 5:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Days;
                        RemindLaterAt = 8;
                        break;
                    case 6:
                        RemindLaterFormat = AutoUpdater.RemindLaterFormat.Days;
                        RemindLaterAt = 10;
                        break;
                }
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Abort;
            }
        }

        private void RadioButtonYesCheckedChanged(object sender, EventArgs e)
        {
            comboBoxRemindLater.Enabled = radioButtonYes.Checked;
        }
    }
}
