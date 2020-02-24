using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoUpdaterTestWPF
{
    /// <summary>
    /// Interaction logic for DownloadUpdateDialog.xaml
    /// </summary>
    public partial class DownloadUpdateDialog : Window, IForm
    {
        // Fields.
        private readonly UpdateInfoEventArgs _args;
        private string _tempFile;
        private MyWebClient _webClient;
        private DateTime _startedAt;

        // Static Methods.
        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }
        private static void CompareChecksum(string fileName, CheckSum checksum)
        {
            using (var hashAlgorithm =
                HashAlgorithm.Create(
                    string.IsNullOrEmpty(checksum.HashingAlgorithm) ? "MD5" : checksum.HashingAlgorithm))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    if (hashAlgorithm != null)
                    {
                        var hash = hashAlgorithm.ComputeHash(stream);
                        var fileChecksum = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

                        if (fileChecksum == checksum.Value.ToLower()) return;

                        throw new Exception("File integrity check failed and reported some errors.");
                    }

                    throw new Exception("Hash algorithm provided in the XML file is not supported.");
                }
            }
        }

        // ctor.
        public DownloadUpdateDialog(UpdateInfoEventArgs args)
        {
            InitializeComponent();

            _args = args;

            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.ForcedDownload)
            {
                WindowStyle = WindowStyle.None;
            }
        }

        // Eventhandlers.
        private void DownloadUpdateDialogLoad(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(_args.DownloadURL);

            _webClient = AutoUpdater.GetWebClient(uri, AutoUpdater.BasicAuthDownload);

            if (string.IsNullOrEmpty(AutoUpdater.DownloadPath))
            {
                _tempFile = Path.GetTempFileName();
            }
            else
            {
                _tempFile = Path.Combine(AutoUpdater.DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");
                if (!Directory.Exists(AutoUpdater.DownloadPath))
                {
                    Directory.CreateDirectory(AutoUpdater.DownloadPath);
                }
            }

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
                    labelInformation.Content = $"Downloading at {BytesToString(bytesPerSecond)}/s";
                }
            }

            labelSize.Content = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            progressBar.Value = e.ProgressPercentage;
        }
        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (asyncCompletedEventArgs.Cancelled)
            {
                return;
            }

            try
            {
                if (asyncCompletedEventArgs.Error != null)
                {
                    throw asyncCompletedEventArgs.Error;
                }

                if (_args.CheckSum != null)
                {
                    CompareChecksum(_tempFile, _args.CheckSum);
                }

                ContentDisposition contentDisposition = null;
                if (_webClient.ResponseHeaders["Content-Disposition"] != null)
                {
                    contentDisposition = new ContentDisposition(_webClient.ResponseHeaders["Content-Disposition"]);
                }

                var fileName = string.IsNullOrEmpty(contentDisposition?.FileName)
                    ? Path.GetFileName(_webClient.ResponseUri.LocalPath)
                    : contentDisposition.FileName;

                var tempPath =
                    Path.Combine(
                        string.IsNullOrEmpty(AutoUpdater.DownloadPath) ? Path.GetTempPath() : AutoUpdater.DownloadPath,
                        fileName);

                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                File.Move(_tempFile, tempPath);

                string installerArgs = null;
                if (!string.IsNullOrEmpty(_args.InstallerArgs))
                {
                    installerArgs = _args.InstallerArgs.Replace("%path%",
                        Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Arguments = installerArgs
                };

                var extension = Path.GetExtension(tempPath);
                if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string installerPath = Path.Combine(Path.GetDirectoryName(tempPath), "ZipExtractor.exe");

                    File.WriteAllBytes(installerPath, Properties.Resources.ZipExtractor);

                    string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                    string extractionPath = Path.GetDirectoryName(executablePath);

                    if (!string.IsNullOrEmpty(AutoUpdater.InstallationPath) &&
                        Directory.Exists(AutoUpdater.InstallationPath))
                    {
                        extractionPath = AutoUpdater.InstallationPath;
                    }

                    StringBuilder arguments =
                        new StringBuilder($"\"{tempPath}\" \"{extractionPath}\" \"{executablePath}\"");
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
                        Arguments = $"/i \"{tempPath}\""
                    };
                    if (!string.IsNullOrEmpty(installerArgs))
                    {
                        processStartInfo.Arguments += " " + installerArgs;
                    }
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
                    if (exception.NativeErrorCode == 1223)
                    {
                        _webClient = null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                _webClient = null;
            }
            finally
            {
                Close();
            }
        }
        private void DownloadUpdateDialog_FormClosing(object sender, CancelEventArgs e)
        {
            if (_webClient == null)
            {
                DialogResult = false;
            }
            else if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
                DialogResult = false;
            }
            else
            {
                DialogResult = true;
            }
        }
    }
}
