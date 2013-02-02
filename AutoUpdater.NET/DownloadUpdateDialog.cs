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

            _tempPath = string.Format(@"{0}{1}", Path.GetTempPath(), GetFileName(_downloadURL));

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

        private string GetFileName(string url)
        {
            var fileName = string.Empty;

            var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
            httpWebRequest.AllowAutoRedirect = false;
            var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode.Equals(HttpStatusCode.Redirect) || httpWebResponse.StatusCode.Equals(HttpStatusCode.Moved) || httpWebResponse.StatusCode.Equals(HttpStatusCode.MovedPermanently))
            {
                if (httpWebResponse.Headers["Location"] != null)
                {
                    var location = httpWebResponse.Headers["Location"];
                    fileName = GetFileName(location);
                    return fileName;
                }
            }
            using (var webClient = new WebClient())
            {
                using (var stream = webClient.OpenRead(url))
                {
                    if (stream != null)
                    {
                        var contentDisposition = webClient.ResponseHeaders["content-disposition"];
                        if (!string.IsNullOrEmpty(contentDisposition))
                        {
                            const string lookForFileName = "filename=";
                            var index = contentDisposition.IndexOf(lookForFileName, StringComparison.CurrentCultureIgnoreCase);
                            if (index >= 0)
                                fileName = contentDisposition.Substring(index + lookForFileName.Length);
                            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                            {
                                fileName = fileName.Substring(1, fileName.Length - 2);
                            }
                        }
                        stream.Close();
                    }
                }
            }
            if (string.IsNullOrEmpty(fileName))
            {
                var uri = new Uri(url);

                fileName = Path.GetFileName(uri.LocalPath);
            }
            return fileName;
        }
    }
}
