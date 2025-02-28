namespace AutoUpdaterDotNET.ChangelogViewers;

/// <summary>
/// Provides a changelog viewer that uses the default WebBrowser control.
/// </summary>
public class WebBrowserViewerProvider(int priority = 1) : IChangelogViewerProvider
{
    /// <summary>
    /// Creates a new instance of the <see cref="WebBrowserViewerProvider"/> with default priority 1.
    /// </summary>
    public WebBrowserViewerProvider() : this(1) { }

    /// <summary>
    /// Gets whether this provider is available. Always returns true as WebBrowser is always available on Windows.
    /// </summary>
    public bool IsAvailable => true;

    /// <inheritdoc />
    public int Priority { get; } = priority;

    /// <summary>
    /// Creates a new instance of the <see cref="WebBrowserViewer"/> class.
    /// </summary>
    public IChangelogViewer CreateViewer() => new WebBrowserViewer();
}
