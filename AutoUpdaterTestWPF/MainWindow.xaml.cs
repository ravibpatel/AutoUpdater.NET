using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
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
            //Assembly assembly = Assembly.GetEntryAssembly();
            //LabelVersion.Content = $"Current Version : {assembly.GetName().Version}";
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("zh");
            AutoUpdater.AppTitle = "自定义标题";
            AutoUpdater.InstalledVersion = Version.Parse("1.0.0.0");
            AutoUpdater.LetUserSelectRemindLater = true;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.RemindLaterAt = 1;
            AutoUpdater.ReportErrors = true;
            //DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            //timer.Tick += delegate
            //{
            //    AutoUpdater.Start("http://localhost:8080/updates/AutoUpdaterTestWPF.xml");
            //};
            //timer.Start();
        }

        private void ButtonCheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdater.Start("http://localhost:8080/updates/AutoUpdaterTestWPF.xml");
        }
    }
}
