using AutoUpdaterDotNET;
using System;
using System.Windows.Forms;

namespace AutoUpdaterTest
{
    internal partial class RemindLaterForm : Form
    {
        public RemindLaterFormat RemindLaterFormat { get; private set; }

        public int RemindLaterAt { get; private set; }

        public RemindLaterForm()
        {
            InitializeComponent();
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
                        RemindLaterFormat = RemindLaterFormat.Minutes;
                        RemindLaterAt = 30;
                        break;
                    case 1:
                        RemindLaterFormat = RemindLaterFormat.Hours;
                        RemindLaterAt = 12;
                        break;
                    case 2:
                        RemindLaterFormat = RemindLaterFormat.Days;
                        RemindLaterAt = 1;
                        break;
                    case 3:
                        RemindLaterFormat = RemindLaterFormat.Days;
                        RemindLaterAt = 2;
                        break;
                    case 4:
                        RemindLaterFormat = RemindLaterFormat.Days;
                        RemindLaterAt = 4;
                        break;
                    case 5:
                        RemindLaterFormat = RemindLaterFormat.Days;
                        RemindLaterAt = 8;
                        break;
                    case 6:
                        RemindLaterFormat = RemindLaterFormat.Days;
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