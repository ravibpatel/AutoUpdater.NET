using AutoUpdaterDotNET;
using System;
using System.Windows.Forms;

namespace ZipExtractorForm
{
    public partial class FormMain : Form
    {
        ZipExtractor z;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Shown(object sender, EventArgs e){
            z.UnzipFile();
            labelInformation.Text = "Finished Unzipping files.";
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e){
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            z = new ZipExtractor();
        }
    }
}
