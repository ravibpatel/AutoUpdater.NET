using System.Windows.Forms;

namespace AutoUpdaterDotNET.ChangelogViewers;

/// <summary>
/// Interface for changelog viewer implementations
/// </summary>
public interface IChangelogViewer
{
    /// <summary>
    /// The Windows Forms control that displays the changelog
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Whether this viewer supports loading content from URLs directly
    /// </summary>
    bool SupportsUrl { get; }

    /// <summary>
    /// Load changelog content as HTML or plain text
    /// </summary>
    void LoadContent(string content);

    /// <summary>
    /// Load changelog content from a URL
    /// </summary>
    void LoadUrl(string url);

    /// <summary>
    /// Clean up any resources used by the viewer
    /// </summary>
    void Cleanup();
}