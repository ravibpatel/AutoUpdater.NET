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
        private readonly string _downloadURL;

        private string _tempPath;

        public DownloadUpdateDialog(string downloadURL)
        {
            InitializeComponent();

            _downloadURL = downloadURL;
        }

        private void DownloadUpdateDialogLoad(object sender, EventArgs e)
        {
            var webClient = new WebClient();

            var uri = new Uri(_downloadURL);
            string filename = Path.GetFileName(uri.LocalPath);

            _tempPath = string.Format(@"{0}\{1}", Path.GetTempPath(), filename);

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
