using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Identifies the type of download client required to retrieve the 
    /// update file, i.e. WebClient, SFTP, etc.
    /// </summary>
    public enum FileDownloadClientType
    {
        /// <summary>
        /// The type of client to use is unknown.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The update process will use the Web Client for 
        /// a traditional HTTP(S) file download.
        /// </summary>
        WebClient = 1,

        /// <summary>
        /// The update process will use an SSH client to connect 
        /// to a secure FTP site that is hosting the update file.
        /// </summary>
        SFTPClient = 2
    }
}
