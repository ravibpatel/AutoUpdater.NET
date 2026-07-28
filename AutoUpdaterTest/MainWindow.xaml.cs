using System;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using AutoUpdaterDotNET;

namespace AutoUpdaterTest;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        var assembly = Assembly.GetEntryAssembly();
        LabelVersion.Content = $"Current Version: {assembly?.GetName().Version}";

        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        bool isElevated = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        LabelRunningAs.Content = $"Running: {(isElevated ? "Elevated" : "Unelevated")}";

        // Uncomment the following lines to change the current language by changing the current thread culture as shown below.
        // Thread.CurrentThread.CurrentCulture =
        //     Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en");

        MessageBox.Show(string.Join(Environment.NewLine, Environment.GetCommandLineArgs()));
    }

    private void ButtonCheckForUpdate_Click(object sender, RoutedEventArgs e)
    {
        // Uncomment the following lines to handle parsing logic of a custom JSON file.
        // AutoUpdater.ParseUpdateInfoEvent += args =>
        // {
        //     dynamic? json = JsonConvert.DeserializeObject(args.RemoteData);
        //     if (json != null)
        //     {
        //         args.UpdateInfo = new UpdateInfoEventArgs
        //         {
        //             CurrentVersion = json.version,
        //             ChangelogURL = json.changelog,
        //             DownloadURL = json.url,
        //             Mandatory = new Mandatory
        //             {
        //                 Value = json.mandatory.value,
        //                 UpdateMode = json.mandatory.mode,
        //                 MinimumVersion = json.mandatory.minVersion
        //             },
        //             CheckSum = new CheckSum
        //             {
        //                 Value = json.checksum.value,
        //                 HashingAlgorithm = json.checksum.hashingAlgorithm
        //             }
        //         };
        //     }
        // };
        // AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.json");

        // Uncomment following line to run update process without admin privileges.
        AutoUpdater.RunUpdateAsAdmin = true;

        // Uncomment the following line if you want to open the download page instead of downloading the update file when the user clicks on the download button.
        // AutoUpdater.OpenDownloadPage = true;

        // Uncomment the following lines if you don't want the user to select remind later time in the AutoUpdater notification window.
        // AutoUpdater.LetUserSelectRemindLater = false;
        // AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
        // AutoUpdater.RemindLaterAt = 2;

        // Uncomment the following line if you don't want to show the Skip button.
        // AutoUpdater.ShowSkipButton = false;

        // Uncomment the following line if you don't want to show the Remind Later button.
        // AutoUpdater.ShowRemindLaterButton = false;

        // Uncomment the following line to show the custom application title in the AutoUpdater notification window.
        // AutoUpdater.AppTitle = "My Custom Application Title";

        // Uncomment the following line if you want to show errors.
        // AutoUpdater.ReportErrors = true;

        // Uncomment the following lines if you want to handle how your application will exit when the application finishes downloading the update.
        // AutoUpdater.ApplicationExitEvent += () =>
        // {
        //     Title = @"Closing application...";
        //     Thread.Sleep(5000);
        //     Application.Current.Shutdown();
        // };

        // Uncomment the following lines to handle update logic yourself.
        // AutoUpdater.CheckForUpdateEvent += args =>
        // {
        //     switch (args.Error)
        //     {
        //         case null when args.IsUpdateAvailable:
        //         {
        //             MessageBoxResult messageBoxResult;
        //             if (args.Mandatory.Value)
        //             {
        //                 messageBoxResult =
        //                     MessageBox.Show(
        //                         $"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is a required update. Press OK to begin updating the application.",
        //                         "Update Available",
        //                         MessageBoxButton.OK,
        //                         MessageBoxImage.Information);
        //             }
        //             else
        //             {
        //                 messageBoxResult =
        //                     MessageBox.Show(
        //                         $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. Do you want to update the application now?",
        //                         @"Update Available",
        //                         MessageBoxButton.YesNo,
        //                         MessageBoxImage.Question);
        //             }
        //
        //             if (messageBoxResult.Equals(MessageBoxResult.OK) || messageBoxResult.Equals(MessageBoxResult.Yes))
        //             {
        //                 try
        //                 {
        //                     if (AutoUpdater.DownloadUpdate(args))
        //                     {
        //                         Application.Current.Shutdown();
        //                     }
        //                 }
        //                 catch (Exception exception)
        //                 {
        //                     MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButton.OK,
        //                         MessageBoxImage.Error);
        //                 }
        //             }
        //
        //             break;
        //         }
        //         case null:
        //             MessageBox.Show(@"There is no update available please try again later.", @"No update available",
        //                 MessageBoxButton.OK, MessageBoxImage.Information);
        //             break;
        //         case WebException:
        //             MessageBox.Show(
        //                 @"There is a problem reaching update server. Please check your internet connection and try again later.",
        //                 @"Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        //             break;
        //         default:
        //             MessageBox.Show(args.Error.Message,
        //                 args.Error.GetType().ToString(), MessageBoxButton.OK,
        //                 MessageBoxImage.Error);
        //             break;
        //     }
        // };

        // Uncomment the following lines if you want to use XML and an update file served only through Proxy.
        // var proxy = new WebProxy("local-proxy-IP:8080", true) {Credentials = new NetworkCredential("domain\\user", "password")};
        // AutoUpdater.Proxy = proxy;

        // Uncomment the following lines to periodically check for updates.
        // var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
        // timer.Tick += delegate { AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTest.xml"); };
        // timer.Start();

        // Uncomment the following lines to provide basic authentication credentials needed to access update resources.
        // var basicAuthentication = new BasicAuthentication("myUserName", "myPassword");
        // AutoUpdater.BasicAuthXML = AutoUpdater.BasicAuthDownload = AutoUpdater.BasicAuthChangeLog = basicAuthentication;

        // Uncomment the following lines to enable forced updates.
        // AutoUpdater.Mandatory = true;
        // AutoUpdater.UpdateMode = Mode.Forced;

        // Uncomment the following line to change AutoUpdater notification window size.
        // AutoUpdater.UpdateFormSize = new System.Drawing.Size(800, 600);

        // Uncomment the following line if you your XML file can only be accessed through FTP.
        // AutoUpdater.Start("ftp://rbsoft.org/updates/AutoUpdaterTest.xml", new NetworkCredential("FtpUserName", "FtpPassword"));

        // Uncomment the following lines if you want to persist Remind Later and Skip values in a json file instead of a registry.
        // string jsonPath = Path.Combine(Environment.CurrentDirectory, "settings.json");
        // AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);

        // Uncomment the following lines if you want to change the update zip extraction path. This only works when you use a zip file as an update file.
        // var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        // if (currentDirectory.Parent != null)
        // {
        //    AutoUpdater.InstallationPath = currentDirectory.Parent.FullName;
        // }

        // Uncomment the following line if you want to check for update synchronously.
        // AutoUpdater.Synchronous = true;

        // Uncomment the following line if you want to assign an installed version manually and don't want to rely on the library to determine the installed version from assembly.
        // AutoUpdater.InstalledVersion = new Version("2.0.0.1");

        // Uncomment the following line if you want to clear the application directory before the update zip is extracted. This only works when you use a zip file as an update file.
        // AutoUpdater.ClearAppDirectory = true;

        // Uncomment the following line if you want to set the User Agent for the web requests.
        AutoUpdater.HttpUserAgent = "AutoUpdater.NET";

        // Uncomment the following line if you want to execute a different executable after the update. This only works when you use a zip file as an update file.
        // AutoUpdater.ExecutablePath = "bin/AutoUpdaterTest.exe";

        // Uncomment following line to set this window as owner of the all dialogs initiated by AutoUpdater.
        AutoUpdater.SetOwner(this);

        // Uncomment the following line to set TopMost to true for all updater dialogs. It is necessary to do this if TopMost is set to true in your form or window.
        AutoUpdater.TopMost = true;

        // Uncomment following line to change the Icon shown on the updater dialog.
        AutoUpdater.Icon = Resource.Icon;

        AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.xml");
    }
}