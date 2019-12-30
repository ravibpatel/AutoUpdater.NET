using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace AutoUpdater.NETStandard
{
    /// <summary>
    ///     Enum representing the beahviour of the update procedure.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// In this mode, it will use all available libaray features.
        /// </summary>
        Normal,

        /// <summary>
        /// In this mode, it ignores Remind Later and Skip values set previously and hide both buttons.
        /// </summary>
        Recommended,

        /// <summary>
        /// In this mode, it won't show close button in addition to Normal mode behaviour.
        /// </summary>
        Forced,

        /// <summary>
        /// In this mode, it will start downloading and applying update without showing standarad update dialog in addition to Forced mode behaviour.
        /// </summary>
        Critical
    }

    public class UpdateInfo : EventArgs
    {
        private string _downloadUrl;
        private string _changelogUrl;
        private MyWebClient _webClient;
        private DateTime _startedAt;

        public delegate void DownloadProgressEventHandler(DownloadProgressEventArgs args);

        public event DownloadProgressEventHandler DownloadProgressChanged;
        public event AsyncCompletedEventHandler DownloadCompleted;

        /// <summary>
        ///     Reference to updater object used to start this update.
        /// </summary>
        [XmlIgnore]
        public Updater Updater { get; set; }

        /// <summary>
        ///     Application name retrieved from Assembly.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     If new update is available then returns true otherwise false.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        ///     Download URL of the update file.
        /// </summary>
        [XmlElement("download")]
        public string DownloadUrl
        {
            get => GetUrl(Updater.BaseUri, _downloadUrl);
            set => _downloadUrl = value;
        }

        /// <summary>
        ///     URL of the webpage specifying changes in the new update.
        /// </summary>
        [XmlElement("changelog")]
        public string ChangelogUrl
        {
            get => GetUrl(Updater.BaseUri, _changelogUrl);
            set => _changelogUrl = value;
        }

        /// <summary>
        ///     Returns newest version of the application available to download.
        /// </summary>
        [XmlElement("version")]
        public String CurrentVersion { get; set; }

        /// <summary>
        ///     Returns version of the application currently installed on the user's PC.
        /// </summary>
        public String InstalledVersion { get; set; }

        /// <summary>
        /// Checksum of the update file.
        /// </summary>
        [XmlElement("checksum")]
        public CheckSum CheckSum { get; set; }

        /// <summary>
        ///     Defines behaviour of the update procedure.
        /// </summary>
        [XmlElement("type")]
        public UpdateType Type { get; set; }

        /// <summary>
        ///     Command line arguments used by Installer.
        /// </summary>
        [XmlElement("args")]
        public string InstallerArgs { get; set; }

        private string GetUrl(Uri baseUri, string url)
        {
            if (!String.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                Uri uri = new Uri(baseUri, url);

                if (uri.IsAbsoluteUri)
                {
                    return uri.AbsoluteUri;
                }
            }

            return url;
        }

        public void Download(UpdateInfo updateInfo)
        {
            var uri = new Uri(updateInfo.DownloadUrl);

            using (_webClient = Updater.GetWebClient(uri, Updater.BasicAuthDownload))
            {
                string tempFile;

                if (string.IsNullOrEmpty(Updater.DownloadPath))
                {
                    tempFile = Path.GetTempFileName();
                }
                else
                {
                    tempFile = Path.Combine(Updater.DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");
                    if (!Directory.Exists(Updater.DownloadPath))
                    {
                        Directory.CreateDirectory(Updater.DownloadPath);
                    }
                }

                _webClient.DownloadProgressChanged += (sender, args) =>
                {
                    DownloadProgressEventArgs downloadProgressEventArgs = new DownloadProgressEventArgs();
                    if (_startedAt == default)
                    {
                        _startedAt = DateTime.Now;
                    }
                    else
                    {

                        var timeSpan = DateTime.Now - _startedAt;
                        long totalSeconds = (long)timeSpan.TotalSeconds;
                        if (totalSeconds > 0)
                        {
                            var bytesPerSecond = args.BytesReceived / totalSeconds;
                            downloadProgressEventArgs.Speed = $"Downloading at {BytesToString(bytesPerSecond)}/s";
                        }
                    }

                    downloadProgressEventArgs.CompletedSize = $@"{BytesToString(args.BytesReceived)} / {BytesToString(args.TotalBytesToReceive)}";
                    downloadProgressEventArgs.ProgressPercentage = args.ProgressPercentage;

                    DownloadProgressChanged?.Invoke(downloadProgressEventArgs);
                };

                _webClient.DownloadFileCompleted += (sender, args) =>
                {
                    if (args.Cancelled)
                    {
                        return;
                    }

                    try
                    {
                        if (args.Error != null)
                        {
                            throw args.Error;
                        }

                        if (updateInfo.CheckSum != null)
                        {
                            CompareChecksum(tempFile, updateInfo.CheckSum);
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
                                string.IsNullOrEmpty(Updater.DownloadPath) ? Path.GetTempPath() : Updater.DownloadPath,
                                fileName);

                        if (File.Exists(tempPath))
                        {
                            File.Delete(tempPath);
                        }

                        File.Move(tempFile, tempPath);

                        if (updateInfo.InstallerArgs != null)
                        {
                            var processModule = Process.GetCurrentProcess().MainModule;
                            updateInfo.InstallerArgs = updateInfo.InstallerArgs.Replace("%path%", processModule != null ? Path.GetDirectoryName(processModule.FileName) : Environment.CurrentDirectory);
                        }

                        DownloadCompleted?.Invoke(sender, new AsyncCompletedEventArgs(null, false, tempPath));
                    }
                    catch (Exception e)
                    {
                        DownloadCompleted?.Invoke(sender, new AsyncCompletedEventArgs(e, false, null));
                    }
                };

                _webClient.DownloadFileAsync(uri, tempFile);
            }
        }

        public void CancelDownload()
        {
            if (_webClient != null && _webClient.IsBusy)
            {
                _webClient.CancelAsync();
            }
        }

        private static void CompareChecksum(string fileName, CheckSum checkSum)
        {
            using (var hashAlgorithm = HashAlgorithm.Create(checkSum.HashingAlgorithm))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    if (hashAlgorithm != null)
                    {
                        var hash = hashAlgorithm.ComputeHash(stream);
                        var fileChecksum = BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();

                        if (fileChecksum == checkSum.Text.ToLower()) return;

                        throw new Exception("File integrity check failed and reported some errors.");
                    }

                    throw new Exception("Hash algorithm provided in the update infromation file is not supported.");
                }
            }
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }
    }

    public class CheckSum
    {
        /// <summary>
        ///     Hash of the file.
        /// </summary>
        [XmlText]
        public string Text { get; set; }

        /// <summary>
        ///     Hash algorithm that generated the hash.
        /// </summary>
        [XmlAttribute("algorithm")]
        public string HashingAlgorithm { get; set; }

    }
}
