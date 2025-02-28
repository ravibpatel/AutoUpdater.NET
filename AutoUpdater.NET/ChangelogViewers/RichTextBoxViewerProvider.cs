namespace AutoUpdaterDotNET.ChangelogViewers;

/// <summary>
/// Provides a text viewer that uses a RichTextBox control.
/// </summary>
public class RichTextBoxViewerProvider(int priority = 0) : IChangelogViewerProvider
{
    /// <summary>
    /// Gets whether this provider is available. Always returns true as RichTextBox is always available.
    /// </summary>
    public bool IsAvailable => true;

    /// <inheritdoc />
    public int Priority { get; } = priority;

    /// <summary>
    /// Creates a new instance of the <see cref="RichTextBoxViewer"/> class.
    /// </summary>
    public IChangelogViewer CreateViewer() => new RichTextBoxViewer();
}
