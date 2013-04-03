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
            Application.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            AutoUpdater.CurrentCulture = Application.CurrentCulture;
            AutoUpdater.Start("http://rbsoft.org/updates/right-click-enhancer.xml");
        }
    }
}
