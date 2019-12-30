using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AutoUpdater.NETStandard;
using MessageBox = System.Windows.MessageBox;

namespace AutoUpdater.NET.WPF
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private UpdateInfo UpdateInfo { get; }

        public DownloadWindow(UpdateInfo updateInfo)
        {
            UpdateInfo = updateInfo;
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            UpdateInfo.DownloadProgressChanged += ApplicationUpdaterOnDownloadProgressChanged;

            UpdateInfo.DownloadCompleted += ApplicationUpdaterOnDownloadCompleted;

            UpdateInfo.Download(UpdateInfo);
        }

        private void ApplicationUpdaterOnDownloadProgressChanged(DownloadProgressEventArgs args)
        {
            LabelSize.Content = args.CompletedSize;
            LabelSpeed.Content = args.Speed;
            ProgressBarDownload.Value = args.ProgressPercentage;
        }

        private void ApplicationUpdaterOnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    throw e.Error;
                }

                string tempPath = e.UserState.ToString();

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Arguments = UpdateInfo.InstallerArgs
                };

                var extension = Path.GetExtension(tempPath);

                if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
                {
                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = "msiexec",
                        Arguments = $"/i \"{tempPath}\""
                    };
                    if (!string.IsNullOrEmpty(UpdateInfo.InstallerArgs))
                    {
                        processStartInfo.Arguments += " " + UpdateInfo.InstallerArgs;
                    }
                }
                else if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (ApplicationUpdater.RunUpdateAsAdmin)
                {
                    processStartInfo.Verb = "runas";
                }

                try
                {
                    Process.Start(processStartInfo);
                    Application.Current.Shutdown();
                }
                catch (Win32Exception exception)
                {
                    if (exception.NativeErrorCode != 1223)
                        throw;
                }

                Close();
            }
            catch (Exception exception)
            {
#if DEBUG
                throw;
#endif
                MessageBox.Show(exception.Message, exception.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            UpdateInfo.CancelDownload();
        }
    }
}
