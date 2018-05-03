using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using AutoUpdaterDotNET.Properties;

namespace AutoUpdaterDotNET
{
    internal partial class DownloadUpdateDialog : Form
    {
        private readonly string _downloadURL;

        private string _tempFile;

        private MyWebClient _webClient;

        private DateTime _startedAt;

        public DownloadUpdateDialog(string downloadURL)
        {
            InitializeComponent();

            _downloadURL = downloadURL;
        }

        private void DownloadUpdateDialogLoad(object sender, EventArgs e)
        {
            _webClient = new MyWebClient { CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore) };

            if (AutoUpdater.Proxy != null)
            {
                _webClient.Proxy = AutoUpdater.Proxy;
            }

            var uri = new Uri(_downloadURL);

            _tempFile = string.IsNullOrEmpty(AutoUpdater.DownloadPath) ? Path.GetTempFileName() : Path.Combine(AutoUpdater.DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");

            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;

            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;

            _webClient.DownloadFileAsync(uri, _tempFile);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_startedAt == default(DateTime))
            {
                _startedAt = DateTime.Now;
            }
            else
            {
                var timeSpan = DateTime.Now - _startedAt;
                long totalSeconds = (long)timeSpan.TotalSeconds;
                if (totalSeconds > 0)
                {
                    var bytesPerSecond = e.BytesReceived / totalSeconds;
                    labelInformation.Text = string.Format(Resources.DownloadSpeedMessage, BytesToString(bytesPerSecond));
                }
            }
            labelSize.Text = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            progressBar.Value = e.ProgressPercentage;
        }

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (asyncCompletedEventArgs.Cancelled)
            {
                return;
            }

            if (asyncCompletedEventArgs.Error != null)
            {
                MessageBox.Show(asyncCompletedEventArgs.Error.Message, asyncCompletedEventArgs.Error.GetType().ToString(), MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _webClient = null;
                Close();
                return;
            }

            if (!string.IsNullOrEmpty(AutoUpdater.Checksum))
            {
                if (!CompareChecksum(_tempFile, AutoUpdater.Checksum))
                {
                    _webClient = null;
                    Close();
                    return;
                }
            }

            string fileName;
            string contentDisposition = _webClient.ResponseHeaders["Content-Disposition"] ?? string.Empty;
            if (string.IsNullOrEmpty(contentDisposition))
            {
                fileName = Path.GetFileName(_webClient.ResponseUri.LocalPath);
            }
            else
            {
                fileName = TryToFindFileName(contentDisposition, "filename=");
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = TryToFindFileName(contentDisposition, "filename*=UTF-8''");
                }
            }
            var tempPath = Path.Combine(string.IsNullOrEmpty(AutoUpdater.DownloadPath) ? Path.GetTempPath() : AutoUpdater.DownloadPath, fileName);

            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                File.Move(_tempFile, tempPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                _webClient = null;
                Close();
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Arguments = AutoUpdater.InstallerArgs.Replace("%path%", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
            };

            var extension = Path.GetExtension(tempPath);
            if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string installerPath = Path.Combine(Path.GetDirectoryName(tempPath), "ZipExtractor.exe");
                File.WriteAllBytes(installerPath, Resources.ZipExtractor);
                StringBuilder arguments = new StringBuilder($"\"{tempPath}\" \"{Process.GetCurrentProcess().MainModule.FileName}\"");
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 1; i < args.Length; i++)
                {
                    if (i.Equals(1))
                    {
                        arguments.Append(" \"");
                    }
                    arguments.Append(args[i]);
                    arguments.Append(i.Equals(args.Length - 1) ? "\"" : " ");
                }
                processStartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,
                    Arguments = arguments.ToString()
                };
            }
            else if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
            {
                processStartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = "/i " + tempPath
                };
            }

            if (AutoUpdater.RunUpdateAsAdmin)
            {
                processStartInfo.Verb = "runas";
            }

            try
            {
                Process.Start(processStartInfo);
            }
            catch (Win32Exception exception)
            {
                _webClient = null;
                if (exception.NativeErrorCode != 1223)
                    throw;
            }

            Close();
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }

        private static string TryToFindFileName(string contentDisposition, string lookForFileName)
        {
            string fileName = String.Empty;
            if (!string.IsNullOrEmpty(contentDisposition))
            {
                var index = contentDisposition.IndexOf(lookForFileName, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                    fileName = contentDisposition.Substring(index + lookForFileName.Length);
                if (fileName.StartsWith("\""))
                {
                    var file = fileName.Substring(1, fileName.Length - 1);
                    var i = file.IndexOf("\"", StringComparison.CurrentCultureIgnoreCase);
                    if (i != -1)
                    {
                        fileName = file.Substring(0, i);
                    }
                }
            }
            return fileName;
        }

        private static bool CompareChecksum(string fileName, string checksum)
        {
            using (var hashAlgorithm = HashAlgorithm.Create(AutoUpdater.HashingAlgorithm))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    if (hashAlgorithm != null)
                    {
                        var hash = hashAlgorithm.ComputeHash(stream);
                        var fileChecksum = BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();

                        if (fileChecksum == checksum.ToLower()) return true;

                        MessageBox.Show(Resources.FileIntegrityCheckFailedMessage, Resources.FileIntegrityCheckFailedCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (AutoUpdater.ReportErrors)
                        {
                            MessageBox.Show(Resources.HashAlgorithmNotSupportedMessage, Resources.HashAlgorithmNotSupportedCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    return false;
                }
            }
        }

        private void DownloadUpdateDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_webClient == null)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }

    /// <inheritdoc />
    public class MyWebClient : WebClient
    {
        /// <summary>
        ///     Response Uri after any redirects.
        /// </summary>
        public Uri ResponseUri;

        /// <inheritdoc />
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse webResponse = base.GetWebResponse(request, result);
            ResponseUri = webResponse.ResponseUri;
            return webResponse;
        }
    }
}
