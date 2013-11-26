using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using AutoUpdaterDotNET;

namespace AutoUpdaterTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //uncomment below line to see russian version
            
            //AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("fr");

            //If you want to open download page when user click on download button uncomment below line.
 
            //AutoUpdater.OpenDownloadPage = true;

            //Don't want user to select remind later time in AutoUpdater notification window then uncomment 3 lines below so default remind later time will be set to 2 days.

            //AutoUpdater.LetUserSelectRemindLater = false;
            //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            //AutoUpdater.RemindLaterAt = 2;

            //Want to handle update logic yourself then uncomment below line.

            //AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            AutoUpdater.Start("http://rbsoft.org/updates/right-click-enhancer.xml");
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.IsUpdateAvailable)
            {
                var dialogResult =
                    MessageBox.Show(
                        string.Format(
                            "There is new version {0} avilable. You are using version {1}. Do you want to update the application now?",
                            args.CurrentVersion, args.InstalledVersion), @"Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialogResult.Equals(DialogResult.Yes))
                {
                    try
                    {
                        Process.Start("explorer.exe", args.DownloadURL);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
