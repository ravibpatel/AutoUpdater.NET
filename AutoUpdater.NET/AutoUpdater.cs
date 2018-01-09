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

        internal static String ChangelogURL;

        internal static String DownloadURL;

        internal static String InstallerArgs;

        internal static String RegistryLocation;

        internal static String Checksum;

        internal static String HashingAlgorithm;

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
        ///     Set this to false if your application doesn't need administrator privileges to replace the old version.
        /// </summary>
        public static bool RunUpdateAsAdmin = true;

        ///<summary>
        ///     Set this to true if you want to ignore previously assigned Remind Later and Skip settings. It will also hide Remind Later and Skip buttons.
        /// </summary>
        public static bool Mandatory;

        /// <summary>
        ///     Set Proxy server to use for all the web requests in AutoUpdater.NET.
        /// </summary>
        public static WebProxy Proxy;

        /// <summary>
        ///     Set if RemindLaterAt interval should be in Minutes, Hours or Days.
        /// </summary>
        public static RemindLaterFormat RemindLaterTimeSpan = RemindLaterFormat.Days;

        /// <summary>
        ///     A delegate type to handle how to exit the application after update is downloaded.
        /// </summary>
        public delegate void ApplicationExitEventHandler();

        /// <summary>
        ///     An event that developers can use to exit the application gracefully.
        /// </summary>
        public static event ApplicationExitEventHandler ApplicationExitEvent;

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
        ///     A delegate type for hooking up parsing logic.
        /// </summary>
        /// <param name="args">An object containing the AppCast file received from server.</param>
        public delegate void ParseUpdateInfoHandler(ParseUpdateInfoEventArgs args);

        /// <summary>
        ///     An event that clients can use to be notified whenever the AppCast file needs parsing.
        /// </summary>
        public static event ParseUpdateInfoHandler ParseUpdateInfoEvent;

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
                if (runWorkerCompletedEventArgs.Result is DateTime remindLaterTime)
                {
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
                                if (Thread.CurrentThread.GetApartmentState().Equals(ApartmentState.STA))
                                {
                                    ShowUpdateForm();
                                }
                                else
                                {
                                    Thread thread = new Thread(ShowUpdateForm);
                                    thread.CurrentCulture = thread.CurrentUICulture = CultureInfo.CurrentCulture;
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                }
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

        private static void ShowUpdateForm()
        {
            var updateForm = new UpdateForm();
            if (updateForm.ShowDialog().Equals(DialogResult.OK))
            {
                Exit();
            }
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
            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }
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
            UpdateInfoEventArgs args;
            using (Stream appCastStream = webResponse.GetResponseStream())
            {
                if (appCastStream != null)
                {
                    if (ParseUpdateInfoEvent != null)
                    {
                        using (StreamReader streamReader = new StreamReader(appCastStream))
                        {
                            string data = streamReader.ReadToEnd();
                            ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(data);
                            ParseUpdateInfoEvent(parseArgs);
                            args = parseArgs.UpdateInfo;
                        }
                    }
                    else
                    {
                        XmlDocument receivedAppCastDocument = new XmlDocument();

                        try
                        {
                            receivedAppCastDocument.Load(appCastStream);

                            XmlNodeList appCastItems = receivedAppCastDocument.SelectNodes("item");
                            
                            args = new UpdateInfoEventArgs();

                            if (appCastItems != null)
                            {
                                foreach (XmlNode item in appCastItems)
                                {
                                    XmlNode appCastVersion = item.SelectSingleNode("version");

                                    try
                                    {
                                        CurrentVersion = new Version(appCastVersion?.InnerText);
                                    }
                                    catch (Exception)
                                    {
                                        CurrentVersion = null;
                                    }

                                    args.CurrentVersion = CurrentVersion;

                                    XmlNode appCastChangeLog = item.SelectSingleNode("changelog");

                                    args.ChangelogURL = appCastChangeLog?.InnerText;

                                    XmlNode appCastUrl = item.SelectSingleNode("url");

                                    args.DownloadURL = appCastUrl?.InnerText;

                                    if (Mandatory.Equals(false))
                                    {
                                        XmlNode mandatory = item.SelectSingleNode("mandatory");

                                        Boolean.TryParse(mandatory?.InnerText, out Mandatory);
                                    }

                                    args.Mandatory = Mandatory;

                                    XmlNode appArgs = item.SelectSingleNode("args");

                                    args.InstallerArgs = appArgs?.InnerText ?? String.Empty;

                                    XmlNode checksum = item.SelectSingleNode("checksum");

                                    args.HashingAlgorithm = checksum?.Attributes["algorithm"]?.InnerText ?? "MD5";

                                    args.Checksum = checksum?.InnerText ?? String.Empty;
                                }
                            }
                        }
                        catch (XmlException)
                        {
                            e.Cancel = false;
                            webResponse.Close();
                            return;
                        }
                    }
                }
                else
                {
                    e.Cancel = false;
                    webResponse.Close();
                    return;
                }
            }

            if (args.CurrentVersion == null || string.IsNullOrEmpty(args.DownloadURL))
            {
                webResponse.Close();
                if (ReportErrors)
                {
                    throw new InvalidDataException();
                }
                return;
            }

            CurrentVersion = args.CurrentVersion;
            ChangelogURL = args.ChangelogURL = GetURL(webResponse.ResponseUri, args.ChangelogURL);
            DownloadURL = args.DownloadURL = GetURL(webResponse.ResponseUri, args.DownloadURL);
            Mandatory = args.Mandatory;
            InstallerArgs = args.InstallerArgs;
            HashingAlgorithm = args.HashingAlgorithm;
            Checksum = args.Checksum;

            webResponse.Close();

            if (Mandatory)
            {
                ShowRemindLaterButton = false;
                ShowSkipButton = false;
            }
            else
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
                                CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);

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

            args.IsUpdateAvailable = CurrentVersion > InstalledVersion;
            args.InstalledVersion = InstalledVersion;

            e.Cancel = false;
            e.Result = args;
        }

        private static string GetURL(Uri baseUri, String url)
        {
            if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                Uri uri = new Uri(baseUri, url);

                if (uri.IsAbsoluteUri)
                {
                    url = uri.AbsoluteUri;
                }
            }

            return url;
        }

        private static void Exit()
        {
            if (ApplicationExitEvent != null)
            {
                ApplicationExitEvent();
            }
            else
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
                    MethodInvoker methodInvoker = Application.Exit;
                    methodInvoker.Invoke();
                }
#if NETWPF
                else if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        System.Windows.Application.Current.Shutdown()));
                }
#endif
                else
                {
                    Environment.Exit(0);
                }
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

            var context = SynchronizationContext.Current;

            _remindLaterTimer = new System.Timers.Timer
            {
                Interval = (int) timeSpan.TotalMilliseconds,
                AutoReset = false
            };

            _remindLaterTimer.Elapsed += delegate
            {
                _remindLaterTimer = null;
                if (context != null)
                {
                    try
                    {
                        context.Send(state => Start(), null);
                    }
                    catch (InvalidAsynchronousStateException)
                    {
                        Start();
                    }
                }
                else
                {
                    Start();
                }
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
        ///     Shows if the update is required or optional.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        ///     Command line arguments used by Installer.
        /// </summary>
        public string InstallerArgs { get; set; }

        /// <summary>
        ///     Checksum of the update file.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        ///     Hash algorithm that generated the checksum provided in the XML file.
        /// </summary>
        public string HashingAlgorithm { get; set; }
    }

    /// <summary>
    ///     An object of this class contains the AppCast file received from server..
    /// </summary>
    public class ParseUpdateInfoEventArgs : EventArgs
    {
        /// <summary>
        ///     Remote data received from the AppCast file.
        /// </summary>
        public string RemoteData { get; }

        /// <summary>
        ///      Set this object with values received from the AppCast file.
        /// </summary>
        public UpdateInfoEventArgs UpdateInfo { get; set; }

        /// <summary>
        ///     An object containing the AppCast file received from server.
        /// </summary>
        /// <param name="remoteData">A string containing remote data received from the AppCast file.</param>
        public ParseUpdateInfoEventArgs(string remoteData)
        {
            RemoteData = remoteData;
        }
    }
}