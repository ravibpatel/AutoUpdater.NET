using SevenZipExtractor;
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
        private readonly StringBuilder _logBuilder = new();

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
                string zipPath = args[1];
                string extractionPath = args[2];
                string executablePath = args[3];
                bool clearAppDirectory = args.Length > 4 && args[4] == "-c";
                string commandLineArgs = args.Length > 5 ? args[5] : string.Empty;

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (_, eventArgs) =>
                {
                    foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath)))
                    {
                        try
                        {
                            if (process.MainModule is {FileName: { }} && process.MainModule.FileName.Equals(executablePath))
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
                    
                    // Ensures that the last character on the extraction path
                    // is the directory separator char.
                    // Without this, a malicious zip file could try to traverse outside of the expected
                    // extraction path.
                    if (!extractionPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    {
                        extractionPath += Path.DirectorySeparatorChar;
                    }
                    //var archive = ZipFile.OpenRead(zipPath);

                    //var entries = archive.Entries;
                  
                    try
                    {
                        int progress = 0;
                        
                        if (clearAppDirectory)
                        {
                            _logBuilder.AppendLine($"Removing all files and folders from {extractionPath}.");
                            DirectoryInfo directoryInfo = new DirectoryInfo(extractionPath);

                            foreach (FileInfo file in directoryInfo.GetFiles())
                            {
                                _logBuilder.AppendLine($"Removing a file located at {file.FullName}.");
                                _backgroundWorker.ReportProgress(0, string.Format(Resources.Removing, file.FullName));
                                file.Delete(); 
                            }
                            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                            {
                                _logBuilder.AppendLine($"Removing a directory located at {directory.FullName} and all its contents.");
                                _backgroundWorker.ReportProgress(0, string.Format(Resources.Removing, directory.FullName));
                                directory.Delete(true); 
                            }
                        }
                        using (ArchiveFile archiveFile = new ArchiveFile(zipPath))
                        {
                            var allcount = archiveFile.Entries.Count;
                            _logBuilder.AppendLine($"Found total of { allcount} files and folders inside the zip file.");
                            var entryindex = 0;
                            foreach (Entry entry in archiveFile.Entries)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    eventArgs.Cancel = true;
                                    break;
                                }

                                string currentFile = string.Format(Resources.CurrentFileExtracting, entry.FileName);
                                _backgroundWorker.ReportProgress(progress, currentFile);
                                int retries = 0;
                                bool notCopied = true;
                                while (notCopied)
                                {
                                    string filePath = String.Empty;
                                    try
                                    {
                                        filePath = Path.Combine(extractionPath, entry.FileName);
                                        if (!entry.IsFolder)
                                        {
                                            var parentDirectory = Path.GetDirectoryName(filePath);
                                            if (!Directory.Exists(parentDirectory))
                                            {
                                                Directory.CreateDirectory(parentDirectory);
                                            }
                                            // extract to file
                                            entry.Extract(entry.FileName, true);
                                        }
                                        notCopied = false;
                                    }
                                    catch (IOException exception)
                                    {
                                        const int errorSharingViolation = 0x20;
                                        const int errorLockViolation = 0x21;
                                        var errorCode = Marshal.GetHRForException(exception) & 0x0000FFFF;
                                        if (errorCode is errorSharingViolation or errorLockViolation)
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

                                progress = (entryindex + 1) * 100 / allcount;
                                _backgroundWorker.ReportProgress(progress, currentFile);

                                _logBuilder.AppendLine($"{currentFile} [{progress}%]");
                                entryindex++;
                            }
                        }
                    }
                    finally
                    {
                        //archive.Dispose();
                    }
                };

                _backgroundWorker.ProgressChanged += (_, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    textBoxInformation.Text = eventArgs.UserState?.ToString();
                    if (textBoxInformation.Text != null)
                    {
                        textBoxInformation.SelectionStart = textBoxInformation.Text.Length;
                        textBoxInformation.SelectionLength = 0;
                    }
                };

                _backgroundWorker.RunWorkerCompleted += (_, eventArgs) =>
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
                                if (!string.IsNullOrEmpty(commandLineArgs))
                                {
                                    processStartInfo.Arguments = commandLineArgs;
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
