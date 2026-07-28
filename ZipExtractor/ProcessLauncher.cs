using System;
using System.Diagnostics;
using System.IO;

namespace ZipExtractor;

/// <summary>
///     Helper used to launch the updated application. When ZipExtractor is running elevated
///     (because <c>AutoUpdater.RunUpdateAsAdmin</c> was enabled), but the application that started the
///     update was running as a normal user, a plain <see cref="Process.Start(ProcessStartInfo)" />
///     would make the updated application inherit the elevated token. To preserve the integrity level
///     of the application that originally started the update, AutoUpdater.NET asks ZipExtractor to
///     relaunch the application unelevated by having the (unelevated) desktop shell create the process.
/// </summary>
internal static class ProcessLauncher
{
    public static void Start(string executablePath, string arguments, bool unelevated)
    {
        if (unelevated)
        {
            try
            {
                SystemUtility.ExecuteProcessUnelevated(
                    executablePath,
                    arguments ?? string.Empty,
                    Path.GetDirectoryName(executablePath) ?? string.Empty
                );
                return;
            }
            catch (Exception)
            {
                // Fall back to a regular start (the updated application will inherit the
                // elevated token) if launching unelevated fails for any reason.
            }
        }

        var processStartInfo = new ProcessStartInfo(executablePath);
        if (!string.IsNullOrEmpty(arguments))
        {
            processStartInfo.Arguments = arguments;
        }

        Process.Start(processStartInfo);
    }
}