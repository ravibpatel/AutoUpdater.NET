using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor;

public partial class FormMain : Form
{
    private const int MaxRetries = 2;
    private readonly StringBuilder _logBuilder = new();
    private BackgroundWorker _backgroundWorker;

    public FormMain()
    {
        InitializeComponent();
    }

    private void FormMain_Shown(object sender, EventArgs e)
    {
        string zipPath = null;
        string extractionPath = null;
        string currentExe = null;
        string updatedExe = null;
        var clearAppDirectory = false;
        string commandLineArgs = null;

        _logBuilder.AppendLine(DateTime.Now.ToString("F"));
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("ZipExtractor started with following command line arguments.");

        string[] args = Environment.GetCommandLineArgs();
        for (var index = 0; index < args.Length; index++)
        {
            string arg = args[index].ToLower();
            switch (arg)
            {
                case "--input":
                    zipPath = args[index + 1];
                    break;
                case "--output":
                    extractionPath = args[index + 1];
                    break;
                case "--current-exe":
                    currentExe = args[index + 1];
                    break;
                case "--updated-exe":
                    updatedExe = args[index + 1];
                    break;
                case "--clear":
                    clearAppDirectory = true;
                    break;
                case "--args":
                    commandLineArgs = args[index + 1];
                    break;
            }

            _logBuilder.AppendLine($"[{index}] {arg}");
        }

        _logBuilder.AppendLine();

        if (string.IsNullOrEmpty(zipPath) || string.IsNullOrEmpty(extractionPath) || string.IsNullOrEmpty(currentExe))
        {
            return;
        }

        // Extract all the files.
        _backgroundWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        _backgroundWorker.DoWork += (_, eventArgs) =>
        {
            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentExe)))
                try
                {
                    if (process.MainModule is { FileName: not null } && process.MainModule.FileName.Equals(currentExe))
                    {
                        _logBuilder.AppendLine("Waiting for application process to exit...");

                        _backgroundWorker.ReportProgress(0, Resources.WaitingForAppToExitMessage);
                        process.WaitForExit();
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
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

            ZipArchive archive = ZipFile.OpenRead(zipPath);

            ReadOnlyCollection<ZipArchiveEntry> entries = archive.Entries;

            try
            {
                var progress = 0;

                if (clearAppDirectory)
                {
                    _logBuilder.AppendLine($"Removing all files and folders from \"{extractionPath}\".");
                    var directoryInfo = new DirectoryInfo(extractionPath);

                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        _logBuilder.AppendLine($"Removing a file located at \"{file.FullName}\".");
                        _backgroundWorker.ReportProgress(0, string.Format(Resources.Removing, file.FullName));
                        file.Delete();
                    }

                    foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                    {
                        _logBuilder.AppendLine(
                            $"Removing a directory located at \"{directory.FullName}\" and all its contents.");
                        _backgroundWorker.ReportProgress(0, string.Format(Resources.Removing, directory.FullName));
                        directory.Delete(true);
                    }
                }

                _logBuilder.AppendLine($"Found total of {entries.Count} files and folders inside the zip file.");

                for (var index = 0; index < entries.Count; index++)
                {
                    if (_backgroundWorker.CancellationPending)
                    {
                        eventArgs.Cancel = true;
                        break;
                    }

                    ZipArchiveEntry entry = entries[index];

                    string currentFile = string.Format(Resources.CurrentFileExtracting, entry.FullName);
                    _backgroundWorker.ReportProgress(progress, currentFile);
                    var retries = 0;
                    var notCopied = true;
                    while (notCopied)
                    {
                        var filePath = string.Empty;
                        try
                        {
                            filePath = Path.Combine(extractionPath, entry.FullName);
                            if (!entry.IsDirectory())
                            {
                                string parentDirectory = Path.GetDirectoryName(filePath);
                                if (parentDirectory != null)
                                {
                                    if (!Directory.Exists(parentDirectory))
                                    {
                                        Directory.CreateDirectory(parentDirectory);
                                    }
                                }
                                else
                                {
                                    throw new ArgumentNullException($"parentDirectory is null for \"{filePath}\"!");
                                }

                                using (Stream destination = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write,
                                           FileShare.None))
                                {
                                    using Stream stream = entry.Open();
                                    stream.CopyTo(destination);
                                    destination.SetLength(destination.Position);
                                }

                                File.SetLastWriteTime(filePath, entry.LastWriteTime.DateTime);
                            }

                            notCopied = false;
                        }
                        catch (IOException exception)
                        {
                            const int errorSharingViolation = 0x20;
                            const int errorLockViolation = 0x21;
                            int errorCode = Marshal.GetHRForException(exception) & 0x0000FFFF;
                            if (errorCode is not (errorSharingViolation or errorLockViolation))
                            {
                                throw;
                            }

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
                                continue;
                            }

                            foreach (Process lockingProcess in lockingProcesses)
                            {
                                DialogResult dialogResult = MessageBox.Show(this,
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

                    progress = (index + 1) * 100 / entries.Count;
                    _backgroundWorker.ReportProgress(progress, currentFile);

                    _logBuilder.AppendLine($"{currentFile} [{progress}%]");
                }
            }
            finally
            {
                archive.Dispose();
            }
        };

        _backgroundWorker.ProgressChanged += (_, eventArgs) =>
        {
            progressBar.Value = eventArgs.ProgressPercentage;
            textBoxInformation.Text = eventArgs.UserState?.ToString() ?? string.Empty;
            if (textBoxInformation.Text == null)
            {
                return;
            }

            textBoxInformation.SelectionStart = textBoxInformation.Text.Length;
            textBoxInformation.SelectionLength = 0;
        };

        _backgroundWorker.RunWorkerCompleted += (_, eventArgs) =>
        {
            try
            {
                if (eventArgs.Error != null)
                {
                    throw eventArgs.Error;
                }

                if (eventArgs.Cancelled)
                {
                    return;
                }

                textBoxInformation.Text = @"Finished";
                try
                {
                    string executablePath = string.IsNullOrWhiteSpace(updatedExe)
                        ? currentExe
                        : Path.Combine(extractionPath, updatedExe);
                    var processStartInfo = new ProcessStartInfo(executablePath);
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
            catch (Exception exception)
            {
                _logBuilder.AppendLine();
                _logBuilder.AppendLine(exception.ToString());

                MessageBox.Show(this, exception.Message, exception.GetType().ToString(),
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

    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        _backgroundWorker?.CancelAsync();

        _logBuilder.AppendLine();
        File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipExtractor.log"),
            _logBuilder.ToString());
    }
}