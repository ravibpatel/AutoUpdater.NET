using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using AutoUpdaterDotNET.Properties;
using Microsoft.Win32;

namespace AutoUpdaterDotNET
{
    /// <summary>
    ///     Enum representing the remind later time span.
    /// </summary>
    public enum RemindLaterFormat
    {
        /// <summary>
        ///     Represents the time span in minutes.
        /// </summary>
        Minutes,

        /// <summary>
        ///     Represents the time span in hours.
        /// </summary>
        Hours,

        /// <summary>
        ///     Represents the time span in days.
        /// </summary>
        Days
    }

    /// <summary>
    ///     Main class that lets you auto update applications by setting some static fields and executing its Start method.
    /// </summary>
    public static class AutoUpdater
    {
        private static System.Timers.Timer _remindLaterTimer;

        internal static String ChangeLogURL;

        internal static String DownloadURL;

        internal static bool Mandatory;

        internal static String Description;
        
        internal static String RegistryLocation;

        internal static Version CurrentVersion;

        internal static Version InstalledVersion;

        internal static bool IsWinFormsApplication;

        internal static bool Running;

        /// <summary>
        ///     Set the Application Title shown in Update dialog. Although AutoUpdater.NET will get it automatically, you can set this property if you like to give custom Title.
        /// </summary>
        public static String AppTitle;

        /// <summary>
        ///     URL of the xml file that contains information about latest version of the application.
        /// </summary>
        public static String AppCastURL;

        /// <summary>
        ///     Opens the download url in default browser if true. Very usefull if you have portable application.
        /// </summary>
        public static bool OpenDownloadPage;
        
        /// <summary>
        ///     Sets the current culture of the auto update notification window. Set this value if your application supports
        ///     functionalty to change the languge of the application.
        /// </summary>
        public static CultureInfo CurrentCulture;

        /// <summary>
        ///     If this is true users can see the skip button.
        /// </summary>
        public static Boolean ShowSkipButton = true;

        /// <summary>
        ///     If this is true users can see the Remind Later button.
        /// </summary>
        public static Boolean ShowRemindLaterButton = true;

        /// <summary>
        ///     If this is true users see dialog where they can set remind later interval otherwise it will take the interval from
        ///     RemindLaterAt and RemindLaterTimeSpan fields.
        /// </summary>
        public static Boolean LetUserSelectRemindLater = true;

        /// <summary>
        ///     Remind Later interval after user should be reminded of update.
        /// </summary>
        public static int RemindLaterAt = 2;

        ///<summary>
        ///     AutoUpdater.NET will report errors if this is true.
        /// </summary>
        public static bool ReportErrors = false;

        /// <summary>
        ///     Set if RemindLaterAt interval should be in Minutes, Hours or Days.
        /// </summary>
        public static RemindLaterFormat RemindLaterTimeSpan = RemindLaterFormat.Days;

        /// <summary>
        ///     A delegate type for hooking up update notifications.
        /// </summary>
        /// <param name="args">An object containing all the parameters recieved from AppCast XML file. If there will be an error while looking for the XML file then this object will be null.</param>
        public delegate void CheckForUpdateEventHandler(UpdateInfoEventArgs args);

        /// <summary>
        ///     An event that clients can use to be notified whenever the update is checked.
        /// </summary>
        public static event CheckForUpdateEventHandler CheckForUpdateEvent;

        /// <summary>
        ///     Start checking for new version of application and display dialog to the user if update is available.
        /// </summary>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(Assembly myAssembly = null)
        {
            Start(AppCastURL, myAssembly);
        }

        /// <summary>
        ///     Start checking for new version of application and display dialog to the user if update is available.
        /// </summary>
        /// <param name="appCast">URL of the xml file that contains information about latest version of the application.</param>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(String appCast, Assembly myAssembly = null)
        {
            if (!Running && _remindLaterTimer == null)
            {
                Running = true;

                if (CurrentCulture == null)
                {
                    CurrentCulture = CultureInfo.CurrentCulture;
                }

                AppCastURL = appCast;

                IsWinFormsApplication = Application.MessageLoop;

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += BackgroundWorkerDoWork;

                backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;

                backgroundWorker.RunWorkerAsync(myAssembly ?? Assembly.GetEntryAssembly());
            }
        }

        private static void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (!runWorkerCompletedEventArgs.Cancelled)
            {
                if (runWorkerCompletedEventArgs.Result is DateTime)
                {
                    var remindLaterTime = (DateTime) runWorkerCompletedEventArgs.Result;
                    SetTimer(remindLaterTime);
                }
                else
                {
                    var args = runWorkerCompletedEventArgs.Result as UpdateInfoEventArgs;
                    if (CheckForUpdateEvent != null)
                    {
                        CheckForUpdateEvent(args);
                    }
                    else
                    {
                        if (args != null)
                        {
                            if (args.IsUpdateAvailable)
                            {
                                if (!IsWinFormsApplication)
                                {
                                    Application.EnableVisualStyles();
                                }
                                Thread thread = new Thread(new ThreadStart(delegate
                                {
                                    var updateForm = new UpdateForm();
                                    if (updateForm.ShowDialog().Equals(DialogResult.OK))
                                    {
                                        Exit();
                                    }
                                }));
                                thread.CurrentCulture = thread.CurrentUICulture = CurrentCulture;
                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start();
                                return;
                            }
                            else
                            {
                                if (ReportErrors)
                                {
                                    MessageBox.Show(Resources.UpdateUnavailableMessage, Resources.UpdateUnavailableCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        else
                        {
                            if (ReportErrors)
                            {
                                MessageBox.Show(
                                    Resources.UpdateCheckFailedMessage,
                                    Resources.UpdateCheckFailedCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            Running = false;
        }

        private static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Cancel = true;
            Assembly mainAssembly = e.Argument as Assembly;

            var companyAttribute =
                (AssemblyCompanyAttribute) GetAttribute(mainAssembly, typeof(AssemblyCompanyAttribute));
            if (string.IsNullOrEmpty(AppTitle))
            {
                var titleAttribute =
                    (AssemblyTitleAttribute) GetAttribute(mainAssembly, typeof(AssemblyTitleAttribute));
                AppTitle = titleAttribute != null ? titleAttribute.Title : mainAssembly.GetName().Name;
            }

            string appCompany = companyAttribute != null ? companyAttribute.Company : "";

            RegistryLocation = !string.IsNullOrEmpty(appCompany)
                ? $@"Software\{appCompany}\{AppTitle}\AutoUpdater"
                : $@"Software\{AppTitle}\AutoUpdater";

            InstalledVersion = mainAssembly.GetName().Version;

            var webRequest = WebRequest.Create(AppCastURL);
            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            WebResponse webResponse;

            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (Exception)
            {
                e.Cancel = false;
                return;
            }

            XmlDocument receivedAppCastDocument;

            using (Stream appCastStream = webResponse.GetResponseStream())
            {
                receivedAppCastDocument = new XmlDocument();

                if (appCastStream != null)
                {
                    receivedAppCastDocument.Load(appCastStream);
                }
                else
                {
                    e.Cancel = false;
                    webResponse.Close();
                    return;
                }
            }

            XmlNodeList appCastItems = receivedAppCastDocument.SelectNodes("item");

            if (appCastItems != null)
            {
                foreach (XmlNode item in appCastItems)
                {
                    XmlNode appCastVersion = item.SelectSingleNode("version");
                    if (appCastVersion != null)
                    {
                        String appVersion = appCastVersion.InnerText;
                        CurrentVersion = new Version(appVersion);

                        if (CurrentVersion == null)
                        {
                            webResponse.Close();
                            return;
                        }
                    }
                    else
                        continue;

                    XmlNode appCastChangeLog = item.SelectSingleNode("changelog");

                    ChangeLogURL = GetURL(webResponse.ResponseUri, appCastChangeLog);

                    XmlNode appCastUrl = item.SelectSingleNode("url");

                    DownloadURL = GetURL(webResponse.ResponseUri, appCastUrl);

                    XmlNode mandatory = item.SelectSingleNode("mandatory");

                    if (mandatory != null)
                    {
                        Mandatory = Boolean.Parse(mandatory.InnerText);
                        if (Mandatory)
                        {
                            ShowRemindLaterButton = false;
                            ShowSkipButton = false;
                        }
                    }

                    XmlNode description = item.SelectSingleNode("description");
                    if (description != null)
                    {
                        Description = description.InnerText;
                    }

                    if (IntPtr.Size.Equals(8))
                    {
                        XmlNode appCastUrl64 = item.SelectSingleNode("url64");

                        var downloadURL64 = GetURL(webResponse.ResponseUri, appCastUrl64);

                        if (!string.IsNullOrEmpty(downloadURL64))
                        {
                            DownloadURL = downloadURL64;
                        }
                    }
                }
            }

            webResponse.Close();

            if (!Mandatory)
            {
                using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation))
                {
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
                                using (RegistryKey updateKeyWrite = Registry.CurrentUser.CreateSubKey(RegistryLocation))
                                {
                                    if (updateKeyWrite != null)
                                    {
                                        updateKeyWrite.SetValue("version", CurrentVersion.ToString());
                                        updateKeyWrite.SetValue("skip", 0);
                                    }
                                }
                            }
                        }

                        object remindLaterTime = updateKey.GetValue("remindlater");

                        if (remindLaterTime != null)
                        {
                            DateTime remindLater = Convert.ToDateTime(remindLaterTime.ToString(),
                                CultureInfo.CreateSpecificCulture("en-US"));

                            int compareResult = DateTime.Compare(DateTime.Now, remindLater);

                            if (compareResult < 0)
                            {
                                e.Cancel = false;
                                e.Result = remindLater;
                                return;
                            }
                        }
                    }
                }
            }

            var args = new UpdateInfoEventArgs
            {
                DownloadURL = DownloadURL,
                ChangelogURL = ChangeLogURL,
                CurrentVersion = CurrentVersion,
                InstalledVersion = InstalledVersion,
                IsUpdateAvailable = CurrentVersion > InstalledVersion,
                Description = Description
            };

            e.Cancel = false;
            e.Result = args;
        }

        private static string GetURL(Uri baseUri, XmlNode xmlNode)
        {
            var temp = xmlNode?.InnerText ?? "";

            if (!string.IsNullOrEmpty(temp) && Uri.IsWellFormedUriString(temp, UriKind.Relative))
            {
                Uri uri = new Uri(baseUri, temp);

                if (uri.IsAbsoluteUri)
                {
                    temp = uri.AbsoluteUri;
                }
            }

            return temp;
        }

        private static void Exit()
        {
            var currentProcess = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (process.Id != currentProcess.Id)
                {
                    process.Kill();
                }
            }

            if (IsWinFormsApplication)
            {
                Application.Exit();
            }
        #if NETWPF
            else if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => System.Windows.Application.Current.Shutdown()));
            }
        #endif
            else
            {
                Environment.Exit(0);
            }
        }

        private static Attribute GetAttribute(Assembly assembly, Type attributeType)
        {
            object[] attributes = assembly.GetCustomAttributes(attributeType, false);
            if (attributes.Length == 0)
            {
                return null;
            }
            return (Attribute) attributes[0];
        }

        internal static void SetTimer(DateTime remindLater)
        {
            TimeSpan timeSpan = remindLater - DateTime.Now;
            _remindLaterTimer = new System.Timers.Timer
            {
                Interval = (int) timeSpan.TotalMilliseconds,
            };
            _remindLaterTimer.Elapsed += delegate
            {
                _remindLaterTimer.Stop();
                _remindLaterTimer = null;
                Start();
            };
            _remindLaterTimer.Start();
        }

        /// <summary>
        ///     Opens the Download window that download the update and execute the installer when download completes.
        /// </summary>
        public static bool DownloadUpdate()
        {
            var downloadDialog = new DownloadUpdateDialog(DownloadURL);

            try
            {
                return downloadDialog.ShowDialog().Equals(DialogResult.OK);
            }
            catch (TargetInvocationException)
            {
            }
            return false;
        }
    }

    /// <summary>
    ///     Object of this class gives you all the details about the update useful in handling the update logic yourself.
    /// </summary>
    public class UpdateInfoEventArgs : EventArgs
    {
        /// <summary>
        ///     If new update is available then returns true otherwise false.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        ///     Download URL of the update file.
        /// </summary>
        public string DownloadURL { get; set; }

        /// <summary>
        ///     URL of the webpage specifying changes in the new update.
        /// </summary>
        public string ChangelogURL { get; set; }

        /// <summary>
        ///     Returns newest version of the application available to download.
        /// </summary>
        public Version CurrentVersion { get; set; }

        /// <summary>
        ///     Returns version of the application currently installed on the user's PC.
        /// </summary>
        public Version InstalledVersion { get; set; }

        /// <summary>
        ///     Returns the description of the new update 
        /// </summary>
        public string Description { get; set; }
    }
}