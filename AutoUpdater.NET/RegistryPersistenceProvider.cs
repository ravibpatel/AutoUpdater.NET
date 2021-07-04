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
        private string RegistryLocation { get; }

        private const string RemindLaterValueName = "RemindLaterAt";

        private const string SkippedVersionValueName = "SkippedVersion";

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
                    object skippedVersionValue = updateKey?.GetValue(SkippedVersionValueName);

                    if (skippedVersionValue != null)
                    {
                        return new Version(skippedVersionValue.ToString());
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
                object remindLaterValue = updateKey?.GetValue(RemindLaterValueName);

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
                autoUpdaterKey?.SetValue(SkippedVersionValueName, version != null ? version.ToString() : string.Empty);
            }
        }

        /// <inheritdoc />
        public void SetRemindLater(DateTime? remindLaterAt)
        {
            using (RegistryKey autoUpdaterKey = Registry.CurrentUser.CreateSubKey(RegistryLocation))
            {
                autoUpdaterKey?.SetValue(RemindLaterValueName,
                    remindLaterAt != null
                        ? remindLaterAt.Value.ToString(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat)
                        : string.Empty);
            }
        }
    }
}