using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AutoUpdaterDotNET
{
    public partial class UpdateForm : Form
    {
        private readonly string _url;

        private readonly string _downloadUrl;

        private readonly Version _currentVersion;

        private int _remindLaterAt;

        private System.Timers.Timer _timer;

        private readonly String _appCast;

        private readonly String _registryLocation;

        private AutoUpdater.RemindLaterFormat _remindLaterFormat;

        private readonly bool _letUserSelectRemindLater;

        private readonly string _appTitle;

        public UpdateForm(String title, String appCast, String appTitle, Version currentVersion, Version installedVersion, String url, String downloadUrl, String registryLocation, int remindLaterAt, AutoUpdater.RemindLaterFormat remindLaterFormat, bool letUserSelectRemindLater)
        {
            InitializeComponent();
            Text = title;
            _url = url;
            _appTitle = appTitle;
            _downloadUrl = downloadUrl;
            _currentVersion = currentVersion;
            _remindLaterAt = remindLaterAt;
            _remindLaterFormat = remindLaterFormat;
            _appCast = appCast;
            _registryLocation = registryLocation;
            _letUserSelectRemindLater = letUserSelectRemindLater;
            labelUpdate.Text = string.Format("A new version of {0} is available!", appTitle);
            labelDescription.Text = string.Format("{0} {1} is now available. You have version {2} installed. Would you like to download it now?", appTitle, currentVersion, installedVersion);
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public UpdateForm(DateTime remindLater, String appCast, String registryLocation, int remindLaterAt, AutoUpdater.RemindLaterFormat remindLaterFormat, bool letUserSelectRemindLater)
        {
            SetTimer(remindLater);
            _appCast = appCast;
            _remindLaterAt = remindLaterAt;
            _remindLaterFormat = remindLaterFormat;
            _registryLocation = registryLocation;
            _letUserSelectRemindLater = letUserSelectRemindLater;
        }

        private void UpdateFormLoad(object sender, EventArgs e)
        {
            webBrowser.Navigate(_url);
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            var downloadDialog = new DownloadUpdateDialog(_downloadUrl);

            try
            {
                downloadDialog.ShowDialog();
            }
            catch (System.Reflection.TargetInvocationException)
            {
            }
        }

        private void ButtonSkipClick(object sender, EventArgs e)
        {
            RegistryKey updateKey = Registry.CurrentUser.CreateSubKey(_registryLocation);
            updateKey.SetValue("version", _currentVersion.ToString());
            updateKey.SetValue("skip", 1);
            updateKey.Close();
        }

        private void ButtonRemindLaterClick(object sender, EventArgs e)
        {
            if(_letUserSelectRemindLater)
            {
                var remindLaterForm = new RemindLaterForm(_appTitle);

                var dialogResult = remindLaterForm.ShowDialog();

                if(dialogResult.Equals(DialogResult.OK))
                {
                    _remindLaterFormat = remindLaterForm.RemindLaterFormat;
                    _remindLaterAt = remindLaterForm.RemindLaterAt;
                }
                else if(dialogResult.Equals(DialogResult.Abort))
                {
                    var downloadDialog = new DownloadUpdateDialog(_downloadUrl);

                    try
                    {
                        downloadDialog.ShowDialog();
                    }
                    catch (System.Reflection.TargetInvocationException)
                    {
                        return;
                    }
                    return;
                }
                else
                {
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            RegistryKey updateKey = Registry.CurrentUser.CreateSubKey(_registryLocation);
            updateKey.SetValue("version", _currentVersion);
            updateKey.SetValue("skip", 0);
            switch (_remindLaterFormat)
            {
                case AutoUpdater.RemindLaterFormat.Days :
                    updateKey.SetValue("remindlater", DateTime.Now + TimeSpan.FromDays(_remindLaterAt));
                    SetTimer(DateTime.Now + TimeSpan.FromDays(_remindLaterAt));
                    break;
                case AutoUpdater.RemindLaterFormat.Hours:
                    updateKey.SetValue("remindlater", DateTime.Now + TimeSpan.FromHours(_remindLaterAt));
                    SetTimer(DateTime.Now + TimeSpan.FromHours(_remindLaterAt));
                    break;
                case AutoUpdater.RemindLaterFormat.Minutes:
                    updateKey.SetValue("remindlater", DateTime.Now + TimeSpan.FromMinutes(_remindLaterAt));
                    SetTimer(DateTime.Now + TimeSpan.FromMinutes(_remindLaterAt));
                    break;
            }
            updateKey.Close();
        }

        private void SetTimer(DateTime remindLater)
        {
            _timer = new System.Timers.Timer();
            TimeSpan timeSpan = remindLater - DateTime.Now;
            _timer.Interval = (int) timeSpan.TotalMilliseconds;
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            AutoUpdater.Start(_appCast, _letUserSelectRemindLater,_remindLaterAt, _remindLaterFormat);
        }
    }
}
