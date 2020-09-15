using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor
{
    public partial class FormMain : Form
    {
        private const int MaxRetries = 2;
        private BackgroundWorker _backgroundWorker;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            _logBuilder.AppendLine(DateTime.Now.ToString("F"));
            _logBuilder.AppendLine();
            _logBuilder.AppendLine("ZipExtractor started with following command line arguments.");

            string[] args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                _logBuilder.AppendLine($"[{index}] {arg}");
            }

            _logBuilder.AppendLine();

            if (args.Length >= 4)
            {
                string executablePath = args[3];

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
                    foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath)))
                    {
                        try
                        {
                            if (process.MainModule != null && process.MainModule.FileName.Equals(executablePath))
                            {
                                _logBuilder.AppendLine("Waiting for application process to exit...");

                                _backgroundWorker.ReportProgress(0, "Waiting for application to exit...");
                                process.WaitForExit();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                    }

                    _logBuilder.AppendLine("BackgroundWorker started successfully.");

                    var path = args[2];
                    
                    // Ensures that the last character on the extraction path
                    // is the directory separator char.
                    // Without this, a malicious zip file could try to traverse outside of the expected
                    // extraction path.
                    if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        path += Path.DirectorySeparatorChar;

#if NET45
                    var archive = ZipFile.OpenRead(args[1]);
                    
                    var entries = archive.Entries;
#else
                    // Open an existing zip file for reading.
                    var zip = ZipStorer.Open(args[1], FileAccess.Read);
                    
                    // Read the central directory collection.
                    var entries = zip.ReadCentralDir();
#endif

                    _logBuilder.AppendLine($"Found total of {entries.Count} files and folders inside the zip file.");

                    try
                    {
                        int progress = 0;
                        for (var index = 0; index < entries.Count; index++)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                eventArgs.Cancel = true;
                                break;
                            }

                            var entry = entries[index];

#if NET45
                            string currentFile = string.Format(Resources.CurrentFileExtracting, entry.FullName);
#else
                            string currentFile = string.Format(Resources.CurrentFileExtracting, entry.FilenameInZip);
#endif
                            _backgroundWorker.ReportProgress(progress, currentFile);
                            int retries = 0;
                            bool notCopied = true;
                            while (notCopied)
                            {
                                string filePath = String.Empty;
                                try
                                {
#if NET45
                                    filePath = Path.Combine(path, entry.FullName);
                                    if (!entry.IsDirectory())
                                    {
                                        var parentDirectory = Path.GetDirectoryName(filePath);
                                        if (!Directory.Exists(parentDirectory))
                                        {
                                            Directory.CreateDirectory(parentDirectory);
                                        }
                                        entry.ExtractToFile(filePath, true);
                                    }
#else
                                    filePath = Path.Combine(path, entry.FilenameInZip);
                                    zip.ExtractFile(entry, filePath);
#endif
                                    notCopied = false;
                                }
                                catch (IOException exception)
                                {
                                    const int errorSharingViolation = 0x20;
                                    const int errorLockViolation = 0x21;
                                    var errorCode = Marshal.GetHRForException(exception) & 0x0000FFFF;
                                    if (errorCode == errorSharingViolation || errorCode == errorLockViolation)
                                    {
                                        retries++;
                                        if (retries > MaxRetries)
                                        {
                                            throw;
                                        }

                                        List<Process> lockingProcesses = null;
                                        if (Environment.OSVersion.Version.Major >= 6 && retries >= 2)
                                        {
                                            try
                                            {
                                                lockingProcesses = FileUtil.WhoIsLocking(filePath);
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }
                                        }

                                        if (lockingProcesses == null)
                                        {
                                            Thread.Sleep(5000);
                                        }
                                        else
                                        {
                                            foreach (var lockingProcess in lockingProcesses)
                                            {
                                                var dialogResult = MessageBox.Show(
                                                    string.Format(Resources.FileStillInUseMessage,
                                                        lockingProcess.ProcessName, filePath),
                                                    Resources.FileStillInUseCaption,
                                                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                                if (dialogResult == DialogResult.Cancel)
                                                {
                                                    throw;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }

                            progress = (index + 1) * 100 / entries.Count;
                            _backgroundWorker.ReportProgress(progress, currentFile);

                            _logBuilder.AppendLine($"{currentFile} [{progress}%]");
                        }
                    }
                    finally
                    {
#if NET45
                        archive.Dispose();
#else
                        zip.Close();
#endif
                    }
                };

                _backgroundWorker.ProgressChanged += (o, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    textBoxInformation.Text = eventArgs.UserState.ToString();
                    textBoxInformation.SelectionStart = textBoxInformation.Text.Length;
                    textBoxInformation.SelectionLength = 0;
                };

                _backgroundWorker.RunWorkerCompleted += (o, eventArgs) =>
                {
                    try
                    {
                        if (eventArgs.Error != null)
                        {
                            throw eventArgs.Error;
                        }

                        if (!eventArgs.Cancelled)
                        {
                            textBoxInformation.Text = @"Finished";
                            try
                            {
                                ProcessStartInfo processStartInfo = new ProcessStartInfo(executablePath);
                                if (args.Length > 4)
                                {
                                    processStartInfo.Arguments = args[4];
                                }

                                Process.Start(processStartInfo);

                                _logBuilder.AppendLine("Successfully launched the updated application.");
                            }
                            catch (Win32Exception exception)
                            {
                                if (exception.NativeErrorCode != 1223)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _logBuilder.AppendLine();
                        _logBuilder.AppendLine(exception.ToString());

                        MessageBox.Show(exception.Message, exception.GetType().ToString(),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        _logBuilder.AppendLine();
                        Application.Exit();
                    }
                };

                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();

            _logBuilder.AppendLine();
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipExtractor.log"),
                _logBuilder.ToString());
        }
    }
}
