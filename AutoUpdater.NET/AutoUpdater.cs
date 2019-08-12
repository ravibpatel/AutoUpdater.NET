using AutoUpdaterDotNET.Properties;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

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
        Normal,

        /// <summary>
        /// In this mode, it won't show close button in addition to Normal mode behaviour.
        /// </summary>
        Forced,

        /// <summary>
        /// In this mode, it will start downloading and applying update without showing standarad update dialog in addition to Forced mode behaviour.
        /// </summary>
        ForcedDownload
    }

    /// <summary>
    ///     Main class that lets you auto update applications by setting some static fields and executing its Start method.
    /// </summary>
    public static class AutoUpdater
    {
        private static System.Timers.Timer _remindLaterTimer;

        internal static string ChangelogURL;

        internal static string DownloadURL;

        internal static string InstallerArgs;

        internal static string RegistryLocation;

        internal static string Checksum;

        internal static string HashingAlgorithm;

        internal static Version CurrentVersion;

        internal static Version InstalledVersion;

        internal static bool IsWinFormsApplication;

        internal static bool Running;

        /// <summary>
        ///     Set it to folder path where you want to download the update file. If not provided then it defaults to Temp folder.
        /// </summary>
        public static string DownloadPath;

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
        public static BasicAuthentication BasicAuthDownload;

        /// <summary>
        ///     Set Basic Authentication credentials required to download the XML file.
        /// </summary>
        public static BasicAuthentication BasicAuthXML;

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
        ///     Set if you want the default update form to have a different size.
        /// </summary>
        public static Size? UpdateFormSize = null;

        /// <summary>
        ///     Start checking for new version of application and display dialog to the user if update is available.
        /// </summary>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(Assembly myAssembly = null)
        {
            Start(AppCastURL, myAssembly);
        }

        /// <summary>
        ///     Start checking for new version of application via FTP and display dialog to the user if update is available.
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
        ///     Start checking for new version of application and display dialog to the user if update is available.
        /// </summary>
        /// <param name="appCast">URL of the xml file that contains information about latest version of the application.</param>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(string appCast, Assembly myAssembly = null)
        {
            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)192 |
                                                        (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            }
            catch (NotSupportedException) { }

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

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += BackgroundWorkerDoWork;

                backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;

                backgroundWorker.RunWorkerAsync(myAssembly ?? Assembly.GetEntryAssembly());
            }
        }

        private static void BackgroundWorkerOnRunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (!runWorkerCompletedEventArgs.Cancelled)
            {
                if (runWorkerCompletedEventArgs.Result is DateTime)
                {
                    SetTimer((DateTime)runWorkerCompletedEventArgs.Result);
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

                                if (Mandatory && UpdateMode == Mode.ForcedDownload)
                                {
                                    DownloadUpdate();
                                    Exit();
                                }
                                else
                                {
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
                                        thread.Join();
                                    }
                                }

                                return;
                            }
                            else
                            {
                                if (ReportErrors)
                                {
                                    MessageBox.Show(Resources.UpdateUnavailableMessage,
                                        Resources.UpdateUnavailableCaption,
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

        /// <summary>
        /// Shows standard update dialog.
        /// <paramref name="updateOK">Download result.</paramref>
        /// </summary>
        public static void ShowUpdateForm(ref bool updateOK)
        {
            using (var updateForm = new UpdateForm())
            {
                if (UpdateFormSize.HasValue)
                {
                    updateForm.Size = UpdateFormSize.Value;
                }

                if (updateForm.ShowDialog().Equals(DialogResult.OK))
                {
                    updateOK = true;
                    Exit();
                    return;
                }
                updateOK = false;
            }
        }

        /// <summary>
        /// Shows standard update dialog.
        /// </summary>
        public static void ShowUpdateForm()
        {
            using (var updateForm = new UpdateForm())
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

        private static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Cancel = true;
            Assembly mainAssembly = e.Argument as Assembly;

            var companyAttribute =
                (AssemblyCompanyAttribute)GetAttribute(mainAssembly, typeof(AssemblyCompanyAttribute));
            if (string.IsNullOrEmpty(AppTitle))
            {
                var titleAttribute =
                    (AssemblyTitleAttribute)GetAttribute(mainAssembly, typeof(AssemblyTitleAttribute));
                AppTitle = titleAttribute != null ? titleAttribute.Title : mainAssembly.GetName().Name;
            }

            string appCompany = companyAttribute != null ? companyAttribute.Company : "";

            RegistryLocation = !string.IsNullOrEmpty(appCompany)
                ? $@"Software\{appCompany}\{AppTitle}\AutoUpdater"
                : $@"Software\{AppTitle}\AutoUpdater";

            InstalledVersion = mainAssembly.GetName().Version;

            WebRequest webRequest = WebRequest.Create(AppCastURL);

            webRequest.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }

            var uri = new Uri(AppCastURL);

            WebResponse webResponse;

            try
            {
                if (uri.Scheme.Equals(Uri.UriSchemeFtp))
                {
                    var ftpWebRequest = (FtpWebRequest)webRequest;
                    ftpWebRequest.Credentials = FtpCredentials;
                    ftpWebRequest.UseBinary = true;
                    ftpWebRequest.UsePassive = true;
                    ftpWebRequest.KeepAlive = true;
                    ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                    webResponse = ftpWebRequest.GetResponse();
                }
                else
                {
                    if (BasicAuthXML != null)
                    {
                        webRequest.Headers[HttpRequestHeader.Authorization] = BasicAuthXML.ToString();
                    }

                    webResponse = webRequest.GetResponse();
                }

            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
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

                                        bool.TryParse(mandatory?.InnerText, out Mandatory);

                                        string mode = mandatory?.Attributes["mode"]?.InnerText;

                                        if (!string.IsNullOrEmpty(mode))
                                        {
                                            UpdateMode = (Mode)Enum.Parse(typeof(Mode), mode);
                                            if (ReportErrors && !Enum.IsDefined(typeof(Mode), UpdateMode))
                                            {
                                                throw new InvalidDataException(
                                                    $"{UpdateMode} is not an underlying value of the Mode enumeration.");
                                            }
                                        }
                                    }

                                    args.Mandatory = Mandatory;
                                    args.UpdateMode = UpdateMode;

                                    XmlNode appArgs = item.SelectSingleNode("args");

                                    args.InstallerArgs = appArgs?.InnerText;

                                    XmlNode checksum = item.SelectSingleNode("checksum");

                                    args.HashingAlgorithm = checksum?.Attributes["algorithm"]?.InnerText;

                                    args.Checksum = checksum?.InnerText;
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
            InstallerArgs = args.InstallerArgs ?? string.Empty;
            HashingAlgorithm = args.HashingAlgorithm ?? "MD5";
            Checksum = args.Checksum ?? string.Empty;

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

        private static string GetURL(Uri baseUri, string url)
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
                            process.WaitForExit((int)TimeSpan.FromSeconds(10)
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

            return (Attribute)attributes[0];
        }

        internal static void SetTimer(DateTime remindLater)
        {
            TimeSpan timeSpan = remindLater - DateTime.Now;

            var context = SynchronizationContext.Current;

            _remindLaterTimer = new System.Timers.Timer
            {
                Interval = (int)timeSpan.TotalMilliseconds,
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
        /// Opens the Download window that download the update and execute the installer when download completes.
        /// <paramref name="restart">Restart after update.</paramref>
        /// </summary>
        public static bool DownloadUpdate(bool restart = true)
        {
            using (var downloadDialog = new DownloadUpdateDialog(DownloadURL, restart))
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
        ///     Defines how the Mandatory flag should work.
        /// </summary>
        public Mode UpdateMode { get; set; }

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
    ///     An object of this class contains the AppCast file received from server.
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

    /// <summary>
    ///     Provides Basic Authentication header for web request.
    /// </summary>
    public class BasicAuthentication
    {
        private string Username { get; }

        private string Password { get; }

        /// <summary>
        /// Initializes credentials for Basic Authentication.
        /// </summary>
        /// <param name="username">Username to use for Basic Authentication</param>
        /// <param name="password">Password to use for Basic Authentication</param>
        public BasicAuthentication(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}"));
            return $"Basic {token}";
        }
    }
}