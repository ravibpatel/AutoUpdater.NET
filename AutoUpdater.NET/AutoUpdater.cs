using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using AutoUpdaterDotNET.Properties;

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
    ///     Enum representing the effect of Mandatory flag.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// In this mode, it ignores Remind Later and Skip values set previously and hide both buttons.
        /// </summary>
        [XmlEnum("0")] Normal,

        /// <summary>
        /// In this mode, it won't show close button in addition to Normal mode behaviour.
        /// </summary>
        [XmlEnum("1")] Forced,

        /// <summary>
        /// In this mode, it will start downloading and applying update without showing standard update dialog in addition to Forced mode behaviour.
        /// </summary>
        [XmlEnum("2")] ForcedDownload
    }

    /// <summary>
    ///     Main class that lets you auto update applications by setting some static fields and executing its Start method.
    /// </summary>
    public static class AutoUpdater
    {
        private static System.Timers.Timer _remindLaterTimer;

        internal static bool IsWinFormsApplication;

        internal static Uri BaseUri;

        internal static bool Running;

        /// <summary>
        /// Set it to use custom update form specified by the type. The custom update form must 
        /// implement <see cref="IForm"/>.
        /// </summary>
        public static Type UpdateFormType;

        /// <summary>
        /// Set it to use custom download update dialog specified by the type. The custom download update dialog 
        /// must implement <see cref="IForm"/>.
        /// </summary>
        public static Type DownloadUpdateDialogType;

        /// <summary>
        ///     Set it to folder path where you want to download the update file. If not provided then it defaults to Temp folder.
        /// </summary>
        public static string DownloadPath;

        /// <summary>
        ///     If you are using a zip file as an update file then you can set this value to path where your app is installed. This is only necessary when your installation directory differs from your executable path.
        /// </summary>
        public static string InstallationPath;

        /// <summary>
        ///     Set the Application Title shown in Update dialog. Although AutoUpdater.NET will get it automatically, you can set this property if you like to give custom Title.
        /// </summary>
        public static string AppTitle;

        /// <summary>
        ///     URL of the xml file that contains information about latest version of the application.
        /// </summary>
        public static string AppCastURL;

        /// <summary>
        /// Login/password/domain for FTP-request
        /// </summary>
        public static NetworkCredential FtpCredentials;

        /// <summary>
        ///     Opens the download URL in default browser if true. Very usefull if you have portable application.
        /// </summary>
        public static bool OpenDownloadPage;

        /// <summary>
        ///     Set Basic Authentication credentials required to download the file.
        /// </summary>
        public static IAuthentication BasicAuthDownload;

        /// <summary>
        ///     Set Basic Authentication credentials required to download the XML file.
        /// </summary>
        public static IAuthentication BasicAuthXML;

        /// <summary>
        ///     Set Basic Authentication credentials to navigate to the change log URL. 
        /// </summary>
        public static IAuthentication BasicAuthChangeLog;

        /// <summary>
        ///     Set the User-Agent string to be used for HTTP web requests.
        /// </summary>
        public static string HttpUserAgent;

        /// <summary>
        ///     If this is true users can see the skip button.
        /// </summary>
        public static bool ShowSkipButton = true;

        /// <summary>
        ///     If this is true users can see the Remind Later button.
        /// </summary>
        public static bool ShowRemindLaterButton = true;

        /// <summary>
        ///     If this is true users see dialog where they can set remind later interval otherwise it will take the interval from
        ///     RemindLaterAt and RemindLaterTimeSpan fields.
        /// </summary>
        public static bool LetUserSelectRemindLater = true;

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

        /// <summary>
        ///     Set this to true if you want to run update check synchronously.
        /// </summary>
        public static bool Synchronous = false;

        ///<summary>
        ///     Set this to true if you want to ignore previously assigned Remind Later and Skip settings. It will also hide Remind Later and Skip buttons.
        /// </summary>
        public static bool Mandatory;

        /// <summary>
        ///     Set this to any of the available modes to change behaviour of the Mandatory flag.
        /// </summary>
        public static Mode UpdateMode;

        /// <summary>
        ///     Set Proxy server to use for all the web requests in AutoUpdater.NET.
        /// </summary>
        public static IWebProxy Proxy;

        /// <summary>
        /// Set this to an instance implementing the IPersistenceProvider interface for using a data storage method different from the default Windows Registry based one.
        /// </summary>
        public static IPersistenceProvider PersistenceProvider;

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
        /// <param name="args">An object containing all the parameters received from AppCast XML file. If there will be an error while looking for the XML file then this object will be null.</param>
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
        ///     Set if you want the default update form to have a different size.
        /// </summary>
        public static Size? UpdateFormSize = null;

        /// <summary>
        ///     Start checking for new version of application and display a dialog to the user if update is available.
        /// </summary>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(Assembly myAssembly = null)
        {
            Start(AppCastURL, myAssembly);
        }

        /// <summary>
        ///     Start checking for new version of application via FTP and display a dialog to the user if update is available.
        /// </summary>
        /// <param name="appCast">FTP URL of the xml file that contains information about latest version of the application.</param>
        /// <param name="ftpCredentials">Credentials required to connect to FTP server.</param>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(string appCast, NetworkCredential ftpCredentials, Assembly myAssembly = null)
        {
            FtpCredentials = ftpCredentials;
            Start(appCast, myAssembly);
        }

        /// <summary>
        ///     Start checking for new version of application and display a dialog to the user if update is available.
        /// </summary>
        /// <param name="appCast">URL of the xml file that contains information about latest version of the application.</param>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(string appCast, Assembly myAssembly = null)
        {
            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType) 192 |
                                                        (SecurityProtocolType) 768 | (SecurityProtocolType) 3072;
            }
            catch (NotSupportedException)
            {
            }

            if (Mandatory && _remindLaterTimer != null)
            {
                _remindLaterTimer.Stop();
                _remindLaterTimer.Close();
                _remindLaterTimer = null;
            }

            if (!Running && _remindLaterTimer == null)
            {
                Running = true;

                AppCastURL = appCast;

                IsWinFormsApplication = Application.MessageLoop;

                Assembly assembly = myAssembly ?? Assembly.GetEntryAssembly();

                if (Synchronous)
                {
                    try
                    {
                        var result = CheckUpdate(assembly);

                        Running = StartUpdate(result);
                    }
                    catch (Exception exception)
                    {
                        ShowError(exception);
                    }
                }
                else
                {
                    using (var backgroundWorker = new BackgroundWorker())
                    {
                        backgroundWorker.DoWork += (sender, args) =>
                        {
                            Assembly mainAssembly = args.Argument as Assembly;

                            args.Result = CheckUpdate(mainAssembly);
                        };

                        backgroundWorker.RunWorkerCompleted += (sender, args) =>
                        {
                            if (args.Error != null)
                            {
                                ShowError(args.Error);
                            }
                            else
                            {
                                if (!args.Cancelled)
                                {
                                    if (StartUpdate(args.Result))
                                    {
                                        return;
                                    }
                                }
                            }

                            Running = false;
                        };

                        backgroundWorker.RunWorkerAsync(assembly);
                    }
                }
            }
        }

        private static object CheckUpdate(Assembly mainAssembly)
        {
            var companyAttribute =
                (AssemblyCompanyAttribute) GetAttribute(mainAssembly, typeof(AssemblyCompanyAttribute));
            string appCompany = companyAttribute != null ? companyAttribute.Company : "";

            if (string.IsNullOrEmpty(AppTitle))
            {
                var titleAttribute =
                    (AssemblyTitleAttribute) GetAttribute(mainAssembly, typeof(AssemblyTitleAttribute));
                AppTitle = titleAttribute != null ? titleAttribute.Title : mainAssembly.GetName().Name;
            }

            string registryLocation = !string.IsNullOrEmpty(appCompany)
                ? $@"Software\{appCompany}\{AppTitle}\AutoUpdater"
                : $@"Software\{AppTitle}\AutoUpdater";

            if (PersistenceProvider == null)
            {
                PersistenceProvider = new RegistryPersistenceProvider(registryLocation);
            }

            BaseUri = new Uri(AppCastURL);

            UpdateInfoEventArgs args;
            using (MyWebClient client = GetWebClient(BaseUri, BasicAuthXML))
            {
                string xml = client.DownloadString(BaseUri);

                if (ParseUpdateInfoEvent == null)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfoEventArgs));
                    XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(xml)) {XmlResolver = null};
                    args = (UpdateInfoEventArgs) xmlSerializer.Deserialize(xmlTextReader);
                }
                else
                {
                    ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(xml);
                    ParseUpdateInfoEvent(parseArgs);
                    args = parseArgs.UpdateInfo;
                }
            }

            if (string.IsNullOrEmpty(args.CurrentVersion) || string.IsNullOrEmpty(args.DownloadURL))
            {
                throw new MissingFieldException();
            }

            args.InstalledVersion = mainAssembly.GetName().Version;
            args.IsUpdateAvailable = new Version(args.CurrentVersion) > mainAssembly.GetName().Version;

            if (!Mandatory)
            {
                if (string.IsNullOrEmpty(args.Mandatory.MinimumVersion) ||
                    args.InstalledVersion < new Version(args.Mandatory.MinimumVersion))
                {
                    Mandatory = args.Mandatory.Value;
                    UpdateMode = args.Mandatory.UpdateMode;
                }
            }

            if (Mandatory)
            {
                ShowRemindLaterButton = false;
                ShowSkipButton = false;
            }
            else
            {
                // Read the persisted state from the persistence provider.
                // This method makes the persistence handling independent from the storage method.
                var skippedVersion = PersistenceProvider.GetSkippedVersion();
                if (skippedVersion != null)
                {
                    var currentVersion = new Version(args.CurrentVersion);
                    if (currentVersion <= skippedVersion)
                        return null;

                    if (currentVersion > skippedVersion)
                    {
                        // Update the persisted state. Its no longer makes sense to have this flag set as we are working on a newer application version.
                        PersistenceProvider.SetSkippedVersion(null);
                    }
                }

                var remindLaterAt = PersistenceProvider.GetRemindLater();
                if (remindLaterAt != null)
                {
                    int compareResult = DateTime.Compare(DateTime.Now, remindLaterAt.Value);

                    if (compareResult < 0)
                    {
                        return remindLaterAt.Value;
                    }
                }
            }

            return args;
        }

        private static bool StartUpdate(object result)
        {
            if (result is DateTime time)
            {
                SetTimer(time);
            }
            else
            {
                if (result is UpdateInfoEventArgs args)
                {
                    if (CheckForUpdateEvent != null)
                    {
                        CheckForUpdateEvent(args);
                    }
                    else
                    {
                        if (args.IsUpdateAvailable)
                        {
                            if (!IsWinFormsApplication)
                            {
                                Application.EnableVisualStyles();
                            }

                            if (Mandatory && UpdateMode == Mode.ForcedDownload)
                            {
                                DownloadUpdate(args);
                                Exit();
                            }
                            else
                            {
                                if (Thread.CurrentThread.GetApartmentState().Equals(ApartmentState.STA))
                                {
                                    ShowUpdateForm(args);
                                }
                                else
                                {
                                    Thread thread = new Thread(new ThreadStart(delegate { ShowUpdateForm(args); }));
                                    thread.CurrentCulture =
                                        thread.CurrentUICulture = CultureInfo.CurrentCulture;
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                    thread.Join();
                                }
                            }

                            return true;
                        }

                        if (ReportErrors)
                        {
                            MessageBox.Show(Resources.UpdateUnavailableMessage,
                                Resources.UpdateUnavailableCaption,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }

            return false;
        }

        private static void ShowError(Exception exception)
        {
            if (ReportErrors)
            {
                if (exception is WebException)
                {
                    MessageBox.Show(
                        Resources.UpdateCheckFailedMessage,
                        Resources.UpdateCheckFailedCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(exception.ToString(),
                        exception.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Detects and exits all instances of running assembly, including current.
        /// </summary>
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
                    string processPath;
                    try
                    {
                        processPath = process.MainModule.FileName;
                    }
                    catch (Win32Exception)
                    {
                        // Current process should be same as processes created by other instances of the application so it should be able to access modules of other instances. 
                        // This means this is not the process we are looking for so we can safely skip this.
                        continue;
                    }

                    if (process.Id != currentProcess.Id &&
                        currentProcess.MainModule.FileName == processPath
                    ) //get all instances of assembly except current
                    {
                        if (process.CloseMainWindow())
                        {
                            process.WaitForExit((int) TimeSpan.FromSeconds(10)
                                .TotalMilliseconds); //give some time to process message
                        }

                        if (!process.HasExited)
                        {
                            process.Kill(); //TODO show UI message asking user to close program himself instead of silently killing it
                        }
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

        internal static string GetUserAgent()
        {
            return string.IsNullOrEmpty(HttpUserAgent) ? $"AutoUpdater.NET" : HttpUserAgent;
        }

        public static void SetTimer(DateTime remindLater)
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
        public static bool DownloadUpdate(UpdateInfoEventArgs args)
        {
            if (DownloadUpdateDialogType == null)
            {
                using (var downloadDialog = new DownloadUpdateDialog(args))
                {
                    try
                    {
                        return downloadDialog.ShowDialog().Equals(DialogResult.OK);
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }

                return false;
            }
            else
            {
                var form = (IForm)Activator.CreateInstance(DownloadUpdateDialogType, args);

                try
                {
                    var ret = form.ShowDialog();

                    return ret.HasValue && ret.Value == true;
                }
                catch (TargetInvocationException)
                {
                }

                return false;
            }
        }

        /// <summary>
        /// Shows standard update dialog.
        /// </summary>
        public static void ShowUpdateForm(UpdateInfoEventArgs args)
        {
            if (UpdateFormType == null)
            {
                using (var updateForm = new UpdateForm(args))
                {
                    if (UpdateFormSize.HasValue)
                    {
                        updateForm.Size = UpdateFormSize.Value;
                    }

                    if (updateForm.ShowDialog().Equals(DialogResult.OK))
                    {
                        Exit();
                    }
                }
            }
            else
            {
                var form = (IForm)Activator.CreateInstance(UpdateFormType, args);
                var ret = form.ShowDialog();

                if (ret.HasValue && 
                    ret.Value == true)
                {
                    Exit();
                }

                Running = false;
            }
        }

        /// <summary>
        /// Get the web client.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="basicAuthentication"></param>
        /// <returns></returns>
        public static MyWebClient GetWebClient(Uri uri, IAuthentication basicAuthentication)
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