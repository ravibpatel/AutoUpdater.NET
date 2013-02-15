using System;
using System.Net;
using System.IO;
using System.Net.Cache;
using System.Xml;
using System.Reflection;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;

namespace AutoUpdaterDotNET
{
    public class AutoUpdater
    {
        internal static String DialogTitle;

        internal static String ChangeLogURL;

        internal static String DownloadURL;

        internal static String AppTitle;

        internal static String AppCompany;

        internal static String RegistryLocation;

        internal static Version CurrentVersion;

        internal static Version InstalledVersion;

        public static String AppCastURL;

        public static bool OpenDownloadPage = false;

        public static int RemindLaterAt = 2;

        public static Boolean LetUserSelectRemindLater = true;

        public static RemindLaterFormat RemindLaterTimeSpan = RemindLaterFormat.Days;

        public enum RemindLaterFormat
        {
            Minutes,
            Hours,
            Days
        }

        public static void Start()
        {
            Start(AppCastURL);
        }

        public static void Start(String appCast)
        {
            AppCastURL = appCast;
            
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += BackgroundWorkerDoWork;

            backgroundWorker.RunWorkerAsync();
        }

        public static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            AppTitle = Assembly.GetEntryAssembly().GetName().Name;

            Assembly currentAssembly = typeof(AutoUpdater).Assembly;
            object[] attribs = currentAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            if(attribs.Length > 0)
            {
                AppCompany = ((AssemblyCompanyAttribute)attribs[0]).Company;
            }

            if (!string.IsNullOrEmpty(AppCompany))
                RegistryLocation = string.Format(@"Software\{0}\{1}\AutoUpdater", AppCompany, AppTitle);

            RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation);

            if (updateKey != null)
            {
                object remindLaterTime = updateKey.GetValue("remindlater");

                if (remindLaterTime != null)
                {
                    DateTime remindLater = DateTime.Parse(remindLaterTime.ToString());

                    int compareResult = DateTime.Compare(DateTime.Now, remindLater);

                    if (compareResult < 0)
                    {
                        var updateForm = new UpdateForm(true);
                        updateForm.SetTimer(remindLater);
                        return;
                    }
                }
            }

            InstalledVersion = Assembly.GetEntryAssembly().GetName().Version;

            WebRequest webRequest = WebRequest.Create(AppCastURL);
            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            WebResponse webResponse;

            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (Exception)
            {
                return;
            }

            Stream appCastStream = webResponse.GetResponseStream();

            var receivedAppCastDocument = new XmlDocument();

            if (appCastStream != null) receivedAppCastDocument.Load(appCastStream);
            else return;

            XmlNodeList appCastItems = receivedAppCastDocument.SelectNodes("item");

            if (appCastItems != null)
                foreach (XmlNode item in appCastItems)
                {
                    XmlNode appCastVersion = item.SelectSingleNode("version");
                    if (appCastVersion != null)
                    {
                        String appVersion = appCastVersion.InnerText;
                        var version = new Version(appVersion);
                        if (version <= InstalledVersion)
                            continue;
                        CurrentVersion = version;
                    }
                    else
                        continue;

                    XmlNode appCastTitle = item.SelectSingleNode("title");

                    DialogTitle = appCastTitle != null ? appCastTitle.InnerText : "";

                    XmlNode appCastChangeLog = item.SelectSingleNode("changelog");

                    ChangeLogURL = appCastChangeLog != null ? appCastChangeLog.InnerText : "";

                    XmlNode appCastUrl = item.SelectSingleNode("url");

                    DownloadURL = appCastUrl != null ? appCastUrl.InnerText : "";
                }

            if (updateKey != null)
            {
                object skip = updateKey.GetValue("skip");
                object applicationVersion = updateKey.GetValue("version");
                if (skip != null && applicationVersion != null)
                {
                    string skipValue = skip.ToString();
                    var skipVersion = new Version(applicationVersion.ToString());
                    if (skipValue.Equals("1") && CurrentVersion <= skipVersion)
                        return;
                    if (CurrentVersion > skipVersion)
                    {
                        RegistryKey updateKeyWrite = Registry.CurrentUser.CreateSubKey(RegistryLocation);
                        if (updateKeyWrite != null)
                        {
                            updateKeyWrite.SetValue("version", CurrentVersion.ToString());
                            updateKeyWrite.SetValue("skip", 0);
                        }
                    }
                }
                updateKey.Close();
            }

            if (CurrentVersion == null)
                return;

            if (CurrentVersion > InstalledVersion)
            {
                var thread = new Thread(ShowUI);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private static void ShowUI()
        {
            var updateForm = new UpdateForm();

            updateForm.ShowDialog();
        }
    }
}
