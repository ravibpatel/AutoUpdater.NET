using System;
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
            AutoUpdater.LetUserSelectRemindLater = false;
            AutoUpdater.RemindLaterTimeSpan = AutoUpdater.RemindLaterFormat.Minutes;
            AutoUpdater.RemindLaterAt = 2;
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start("http://rbsoft.org/updates/right-click-enhancer.xml");
        }
    }
}
