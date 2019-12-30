using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Xml;
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
        /// Login/password/domain for FTP request.
        /// </summary>
        public NetworkCredential FtpCredentials { get; set; }

        /// <summary>
        ///     Set the User-Agent string to be used for HTTP web requests.
        /// </summary>
        public string HttpUserAgent
        {
            get => string.IsNullOrEmpty(_httpUserAgent) ? "AutoUpdater.NET" : _httpUserAgent;
            set => _httpUserAgent = value;
        }

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

        /// <summary>
        ///     An event that developers can use to be notified whenever the update is checked.
        /// </summary>
        public event CheckForUpdateEventHandler CheckForUpdate;

        /// <summary>
        ///     An event that developers can use to be notified whenever the XML or JSON file needs parsing.
        /// </summary>
        public event ParseUpdateInfoHandler ParseUpdateInfo;

        /// <summary>
        ///     Uri of the XML file.
        /// </summary>
        internal Uri BaseUri { get; set; }

        private string _httpUserAgent;

        public virtual async void Start(string url, Assembly assembly = null)
        {
            UpdateInfo updateInfo;

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }

            BaseUri = new Uri(url);

            using (MyWebClient client = GetWebClient(BaseUri, BasicAuthXML))
            {
                string content = await client.DownloadStringTaskAsync(BaseUri);
                if (ParseUpdateInfo == null)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfo));
                    XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(content)) { XmlResolver = null };
                    updateInfo = (UpdateInfo)xmlSerializer.Deserialize(xmlTextReader);
                }
                else
                {
                    ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(content);
                    ParseUpdateInfo(parseArgs);
                    updateInfo = parseArgs.UpdateInfo;
                }
            }

            updateInfo.Updater = this;
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            updateInfo.ApplicationName = attributes.Length > 0 ? ((AssemblyTitleAttribute) attributes[0]).Title : assembly.GetName().Name;
            updateInfo.InstalledVersion = assembly.GetName().Version.ToString();
            updateInfo.IsUpdateAvailable = new Version(updateInfo.CurrentVersion) > assembly.GetName().Version;

            CheckForUpdate?.Invoke(updateInfo);
        }

        internal MyWebClient GetWebClient(Uri uri, BasicAuthentication basicAuthentication)
        {
            MyWebClient webClient = new MyWebClient
            {
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            };

            if (Proxy != null)
            {
                webClient.Proxy = Proxy;
            }

            if (uri.Scheme.Equals(Uri.UriSchemeFtp))
            {
                webClient.Credentials = FtpCredentials;
            }
            else
            {
                if (basicAuthentication != null)
                {
                    webClient.Headers[HttpRequestHeader.Authorization] = basicAuthentication.ToString();
                }

                webClient.Headers[HttpRequestHeader.UserAgent] = HttpUserAgent;
            }

            return webClient;
        }
    }
}