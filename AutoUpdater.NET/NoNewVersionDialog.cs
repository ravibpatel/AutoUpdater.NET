using System;
using System.ComponentModel;
using System.Net.Cache;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace AutoUpdaterDotNET
{
    internal partial class NoNewVersionDialog : Form
    {
        public NoNewVersionDialog()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
