using System;

namespace AutoUpdaterDotNET;

/// <summary>
///     Provides a mechanism for storing AutoUpdater state between sessions.
/// </summary>
public interface IPersistenceProvider
{
    /// <summary>
    ///     Reads the flag indicating whether a specific version should be skipped or not.
    /// </summary>
    /// <returns>Returns a version to skip. If skip value is false or not present then it will return null.</returns>
    Version GetSkippedVersion();

    /// <summary>
    ///     Reads the value containing the date and time at which the user must be given again the possibility to upgrade the
    ///     application.
    /// </summary>
    /// <returns>
    ///     Returns a DateTime value at which the user must be given again the possibility to upgrade the application. If
    ///     remind later value is not present then it will return null.
    /// </returns>
    DateTime? GetRemindLater();

    /// <summary>
    ///     Sets the values indicating the specific version that must be ignored by AutoUpdater.
    /// </summary>
    /// <param name="version">
    ///     Version code for the specific version that must be ignored. Set it to null if you don't want to
    ///     skip any version.
    /// </param>
    void SetSkippedVersion(Version version);

    /// <summary>
    ///     Sets the date and time at which the user must be given again the possibility to upgrade the application.
    /// </summary>
    /// <param name="remindLaterAt">
    ///     Date and time at which the user must be given again the possibility to upgrade the
    ///     application.
    /// </param>
    void SetRemindLater(DateTime? remindLaterAt);
}