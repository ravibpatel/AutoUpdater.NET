using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using AutoUpdaterTest.Properties;

namespace AutoUpdaterTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            labelVersion.Text = string.Format(Resources.CurrentVersion, Assembly.GetEntryAssembly().GetName().Version);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //Uncomment below line to see russian version

            //AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("ru");

            //If you want to open download page when user click on download button uncomment below line.

            //AutoUpdater.OpenDownloadPage = true;

            //Don't want user to select remind later time in AutoUpdater notification window then uncomment 3 lines below so default remind later time will be set to 2 days.

            //AutoUpdater.LetUserSelectRemindLater = false;
            //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            //AutoUpdater.RemindLaterAt = 1;

            //Don't want to show Skip button then uncomment below line.

            //AutoUpdater.ShowSkipButton = false;

            //Don't want to show Remind Later button then uncomment below line.

            //AutoUpdater.ShowRemindLaterButton = false;

            //Want to show custom application title then uncomment below line.

            //AutoUpdater.AppTitle = "My Custom Application Title";

            //Want to handle update logic yourself then uncomment below line.

            //AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            //AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTest.xml");
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (args.IsUpdateAvailable)
                {
                    var dialogResult =
                        MessageBox.Show(
                            $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. Do you want to update the application now?", @"Update Available",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                    if (dialogResult.Equals(DialogResult.Yes))
                    {
                        try
                        {
                            //You can use Download Update dialog used by AutoUpdater.NET to download the update.

                            AutoUpdater.DownloadUpdate();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(
                       @"There is a problem reaching update server please check your internet connection and try again later.",
                       @"Update check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCheckForUpdate_Click(object sender, EventArgs e)
        {
            AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTest.xml");
        }
    }
}
