using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoUpdaterTestWPF
{
    /// <summary>
    /// Interaction logic for RemindLaterForm.xaml
    /// </summary>
    public partial class RemindLaterForm : Window
    {
        // Properties.
        public RemindLaterFormat RemindLaterFormat 
        { 
            get; 
            private set; 
        }
        public int RemindLaterAt 
        { 
            get; 
            private set; 
        }

        // ctor.
        public RemindLaterForm()
        {
            InitializeComponent();

            comboBoxRemindLater.Items.Add("30 minutes");
            comboBoxRemindLater.Items.Add("12 hours");
            comboBoxRemindLater.Items.Add("1 days");
            comboBoxRemindLater.Items.Add("2 days");
            comboBoxRemindLater.Items.Add("4 days");
            comboBoxRemindLater.Items.Add("8 days");
            comboBoxRemindLater.Items.Add("10 days");
        }

        // Eventhandlers.
        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if (radioButtonYes.IsChecked.GetValueOrDefault())
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

                DialogResult = true;
            }
            else
            {
                DialogResult = null;
            }
        }
    }
}
