using System.Windows;
using AutoUpdater.NETStandard;

namespace AutoUpdater.NET.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApplicationUpdater ApplicationUpdater { get; }

        private UpdateInfo UpdateInfo { get; }

        public MainWindow(UpdateInfo updateInfo, ApplicationUpdater applicationUpdater)
        {
            UpdateInfo = updateInfo;
            ApplicationUpdater = applicationUpdater;
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            Title = $"v{UpdateInfo.CurrentVersion} available!";
            WebView.Navigate(UpdateInfo.ChangelogURL);
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            DownloadWindow downloadWindow = new DownloadWindow(UpdateInfo, ApplicationUpdater);
            downloadWindow.ShowDialog();
        }
    }
}
