using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using CommandLine;

namespace ZipExtractor.NETCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _backgroundWorker;
        readonly StringBuilder _logBuilder = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
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

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        private void RunOptions(Options opts)
        {
            _logBuilder.AppendLine();

            // Extract all the files.
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _backgroundWorker.DoWork += (o, eventArgs) =>
            {
                try
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        try
                        {
                            if (process.MainModule.FileName.Equals(opts.Executable))
                            {
                                _logBuilder.AppendLine("Waiting for application process to Exit...");

                                _backgroundWorker.ReportProgress(0, "Waiting for application to Exit...");
                                process.WaitForExit();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }

                _logBuilder.AppendLine("BackgroundWorker started successfully.");

                string path = opts.Path;

                if (string.IsNullOrEmpty(opts.Path))
                {
                    path = Path.GetDirectoryName(opts.Executable);
                }

                // Ensures that the last character on the extraction path
                // is the directory separator char. 
                // Without this, a malicious zip file could try to traverse outside of the expected
                // extraction path.
                if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    path += Path.DirectorySeparatorChar;

                using ZipArchive archive = ZipFile.OpenRead(opts.Zip);
                int fileCount = archive.Entries.Count;

                _logBuilder.AppendLine($"Found total of {fileCount} files and folders inside the zip file.");

                for (var index = 0; index < fileCount; index++)
                {
                    ZipArchiveEntry entry = archive.Entries[index];
                    if (_backgroundWorker.CancellationPending)
                    {
                        eventArgs.Cancel = true;
                        return;
                    }

                    // Gets the full path to ensure that relative segments are removed.
                    string destinationPath = Path.GetFullPath(Path.Combine(path, entry.FullName));

                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                    // are case-insensitive.
                    if (destinationPath.StartsWith(path, StringComparison.Ordinal))
                    {
                        if (Path.EndsInDirectorySeparator(destinationPath) && entry.Length == 0)
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        else
                        {
                            entry.ExtractToFile(destinationPath, true);
                        }

                        string currentFile = $"Extracting {destinationPath}";
                        int progress = (index + 1) * 100 / fileCount;
                        _backgroundWorker.ReportProgress(progress, currentFile);

                        _logBuilder.AppendLine($"{currentFile} [{progress}%]");
                    }
                }
            };

            _backgroundWorker.ProgressChanged += (o, eventArgs) =>
            {
                ProgressBar.Value = eventArgs.ProgressPercentage;
                LabelInfo.Content = eventArgs.UserState.ToString();
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
                        LabelInfo.Content = @"Finished";
                        try
                        {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo(opts.Executable);
                            if (!string.IsNullOrEmpty(opts.Args))
                            {
                                processStartInfo.Arguments = opts.Args;
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

                    MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _logBuilder.AppendLine();
                    Application.Current.Shutdown();
                }
            };

            _backgroundWorker.RunWorkerAsync();
        }
        private void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _backgroundWorker?.CancelAsync();

            _logBuilder.AppendLine();
            File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "ZipExtractor.log"), _logBuilder.ToString());
        }
    }

    public class Options
    {
        [Option('e', "executable", Required = true, HelpText = "Set executable you want to launch after the extraction")]
        public string Executable { get; set; }

        [Option('p', "path", Required = false, HelpText = "Set path where you want to extract the content of zip file (Defaults to executable path)")]
        public string Path { get; set; }

        [Option('a', "args", Required = false, HelpText = "Set command line arguments for the executable")]
        public string Args { get; set; }

        [Option('z', "zip", Required = true, HelpText = "Set path of the zip file you want to extract")]
        public string Zip { get; set; }
    }
}
