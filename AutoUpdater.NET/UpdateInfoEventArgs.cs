using System;
using System.Xml.Serialization;

namespace AutoUpdaterDotNET;

/// <summary>
///     Object of this class gives you all the details about the update useful in handling the update logic yourself.
/// </summary>
[XmlRoot("item")]
public class UpdateInfoEventArgs : EventArgs
{
    private string _changelogURL;
    private string _downloadURL;

    /// <inheritdoc />
    public UpdateInfoEventArgs()
    {
        Mandatory = new Mandatory();
    }

    /// <summary>
    ///     If new update is available then returns true otherwise false.
    /// </summary>
    public bool IsUpdateAvailable { get; set; }

    /// <summary>
    ///     If there is an error while checking for update then this property won't be null.
    /// </summary>
    [XmlIgnore]
    public Exception Error { get; set; }

    /// <summary>
    ///     Download URL of the update file.
    /// </summary>
    [XmlElement("url")]
    public string DownloadURL
    {
        get => GetURL(AutoUpdater.BaseUri, _downloadURL);
        set => _downloadURL = value;
    }

    /// <summary>
    ///     URL of the webpage specifying changes in the new update.
    /// </summary>
    [XmlElement("changelog")]
    public string ChangelogURL
    {
        get => GetURL(AutoUpdater.BaseUri, _changelogURL);
        set => _changelogURL = value;
    }

    /// <summary>
    ///     Returns text specifying changes in the new update.
    /// </summary>
    [XmlElement("changelogText")]
    public string ChangelogText { get; set; }

    /// <summary>
    ///     Returns newest version of the application available to download.
    /// </summary>
    [XmlElement("version")]
    public string CurrentVersion { get; set; }

    /// <summary>
    ///     Returns version of the application currently installed on the user's PC.
    /// </summary>
    public Version InstalledVersion { get; set; }

    /// <summary>
    ///     Shows if the update is required or optional.
    /// </summary>
    [XmlElement("mandatory")]
    public Mandatory Mandatory { get; set; }

    /// <summary>
    ///     Executable path of the updated application relative to installation directory.
    /// </summary>
    [XmlElement("executable")]
    public string ExecutablePath { get; set; }

    /// <summary>
    ///     Command line arguments used by Installer.
    /// </summary>
    [XmlElement("args")]
    public string InstallerArgs { get; set; }

    /// <summary>
    ///     Checksum of the update file.
    /// </summary>
    [XmlElement("checksum")]
    public CheckSum CheckSum { get; set; }

    internal static string GetURL(Uri baseUri, string url)
    {
        if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Relative))
        {
            var uri = new Uri(baseUri, url);

            if (uri.IsAbsoluteUri)
            {
                url = uri.AbsoluteUri;
            }
        }

        return url;
    }
}

/// <summary>
///     Mandatory class to fetch the XML values related to Mandatory field.
/// </summary>
public class Mandatory
{
    /// <summary>
    ///     Value of the Mandatory field.
    /// </summary>
    [XmlText]
    public bool Value { get; set; }

    /// <summary>
    ///     If this is set and 'Value' property is set to true then it will trigger the mandatory update only when current
    ///     installed version is less than value of this property.
    /// </summary>
    [XmlAttribute("minVersion")]
    public string MinimumVersion { get; set; }

    /// <summary>
    ///     Mode that should be used for this update.
    /// </summary>
    [XmlAttribute("mode")]
    public Mode UpdateMode { get; set; }
}

/// <summary>
///     Checksum class to fetch the XML values for checksum.
/// </summary>
public class CheckSum
{
    /// <summary>
    ///     Hash of the file.
    /// </summary>
    [XmlText]
    public string Value { get; set; }

    /// <summary>
    ///     Hash algorithm that generated the hash.
    /// </summary>
    [XmlAttribute("algorithm")]
    public string HashingAlgorithm { get; set; }
}