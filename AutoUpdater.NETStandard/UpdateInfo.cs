using System;
using System.Xml.Serialization;

namespace AutoUpdater.NETStandard
{
    /// <summary>
    ///     Enum representing the beahviour of the update procedure.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// In this mode, it will use all available libaray features.
        /// </summary>
        Normal,

        /// <summary>
        /// In this mode, it ignores Remind Later and Skip values set previously and hide both buttons.
        /// </summary>
        Recommended,

        /// <summary>
        /// In this mode, it won't show close button in addition to Normal mode behaviour.
        /// </summary>
        Forced,

        /// <summary>
        /// In this mode, it will start downloading and applying update without showing standarad update dialog in addition to Forced mode behaviour.
        /// </summary>
        Critical
    }

    public class UpdateInfo : EventArgs
    {
        /// <summary>
        ///     Application name retrieved from Assembly.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     If new update is available then returns true otherwise false.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        ///     Download URL of the update file.
        /// </summary>
        [XmlElement("download")]
        public string DownloadURL { get; set; }

        /// <summary>
        ///     URL of the webpage specifying changes in the new update.
        /// </summary>
        [XmlElement("changelog")]
        public string ChangelogURL { get; set; }

        /// <summary>
        ///     Returns newest version of the application available to download.
        /// </summary>
        [XmlElement("version")]
        public String CurrentVersion { get; set; }

        /// <summary>
        ///     Returns version of the application currently installed on the user's PC.
        /// </summary>
        public String InstalledVersion { get; set; }

        /// <summary>
        /// Checksum of the update file.
        /// </summary>
        [XmlElement("checksum")]
        public CheckSum CheckSum { get; set; }

        /// <summary>
        ///     Defines behaviour of the update procedure.
        /// </summary>
        [XmlElement("type")]
        public UpdateType Type { get; set; }

        /// <summary>
        ///     Command line arguments used by Installer.
        /// </summary>
        [XmlElement("args")]
        public string InstallerArgs { get; set; }
    }

    public class CheckSum
    {
        /// <summary>
        ///     Hash of the file.
        /// </summary>
        [XmlText]
        public string Text { get; set; }

        /// <summary>
        ///     Hash algorithm that generated the hash.
        /// </summary>
        [XmlAttribute("algorithm")]
        public string HashingAlgorithm { get; set; }

    }
}
