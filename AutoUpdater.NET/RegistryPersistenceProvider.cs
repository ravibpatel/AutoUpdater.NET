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


        /// <summary>
        /// Reads the flag indicating whether a specific version should be skipped or not.
        /// </summary>
        /// <param name="skip">On return, this output parameter will be filled with the current state for skip flag.</param>
        /// <param name="version">On return, this output parameter will be filled with the current version code to be skipped. It should be ignored whenever the skip flag is set to <code>false</code>.</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        /// <remarks>This function does not create the registry key if it does not exist.</remarks>
        public bool GetSkippedApplicationVersion(out bool skip, out string version)
        {
            skip = false;
            version = null;

            using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation))
            {
                if (updateKey == null)
                    return false;

                object skipRegKey = updateKey.GetValue("skip");
                object versionRegKey = updateKey.GetValue("version");

                if (skipRegKey == null || versionRegKey == null)
                    return false;

                skip = "1".Equals(skipRegKey.ToString());
                version = versionRegKey.ToString();

                return true;
            }
        }


        /// <summary>
        /// Reads the value containing the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="remindLater">On return, this output parameter will be filled with the date and time at which the user must be given again the possibility to upgrade the application</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        /// <remarks>This function does not create the registry key if it does not exist.</remarks>
        public bool GetRemindLater(out DateTime remindLater)
        {
            remindLater = DateTime.MinValue;

            using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation))
            {
                object remindLaterRegKey = updateKey?.GetValue("remindlater");

                if (remindLaterRegKey == null)
                    return false;

                remindLater = Convert.ToDateTime(remindLaterRegKey.ToString(),
                    CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);

                return true;
            }
        }


        /// <summary>
        /// Sets the values indicating the specific version that must be ignored by AutoUpdater.
        /// </summary>
        /// <param name="skip">Flag indicating that a specific version must be ignored (<code>true</code>) or not (<code>false</code>).</param>
        /// <param name="version">Version code for the specific version that must be ignored. This value is taken into account only when <paramref name="skip"/> has value <code>true</code>.</param>
        public void SetSkippedApplicationVersion(bool skip, string version)
        {
            using (RegistryKey updateKeyWrite = Registry.CurrentUser.CreateSubKey(RegistryLocation))
            {
                if (updateKeyWrite != null)
                {
                    updateKeyWrite.SetValue("version", version);
                    updateKeyWrite.SetValue("skip", skip ? 1 : 0);
                }
            }
        }


        /// <summary>
        /// Sets the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="remindLater">Date and time at which the user must be given again the possibility to upgrade the application.</param>
        public void SetRemindLater(DateTime remindLater)
        {
            using (RegistryKey updateKeyWrite = Registry.CurrentUser.CreateSubKey(RegistryLocation))
            {
                updateKeyWrite?.SetValue("remindlater",
                    remindLater.ToString(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat));
            }
        }
    }
}