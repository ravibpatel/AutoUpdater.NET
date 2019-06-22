using System.Reflection;
using AutoUpdater.NETStandard;

namespace AutoUpdater.NET.WPF
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
            base.Start(url, assembly);

            CheckForUpdate += args =>
            {
                if (args.IsUpdateAvailable)
                {
                    MainWindow mainWindow = new MainWindow(args, this);
                    mainWindow.ShowDialog();
                }
            };
        }
    }
}
