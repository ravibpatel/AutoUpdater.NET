using AutoUpdaterDotNET.ChangelogViewers;

namespace AutoUpdaterDotNET.Markdown;

/// <summary>
/// Provides a Markdown viewer that uses a WebBrowser control or a user provided viewer to render Markdown content.
/// </summary>
public class MarkdownViewerProvider(int priority = 2) : IChangelogViewerProvider
{
    private readonly IChangelogViewer _viewer;

    /// <summary>
    /// Creates a new instance of the <see cref="MarkdownViewerProvider"/> with default priority 2.
    /// </summary>
    public MarkdownViewerProvider() : this(2) { }

    /// <summary>
    /// Creates a new instance of the <see cref="MarkdownViewerProvider"/> with a specific viewer.
    /// </summary>
    /// <param name="viewer">The viewer to use for rendering Markdown content.</param>
    /// <param name="priority">The priority of this provider.</param>
    public MarkdownViewerProvider(IChangelogViewer viewer, int priority = 2) : this(priority)
    {
        _viewer = viewer;
    }

    /// <summary>
    /// Gets whether this provider is available. Always returns true as it uses WebBrowser as fallback.
    /// </summary>
    public bool IsAvailable => true;

    /// <inheritdoc />
    public int Priority { get; } = priority;

    /// <summary>
    /// Creates a new instance of the <see cref="MarkdownViewer"/> class.
    /// </summary>
    public IChangelogViewer CreateViewer() => new MarkdownViewer(_viewer);
}