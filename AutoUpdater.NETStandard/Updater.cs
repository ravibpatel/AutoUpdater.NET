using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace AutoUpdater.NETStandard
{
    public class Updater
    {
        /// <summary>
        ///     Defines behaviour of the update procedure.
        /// </summary>
        public UpdateType Type { get; set; }

        /// <summary>
        ///     Set it to folder path where you want to download the update file. If not provided then it defaults to Temp folder.
        /// </summary>
        public String DownloadPath { get; set; }

        /// <summary>
        ///     Set Proxy server to use for all the web requests.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        ///     Set Basic Authentication credentials required to download the file.
        /// </summary>
        public BasicAuthentication BasicAuthDownload { get; set; }

        /// <summary>
        ///     Set Basic Authentication credentials required to download the XML file.
        /// </summary>
        public BasicAuthentication BasicAuthXML { get; set; }

        private UpdateInfo UpdateInfo { get; set; }

        /// <summary>
        ///     A delegate type for hooking up update notifications.
        /// </summary>
        /// <param name="args">An object containing all the parameters recieved from XML file. If there will be an error while looking for the XML file then this object will be null.</param>
        public delegate void CheckForUpdateEventHandler(UpdateInfo args);

        /// <summary>
        ///     A delegate type for hooking up parsing logic.
        /// </summary>
        /// <param name="args">An object containing the XML or JSON file received from server.</param>
        public delegate void ParseUpdateInfoHandler(ParseUpdateInfoEventArgs args);

        public delegate void DownloadProgressEventHandler(DownloadProgressEventArgs args);

        /// <summary>
        ///     An event that developers can use to be notified whenever the update is checked.
        /// </summary>
        public event CheckForUpdateEventHandler CheckForUpdate;

        /// <summary>
        ///     An event that developers can use to be notified whenever the XML or JSON file needs parsing.
        /// </summary>
        public event ParseUpdateInfoHandler ParseUpdateInfo;

        public event DownloadProgressEventHandler DownloadProgressChanged;

        public event AsyncCompletedEventHandler DownloadCompleted;

        private MyWebClient _webClient;

        private DateTime _startedAt;

        public virtual async void Start(string url, Assembly assembly = null)
        {
            UpdateInfo updateInfo;

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }

            HttpClient client;
            if (Proxy != null)
            {
                HttpMessageHandler httpMessageHandler = new HttpClientHandler
                {
                    Proxy = Proxy
                };
                client = new HttpClient(httpMessageHandler, true);
            }
            else
            {
                client = new HttpClient();
            }

            if (BasicAuthXML != null)
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", BasicAuthXML.ToString());
            }

            using (client)
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                if (ParseUpdateInfo == null)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfo));
                    updateInfo = (UpdateInfo) xmlSerializer.Deserialize(await content.ReadAsStreamAsync());
                }
                else
                {
                    ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(await content.ReadAsStringAsync());
                    ParseUpdateInfo(parseArgs);
                    updateInfo = parseArgs.UpdateInfo;
                }
            }

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            updateInfo.ApplicationName = attributes.Length > 0 ? ((AssemblyTitleAttribute) attributes[0]).Title : assembly.GetName().Name;
            updateInfo.InstalledVersion = assembly.GetName().Version.ToString();
            updateInfo.IsUpdateAvailable = new Version(updateInfo.CurrentVersion) > assembly.GetName().Version;

            CheckForUpdate?.Invoke(updateInfo);
        }

        public void Download(UpdateInfo updateInfo)
        {
            _webClient = new MyWebClient
            {
                CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore)
            };

            if (Proxy != null)
            {
                _webClient.Proxy = Proxy;
            }

            var uri = new Uri(updateInfo.DownloadURL);

            string tempFile;

            if (string.IsNullOrEmpty(DownloadPath))
            {
                tempFile = Path.GetTempFileName();
            }
            else
            {
                tempFile = Path.Combine(DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");
                if (!Directory.Exists(DownloadPath))
                {
                    Directory.CreateDirectory(DownloadPath);
                }
            }

            if (BasicAuthDownload != null)
            {
                _webClient.Headers[HttpRequestHeader.Authorization] = BasicAuthDownload.ToString();
            }

            _webClient.DownloadProgressChanged += (sender, args) =>
            {
                DownloadProgressEventArgs downloadProgressEventArgs = new DownloadProgressEventArgs();
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
                         contentDisposition =  new ContentDisposition(_webClient.ResponseHeaders["Content-Disposition"]);
                    }

                    var fileName = string.IsNullOrEmpty(contentDisposition?.FileName)
                        ? Path.GetFileName(_webClient.ResponseUri.LocalPath)
                        : contentDisposition.FileName;

                    var tempPath =
                        Path.Combine(
                            string.IsNullOrEmpty(DownloadPath) ? Path.GetTempPath() : DownloadPath,
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
}