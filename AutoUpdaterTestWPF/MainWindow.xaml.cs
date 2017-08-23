using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;

namespace AutoUpdaterTestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetEntryAssembly();
            LabelVersion.Content = $"Current Version : {assembly.GetName().Version}";
            AutoUpdater.CurrentCulture = new CultureInfo("fr-FR");
            AutoUpdater.LetUserSelectRemindLater = true;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            AutoUpdater.RemindLaterAt = 1;
            AutoUpdater.ReportErrors = true;
            System.Timers.Timer timer = new System.Timers.Timer {Interval = 2 * 60 * 1000};
            timer.Elapsed += delegate
            {
                AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTestWPF.xml");
            };
            timer.Start();
        }

        private void ButtonCheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTestWPF.xml");
        }
    }
}
