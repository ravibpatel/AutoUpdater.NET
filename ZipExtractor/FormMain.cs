using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor
{
    public partial class FormMain : Form
    {
        private BackgroundWorker _backgroundWorker;

        public FormMain()
        {
            InitializeComponent();
            UseSystemFont(this);
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length.Equals(3))
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.MainModule.FileName.Equals(args[2]))
                        {
                            labelInformation.Text = @"Waiting for application to Exit...";
                            process.WaitForExit();
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
                    var path = Path.GetDirectoryName(args[2]);

                    // Open an existing zip file for reading.
                    ZipStorer zip = ZipStorer.Open(args[1], FileAccess.Read);

                    // Read the central directory collection.
                    List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                    for (var index = 0; index < dir.Count; index++)
                    {
                        if (_backgroundWorker.CancellationPending)
                        {
                            eventArgs.Cancel = true;
                            zip.Close();
                            return;
                        }
                        ZipStorer.ZipFileEntry entry = dir[index];
                        zip.ExtractFile(entry, Path.Combine(path, entry.FilenameInZip));
                        _backgroundWorker.ReportProgress((index + 1) * 100 / dir.Count, string.Format(Resources.CurrentFileExtracting, entry.FilenameInZip));
                    }

                    zip.Close();
                };

                _backgroundWorker.ProgressChanged += (o, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    labelInformation.Text = eventArgs.UserState.ToString();
                };

                _backgroundWorker.RunWorkerCompleted += (o, eventArgs) =>
                {
                    if (!eventArgs.Cancelled)
                    {
                        labelInformation.Text = @"Finished";
                        try
                        {
                            Process.Start(args[2]);
                        }
                        catch (Win32Exception exception)
                        {
                            if (exception.NativeErrorCode != 1223)
                                throw;
                        }
                        Application.Exit();
                    }
                };
                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker.CancelAsync();
        }

        private static void UseSystemFont(Form form)
        {
            form.Font = SystemFonts.MessageBoxFont;
            SetFont(form.Controls);
        }

        private static void SetFont(IEnumerable controls)
        {
            foreach (Control control in controls)
            {
                var font = new Font(SystemFonts.MessageBoxFont.FontFamily, control.Font.SizeInPoints, control.Font.Style,
                    GraphicsUnit.Point);
                control.Font = font;
                if (control.HasChildren)
                {
                    SetFont(control.Controls);
                }
            }
        }
    }
}
