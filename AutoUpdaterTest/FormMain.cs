using System;
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
            
            //AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            //If you want to open download page when user click on download button uncomment below line.
 
            //AutoUpdater.OpenDownloadPage = true;

            //Don't want user to select remind later time in AutoUpdater notification window then uncomment 3 lines below so default remind later time will be set to 2 days.

            //AutoUpdater.LetUserSelectRemindLater = false;
            //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            //AutoUpdater.RemindLaterAt = 2;

            AutoUpdater.Start("http://rbsoft.org/updates/right-click-enhancer.xml");
        }
    }
}
