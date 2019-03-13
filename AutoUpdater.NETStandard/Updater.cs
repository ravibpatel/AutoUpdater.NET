using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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
        ///     Opens the download URL in default browser if true.
        /// </summary>
        public bool OpenDownloadPage { get; set; }

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

        /// <summary>
        ///     An event that developers can use to be notified whenever the update is checked.
        /// </summary>
        public event CheckForUpdateEventHandler CheckForUpdateEvent;

        /// <summary>
        ///     An event that developers can use to be notified whenever the XML or JSON file needs parsing.
        /// </summary>
        public static event ParseUpdateInfoHandler ParseUpdateInfoEvent;

        public async void Start(string url, Assembly assembly = null)
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
                if (ParseUpdateInfoEvent == null)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfo));
                    updateInfo = (UpdateInfo) xmlSerializer.Deserialize(await content.ReadAsStreamAsync());
                }
                else
                {
                    ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(await content.ReadAsStringAsync());
                    ParseUpdateInfoEvent(parseArgs);
                    updateInfo = parseArgs.UpdateInfo;
                }
            }

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            updateInfo.ApplicationName = attributes.Length > 0 ? ((AssemblyTitleAttribute) attributes[0]).Title : assembly.GetName().Name;
            updateInfo.InstalledVersion = assembly.GetName().Version.ToString();
            updateInfo.IsUpdateAvailable = new Version(updateInfo.CurrentVersion) > assembly.GetName().Version;

            CheckForUpdateEvent.Invoke(UpdateInfo);
        }
    }
}