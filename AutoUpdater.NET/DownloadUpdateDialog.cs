using System;
using System.ComponentModel;
using System.Net.Cache;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AutoUpdaterDotNET
{
    internal partial class DownloadUpdateDialog : Form
    {
        private readonly string _downloadURL;

        private string _tempPath;

        private WebClient _webClient;

        public DownloadUpdateDialog(string downloadURL)
        {
            InitializeComponent();

            _downloadURL = downloadURL;
        }

        private void DownloadUpdateDialogLoad(object sender, EventArgs e)
        {
            _webClient = new WebClient {CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore)};

            if (AutoUpdater.Proxy != null)
            {
                _webClient.Proxy = AutoUpdater.Proxy;
            }

            var uri = new Uri(_downloadURL);

            _tempPath = Path.Combine(Path.GetTempPath(), GetFileName(_downloadURL));

            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;

            _webClient.DownloadFileCompleted += OnDownloadComplete;

            _webClient.DownloadFileAsync(uri, _tempPath);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void OnDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, e.Error.GetType().ToString(), MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _webClient = null;
                Close();
                return;
            }

            var processStartInfo = new ProcessStartInfo {
                FileName = _tempPath,
                UseShellExecute = true,
                Arguments = AutoUpdater.InstallerArgs.Replace("%path%", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
            };
            var extension = Path.GetExtension(_tempPath);
            if (extension != null && extension.ToLower().Equals(".zip"))
            {
                string installerPath = Path.Combine(Path.GetTempPath(), "ZipExtractor.exe");
                File.WriteAllBytes(installerPath, Properties.Resources.ZipExtractor);
                processStartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,
                    Arguments = $"\"{_tempPath}\" \"{Process.GetCurrentProcess().MainModule.FileName}\""
                };
                if (AutoUpdater.RunUpdateAsAdmin)
                {
                    processStartInfo.Verb = "runas";
                }
            }

            if (!string.IsNullOrEmpty(AutoUpdater.Checksum))
            {
                if (!CompareChecksum(_tempPath, AutoUpdater.Checksum))
                {
                    _webClient = null;
                    Close();
                    return;                  
                }

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

        private static string GetFileName(string url, string httpWebRequestMethod = "HEAD")
        {
            var fileName = string.Empty;
            var uri = new Uri(url);
            try
            {
                if (uri.Scheme.Equals(Uri.UriSchemeHttp) || uri.Scheme.Equals(Uri.UriSchemeHttps))
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                    if (AutoUpdater.Proxy != null)
                    {
                        httpWebRequest.Proxy = AutoUpdater.Proxy;
                    }
                    httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    httpWebRequest.Method = httpWebRequestMethod;
                    httpWebRequest.AllowAutoRedirect = false;
                    string contentDisposition;
                    using (var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                    {
                        if (httpWebResponse.StatusCode.Equals(HttpStatusCode.Redirect) ||
                            httpWebResponse.StatusCode.Equals(HttpStatusCode.Moved) ||
                            httpWebResponse.StatusCode.Equals(HttpStatusCode.MovedPermanently))
                        {
                            if (httpWebResponse.Headers["Location"] != null)
                            {
                                var location = httpWebResponse.Headers["Location"];
                                fileName = GetFileName(location);
                                return fileName;
                            }
                        }
                        contentDisposition = httpWebResponse.Headers["content-disposition"];
                    }

                    fileName = TryToFindFileName(contentDisposition, "filename=");

                    // It can be another response: attachment; filename*=UTF-8''Setup_client_otb_1.2.88.0.msi
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = TryToFindFileName(contentDisposition, "filename*=UTF-8''");
                    }
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Path.GetFileName(uri.LocalPath);
                }
                return fileName;
            }
            catch (WebException)
            {
                if (httpWebRequestMethod.Equals("GET"))
                {
                    return Path.GetFileName(uri.LocalPath);
                }
                return GetFileName(url, "GET");
            }
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
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var hash = md5.ComputeHash(stream);
                    var fileChecksum = BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();

                    if (fileChecksum == checksum.ToLower()) return true;

                    MessageBox.Show(string.Format("Checksum Failed: {0} <> {1}", fileChecksum, checksum), @"Security Alert", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

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
}
