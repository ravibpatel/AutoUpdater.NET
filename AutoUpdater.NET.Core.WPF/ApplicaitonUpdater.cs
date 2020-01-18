using System.Reflection;
using AutoUpdater.NETStandard;

namespace AutoUpdater.NET.Core.WPF
{
    public class ApplicationUpdater : Updater
    {
        /// <summary>
        ///     Set this to false if your application doesn't need administrator privileges to replace the old version.
        /// </summary>
        public static bool RunUpdateAsAdmin = true;

        public override void Start(string url, Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetEntryAssembly();

            CheckForUpdate += args =>
            {
                if (args.IsUpdateAvailable)
                {
                    MainWindow mainWindow = new MainWindow(args);
                    mainWindow.ShowDialog();
                }
            };

            base.Start(url, assembly);
        }
    }
}
