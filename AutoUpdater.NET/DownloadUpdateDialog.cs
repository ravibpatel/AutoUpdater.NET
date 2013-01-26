using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace AutoUpdaterDotNET
{
    public partial class DownloadUpdateDialog : Form
    {
        private readonly string _downloadUrl;

        private string _tempPath;

        public DownloadUpdateDialog(string downloadUrl)
        {
            InitializeComponent();

            _downloadUrl = downloadUrl;
        }

        private void DownloadUpdateDialogLoad(object sender, EventArgs e)
        {
            var webClient = new WebClient();

            var uri = new Uri(_downloadUrl);
            string filename = Path.GetFileName(uri.LocalPath);

            _tempPath = Path.GetTempPath() + "\\" + filename;

            webClient.DownloadProgressChanged += OnDownloadProgressChanged;

            webClient.DownloadFileCompleted += OnDownloadComplete;

            webClient.DownloadFileAsync(uri, _tempPath);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void OnDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            var processStartInfo = new ProcessStartInfo {FileName = _tempPath, UseShellExecute = true};
            Process.Start(processStartInfo);
            Application.Exit();
        }
    }
}
