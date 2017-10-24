using System;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Provides information about the available remote update
    /// </summary>
    public interface IUpdateAppInfo
    {
        /// <summary>
        /// Change log URL
        /// </summary>
        string ChangeLogURL { get; }

        /// <summary>
        /// Download URL
        /// </summary>
        string DownloadURL { get; }

        /// <summary>
        /// Current version
        /// </summary>
        Version CurrentVersion { get; }

        /// <summary>
        /// Is the new version Mandatory
        /// </summary>
        bool Mandatory { get; }
    }
}

