using System;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Provides a mechanism for storing AutoUpdater state between sessions.
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>
        /// Reads the flag indicating whether a specific version should be skipped or not.
        /// </summary>
        /// <param name="skip">On return, this output parameter will be filled with the current state for skip flag.</param>
        /// <param name="version">On return, this output parameter will be filled with the current version code to be skipped. It should be ignored whenever the skip flag is set to <code>false</code>.</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        bool GetSkippedApplicationVersion(out bool skip, out string version);

        /// <summary>
        /// Reads the value containing the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="remindLater">On return, this output parameter will be filled with the date and time at which the user must be given again the possibility to upgrade the application</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        bool GetRemindLater(out DateTime remindLater);

        /// <summary>
        /// Sets the values indicating the specific version that must be ignored by AutoUpdater.
        /// </summary>
        /// <param name="skip">Flag indicating that a specific version must be ignored (<code>true</code>) or not (<code>false</code>).</param>
        /// <param name="version">Version code for the specific version that must be ignored. This value is taken into account only when <paramref name="skip"/> has value <code>true</code>.</param>
        void SetSkippedApplicationVersion(bool skip, string version);

        /// <summary>
        /// Sets the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="remindLater">Date and time at which the user must be given again the possibility to upgrade the application.</param>
        void SetRemindLater(DateTime remindLater);
    }
}
