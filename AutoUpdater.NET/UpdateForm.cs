﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace AutoUpdaterDotNET
{
    internal partial class UpdateForm : Form
    {
        private readonly UpdateInfoEventArgs _args;

        public UpdateForm(UpdateInfoEventArgs args)
        {
            _args = args;
            InitializeComponent();
            InitializeBrowserControl();
            UseLatestIE();
            buttonSkip.Visible = AutoUpdater.ShowSkipButton;
            buttonRemindLater.Visible = AutoUpdater.ShowRemindLaterButton;
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            Text = string.Format(resources.GetString("$this.Text", CultureInfo.CurrentCulture),
                AutoUpdater.AppTitle, _args.CurrentVersion);
            labelUpdate.Text = string.Format(resources.GetString("labelUpdate.Text", CultureInfo.CurrentCulture),
                AutoUpdater.AppTitle);
            labelDescription.Text =
                string.Format(resources.GetString("labelDescription.Text", CultureInfo.CurrentCulture),
                    AutoUpdater.AppTitle, _args.CurrentVersion, _args.InstalledVersion);

            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.Forced)
            {
                ControlBox = false;
            }
        }

        private void InitializeBrowserControl()
        {
            if (string.IsNullOrEmpty(_args.ChangelogURL))
            {
                var reduceHeight = labelReleaseNotes.Height + webBrowser.Height;
                labelReleaseNotes.Hide();
                webBrowser.Hide();
                webView2.Hide();
                Height -= reduceHeight;
            }
            else
            {
                if (string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString()))
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
                else
                {
                    webBrowser.Hide();
                    webView2.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                    webView2.EnsureCoreWebView2Async();
                }
            }
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView2.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView2.CoreWebView2.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            webView2.Show();
            webView2.BringToFront();
            if (null != AutoUpdater.BasicAuthChangeLog)
            {
                var resourceRequest = webView2.CoreWebView2.Environment.CreateWebResourceRequest(_args.ChangelogURL, "GET", Stream.Null, $"Authorization: {AutoUpdater.BasicAuthChangeLog}");
                webView2.CoreWebView2.NavigateWithWebResourceRequest(resourceRequest);
            }
            else
            {
                webView2.CoreWebView2.Navigate(_args.ChangelogURL);
            }
        }

        private void UseLatestIE()
        {
            int ieValue = 0;
            switch (webBrowser.Version.Major)
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
                        registryKey?.SetValue(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName),
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

        private void UpdateFormLoad(object sender, EventArgs e)
        {
            var labelSize = new Size(webBrowser.Width, 0);
            labelDescription.MaximumSize = labelUpdate.MaximumSize = labelSize;
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            if (AutoUpdater.OpenDownloadPage)
            {
                var processStartInfo = new ProcessStartInfo(_args.DownloadURL);

                Process.Start(processStartInfo);

                DialogResult = DialogResult.OK;
            }
            else
            {
                if (AutoUpdater.DownloadUpdate(_args))
                {
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void ButtonSkipClick(object sender, EventArgs e)
        {
            AutoUpdater.PersistenceProvider.SetSkippedVersion(new Version(_args.CurrentVersion));
        }

        private void ButtonRemindLaterClick(object sender, EventArgs e)
        {
            if (AutoUpdater.LetUserSelectRemindLater)
            {
                using (var remindLaterForm = new RemindLaterForm())
                {
                    var dialogResult = remindLaterForm.ShowDialog();

                    if (dialogResult.Equals(DialogResult.OK))
                    {
                        AutoUpdater.RemindLaterTimeSpan = remindLaterForm.RemindLaterFormat;
                        AutoUpdater.RemindLaterAt = remindLaterForm.RemindLaterAt;
                    }
                    else if (dialogResult.Equals(DialogResult.Abort))
                    {
                        ButtonUpdateClick(sender, e);
                        return;
                    }
                    else
                    {
                        return;
                    }
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

            DialogResult = DialogResult.Cancel;
        }

        private void UpdateForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            AutoUpdater.Running = false;
        }

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.Forced)
            {
                AutoUpdater.Exit();
            }
        }
    }
}