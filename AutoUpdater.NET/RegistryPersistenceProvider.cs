using System;
using System.Globalization;
using Microsoft.Win32;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Provides a mechanism for storing AutoUpdater state between sessions based on storing data on the Windows Registry.
    /// </summary>
    public class RegistryPersistenceProvider : IPersistenceProvider
    {
        /// <summary>
        /// Gets/sets the path for the Windows Registry key that will contain the data.
        /// </summary>
        public string RegistryLocation { get; }

        /// <summary>
        /// Initializes a new instance of the RegistryPersistenceProvider class indicating the path for the Windows registry key to use for storing the data.
        /// </summary>
        /// <param name="registryLocation"></param>
        public RegistryPersistenceProvider(string registryLocation)
        {
            RegistryLocation = registryLocation;
        }

        /// <inheritdoc />
        public Version GetSkippedVersion()
        {
            try
            {
                using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation))
                {
                    object versionValue = updateKey?.GetValue("version");

                    if (versionValue != null)
                    {
                        return new Version(versionValue.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }


        /// <inheritdoc />
        public DateTime? GetRemindLater()
        {
            using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation))
            {
                object remindLaterValue = updateKey?.GetValue("remindlater");

                if (remindLaterValue != null)
                {
                    return Convert.ToDateTime(remindLaterValue.ToString(),
                        CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);
                }

                return null;
            }
        }

        /// <inheritdoc />
        public void SetSkippedVersion(Version version)
        {
            using (RegistryKey autoUpdaterKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
            {
                autoUpdaterKey?.SetValue("version", version != null ? version.ToString() : string.Empty);
            }
        }

        /// <inheritdoc />
        public void SetRemindLater(DateTime remindLater)
        {
            using (RegistryKey autoUpdaterKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
            {
                autoUpdaterKey?.SetValue("remindlater",
                    remindLater.ToString(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat));
            }
        }
    }
}