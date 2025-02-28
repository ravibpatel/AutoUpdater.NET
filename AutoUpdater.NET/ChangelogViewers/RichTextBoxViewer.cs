using System.Windows.Forms;

namespace AutoUpdaterDotNET.ChangelogViewers;

/// <summary>
/// A changelog viewer for displaying changelogs using a RichTextBox control.
/// </summary>
public class RichTextBoxViewer : IChangelogViewer
{
    private readonly RichTextBox _richTextBox = new()
    {
        ReadOnly = true,
        Dock = DockStyle.Fill,
        BackColor = System.Drawing.SystemColors.Control,
        BorderStyle = BorderStyle.Fixed3D
    };

    /// <inheritdoc />
    public Control Control => _richTextBox;

    /// <summary>
    /// Always returns false
    /// </summary>
    public bool SupportsUrl => false;

    /// <summary>
    /// Loads the content into the RichTextBox control.
    /// </summary>
    /// <param name="content">The content to load.</param>
    public void LoadContent(string content)
    {
        _richTextBox.Text = content;
    }

    /// <summary>
    /// Loads the RTF content into the RichTextBox control.
    /// </summary>
    /// <param name="content">The RTF content to load.</param>
    public void LoadContentRtf(string content)
    {
        _richTextBox.Rtf = content;
    }

    /// <summary>
    /// Always throws a NotSupportedException
    /// </summary>
    /// <exception cref="System.NotSupportedException"></exception>
    public void LoadUrl(string url)
    {
        throw new System.NotSupportedException("RichTextBox does not support loading from URL");
    }

    /// <inheritdoc />
    public void Cleanup()
    {
        _richTextBox.Dispose();
    }
}