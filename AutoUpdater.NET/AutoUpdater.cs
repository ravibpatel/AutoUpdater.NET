using System;
using System.Net;
using System.IO;
using System.Xml;
using System.Reflection;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;

namespace AutoUpdaterDotNET
{
    public class AutoUpdater
    {

        private static String _title;

        private static String _changeLogUrl;

        private static String _downloadUrl;

        private static String _appCastUrl;

        private static String _appTitle;

        private static Version _currentVersion;

        private static Version _installedVersion;

        private static int _remindLaterAt;

        private static String _appCompany;

        private static String _registryLocation;

        private static Boolean _letUserSelectRemindLater;

        public enum RemindLaterFormat
        {
            Minutes,
            Hours,
            Days
        }

        private static RemindLaterFormat _remindLaterFormat;

        public static void Start(String appCast, bool letUserSelectRemindLater = true, int remindLaterAt = 1, RemindLaterFormat remindLaterFormat = RemindLaterFormat.Days)
        {
            _appCastUrl = appCast;
            _remindLaterAt = remindLaterAt;
            _remindLaterFormat = remindLaterFormat;
            _letUserSelectRemindLater = letUserSelectRemindLater;
            
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += BackgroundWorkerDoWork;

            backgroundWorker.RunWorkerAsync();
        }

        public static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _appTitle = Assembly.GetEntryAssembly().GetName().Name;

            Assembly currentAssembly = typeof(AutoUpdater).Assembly;
            object[] attribs = currentAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            if(attribs.Length > 0)
            {
                _appCompany = ((AssemblyCompanyAttribute)attribs[0]).Company;
            }

            if (!string.IsNullOrEmpty(_appCompany))
                _registryLocation = "Software\\" + _appCompany + "\\" + _appTitle + "\\AutoUpdater";

            RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(_registryLocation);

            if (updateKey != null)
            {
                object remindLaterTime = updateKey.GetValue("remindlater");

                if (remindLaterTime != null)
                {
                    DateTime remindLater = DateTime.Parse(remindLaterTime.ToString());

                    int compareResult = DateTime.Compare(DateTime.Now, remindLater);

                    if (compareResult < 0)
                    {
                        new UpdateForm(remindLater, _appCastUrl, _registryLocation, _remindLaterAt, _remindLaterFormat, _letUserSelectRemindLater);

                        return;
                    }
                }
            }

            _installedVersion = Assembly.GetEntryAssembly().GetName().Version;

            WebRequest webRequest = WebRequest.Create(_appCastUrl);

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
                        if (version <= _installedVersion)
                            continue;
                        _currentVersion = version;
                    }
                    else
                        continue;

                    XmlNode appCastTitle = item.SelectSingleNode("title");

                    _title = appCastTitle != null ? appCastTitle.InnerText : "";

                    XmlNode appCastChangeLog = item.SelectSingleNode("changelog");

                    _changeLogUrl = appCastChangeLog != null ? appCastChangeLog.InnerText : "";

                    XmlNode appCastUrl = item.SelectSingleNode("url");

                    _downloadUrl = appCastUrl != null ? appCastUrl.InnerText : "";
                }

            if (updateKey != null)
            {
                object skip = updateKey.GetValue("skip");
                object applicationVersion = updateKey.GetValue("version");
                if (skip != null && applicationVersion != null)
                {
                    string skipValue = skip.ToString();
                    var skipVersion = new Version(applicationVersion.ToString());
                    if (skipValue.Equals("1") && _currentVersion <= skipVersion)
                        return;
                    if (_currentVersion > skipVersion)
                    {
                        RegistryKey updateKeyWrite = Registry.CurrentUser.CreateSubKey(_registryLocation);
                        updateKeyWrite.SetValue("version", _currentVersion.ToString());
                        updateKeyWrite.SetValue("skip", 0);
                    }
                }
                updateKey.Close();
            }

            if (_currentVersion == null)
                return;

            if (_currentVersion > _installedVersion)
            {
                var thread = new Thread(ShowUi);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private static void ShowUi()
        {
            var updateForm = new UpdateForm(_title, _appCastUrl, _appTitle, _currentVersion, _installedVersion, _changeLogUrl, _downloadUrl, _registryLocation, _remindLaterAt, _remindLaterFormat, _letUserSelectRemindLater);

            updateForm.ShowDialog();
        }
    }
}
