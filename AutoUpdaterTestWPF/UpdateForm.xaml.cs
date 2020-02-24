using AutoUpdaterDotNET;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SD = System.Drawing;

namespace AutoUpdaterTestWPF
{
    /// <summary>
    /// Interaction logic for UpdateForm.xaml
    /// </summary>
    public partial class UpdateForm : Window, IForm
    {
        // Fields.
        private readonly UpdateInfoEventArgs _args;

        // Methods.
        public static int GetIEMajorVersion()
        {
            // ref:
            // 01.  http://windowsapptutorials.com/wpf/using-browser-emulation-web-browser-control-wpf-app/

            const string InternetExplorerRootKey = @"Software\Microsoft\Internet Explorer";
            int result;

            result = 0;

            try
            {
                RegistryKey key;

                key = Registry.LocalMachine.OpenSubKey(InternetExplorerRootKey);

                if (key != null)
                {
                    object value;

                    value = key.GetValue("svcVersion", null) ?? key.GetValue("Version", null);

                    if (value != null)
                    {
                        string version;
                        int separator;

                        version = value.ToString();
                        separator = version.IndexOf('.');
                        if (separator != -1)
                        {
                            int.TryParse(version.Substring(0, separator), out result);
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }
        private void UseLatestIE()
        {
            int ieValue = 0;
            switch (GetIEMajorVersion())
            {
                case 11:
                    ieValue = 11001;
                    break;
                case 10:
                    ieValue = 10001;
                    break;
                case 9:
                    ieValue = 9999;
                    break;
                case 8:
                    ieValue = 8888;
                    break;
                case 7:
                    ieValue = 7000;
                    break;
            }

            if (ieValue != 0)
            {
                try
                {
                    using (RegistryKey registryKey =
                        Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                            true))
                    {
                        registryKey?.SetValue(System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName),
                            ieValue,
                            RegistryValueKind.DWord);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        // ctor.
        public UpdateForm(UpdateInfoEventArgs args)
        {
            _args = args;
            InitializeComponent();
            UseLatestIE();
            buttonSkip.Visibility = AutoUpdater.ShowSkipButton ? Visibility.Visible : Visibility.Hidden;
            buttonRemindLater.Visibility = AutoUpdater.ShowRemindLaterButton ? Visibility.Visible : Visibility.Hidden;

            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.Forced)
            {
                WindowStyle = WindowStyle.None;
            }
        }

        #region Eventhandlers.
        private void UpdateFormLoad(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_args.ChangelogURL))
            {
                var reduceHeight = labelReleaseNotes.Height + webBrowser.Height;
                labelReleaseNotes.Visibility = Visibility.Hidden;
                webBrowser.Visibility = Visibility.Hidden;
                Height -= reduceHeight;
            }
            else
            {
                if (null != AutoUpdater.BasicAuthChangeLog)
                {
                    webBrowser.Navigate(_args.ChangelogURL, "", null,
                        $"Authorization: {AutoUpdater.BasicAuthChangeLog}");
                }
                else
                {
                    webBrowser.Navigate(_args.ChangelogURL);
                }
            }
        }
        private void ButtonUpdateClick(object sender, RoutedEventArgs e)
        {
            if (AutoUpdater.OpenDownloadPage)
            {
                var processStartInfo = new ProcessStartInfo(_args.DownloadURL);

                Process.Start(processStartInfo);

                DialogResult = true;
            }
            else
            {
                if (AutoUpdater.DownloadUpdate(_args))
                {
                    DialogResult = true;
                }
            }
        }
        private void ButtonSkipClick(object sender, RoutedEventArgs e)
        {
            AutoUpdater.PersistenceProvider.SetSkippedVersion(new Version(_args.CurrentVersion));
        }
        private void ButtonRemindLaterClick(object sender, RoutedEventArgs e)
        {
            if (AutoUpdater.LetUserSelectRemindLater)
            {
                var remindLaterForm = new RemindLaterForm();
                var dialogResult = remindLaterForm.ShowDialog();

                if (dialogResult.Equals(true))
                {
                    AutoUpdater.RemindLaterTimeSpan = remindLaterForm.RemindLaterFormat;
                    AutoUpdater.RemindLaterAt = remindLaterForm.RemindLaterAt;
                }
                else if (dialogResult.Equals(null))
                {
                    ButtonUpdateClick(sender, e);
                    return;
                }
                else
                {
                    return;
                }
            }

            AutoUpdater.PersistenceProvider.SetSkippedVersion(null);

            DateTime remindLaterDateTime = DateTime.Now;
            switch (AutoUpdater.RemindLaterTimeSpan)
            {
                case RemindLaterFormat.Days:
                    remindLaterDateTime = DateTime.Now + TimeSpan.FromDays(AutoUpdater.RemindLaterAt);
                    break;
                case RemindLaterFormat.Hours:
                    remindLaterDateTime = DateTime.Now + TimeSpan.FromHours(AutoUpdater.RemindLaterAt);
                    break;
                case RemindLaterFormat.Minutes:
                    remindLaterDateTime = DateTime.Now + TimeSpan.FromMinutes(AutoUpdater.RemindLaterAt);
                    break;
            }

            AutoUpdater.PersistenceProvider.SetRemindLater(remindLaterDateTime);
            AutoUpdater.SetTimer(remindLaterDateTime);

            DialogResult = false;
        }
        #endregion
    }
}
