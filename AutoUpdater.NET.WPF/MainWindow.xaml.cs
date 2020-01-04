using System.Windows;
using AutoUpdater.NETStandard;

namespace AutoUpdater.NET.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UpdateInfo UpdateInfo { get; }

        public MainWindow(UpdateInfo updateInfo)
        {
            UpdateInfo = updateInfo;
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            Title = $"v{UpdateInfo.CurrentVersion} available!";
            WebView.Navigate(UpdateInfo.ChangelogUrl);
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            DownloadWindow downloadWindow = new DownloadWindow(UpdateInfo);
            downloadWindow.ShowDialog();
        }

        private void ButtonSkip_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonRemindLater_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
