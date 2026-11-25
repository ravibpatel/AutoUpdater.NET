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
        var relaunchUnelevated = false;
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
                case "--relaunch-unelevated":
                    relaunchUnelevated = true;
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
            {
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
            }

            _logBuilder.AppendLine("BackgroundWorker started successfully.");

            Invoke(() => { ControlBox = false; });

            // Canonicalises the extraction path and ensures it ends with the directory separator char, so
            // GetDestinationPath can check that paths stay inside it. Without the trailing separator, a path
            // resolving to "C:\App.Evil\x" would pass a containment check against "C:\App".
            extractionPath = Path.GetFullPath(extractionPath);
            if (!extractionPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                extractionPath += Path.DirectorySeparatorChar;
            }

            ZipArchive archive = ZipFile.OpenRead(zipPath);

            ReadOnlyCollection<ZipArchiveEntry> entries = archive.Entries;

            try
            {
                // Resolve every destination before touching the disk, so an archive with an entry that
                // escapes the extraction path is rejected without clearing or partially updating the app.
                var destinationPaths = new string[entries.Count];
                for (var index = 0; index < entries.Count; index++)
                {
                    string entryName = entries[index].FullName;
                    destinationPaths[index] = GetDestinationPath(extractionPath, entryName) ?? throw new InvalidDataException(
                        $"The zip entry \"{entryName}\" would be extracted outside of \"{extractionPath}\", so the update was aborted.");
                }

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
                        string filePath = destinationPaths[index];
                        try
                        {
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
                                var dialogResult = DialogResult.None;

                                Invoke(() =>
                                {
                                    dialogResult = MessageBox.Show(this,
                                        string.Format(Resources.FileStillInUseMessage,
                                            lockingProcess.ProcessName, filePath),
                                        Resources.FileStillInUseCaption,
                                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                });

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
                    // The updated executable can come from the update XML, so like the zip entries, it must be
                    // relative to the extraction path and stay inside it.
                    string executablePath = string.IsNullOrWhiteSpace(updatedExe)
                        ? currentExe
                        : GetDestinationPath(extractionPath, updatedExe) ?? throw new ArgumentException(
                            $"The executable \"{updatedExe}\" must be a relative path inside \"{extractionPath}\", so it wasn't launched.");

                    // If the original application was launched unelevated, the updated executable should
                    // be launched unelevated too. AutoUpdater.NET passes --relaunch-unelevated when it
                    // elevated ZipExtractor from a non-elevated process, so that the updated application
                    // does not inherit ZipExtractor's elevated token.
                    ProcessLauncher.Start(executablePath, commandLineArgs, relaunchUnelevated);

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

    /// <summary>
    ///     Returns the full path of a path relative to the extraction path, or null if it is rooted or resolves
    ///     outside of the extraction path, e.g. "../../x", "C:\x", "C:x", "\x" or "\\server\share\x".
    /// </summary>
    /// <param name="extractionPath">Full extraction path, ending with the directory separator char.</param>
    /// <param name="relativePath">Zip entry name or executable path, which may come from the update server.</param>
    private static string GetDestinationPath(string extractionPath, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            return null;
        }

        // Same check as ZipFile.ExtractToDirectory in .NET. Ordinal rather than OrdinalIgnoreCase, as Windows
        // folders can be case-sensitive, where "..\APP\x" leads to a sibling of "app" rather than inside it.
        string fullPath = Path.GetFullPath(Path.Combine(extractionPath, relativePath));
        return fullPath.StartsWith(extractionPath, StringComparison.Ordinal) ? fullPath : null;
    }

    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        _backgroundWorker?.CancelAsync();

        _logBuilder.AppendLine();
        File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipExtractor.log"),
            _logBuilder.ToString());
    }
}