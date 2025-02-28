using AutoUpdaterDotNET.ChangelogViewers;

namespace AutoUpdaterDotNET.WebView2;

/// <summary>
/// Provides a changelog viewer that uses the modern WebView2 control.
/// </summary>
public class WebView2ViewerProvider(int priority = 3) : IChangelogViewerProvider
{
    /// <summary>
    /// Creates a new instance of the <see cref="WebView2ViewerProvider"/> with default priority 3.
    /// </summary>
    public WebView2ViewerProvider() : this(3) { }

    /// <summary>
    /// Gets whether WebView2 runtime is available on the current system.
    /// </summary>
    public bool IsAvailable => WebView2Viewer.IsAvailable();

    /// <inheritdoc />
    public int Priority { get; } = priority;

    /// <summary>
    /// Creates a new instance of the <see cref="WebView2Viewer"/> class.
    /// </summary>
    public IChangelogViewer CreateViewer() => new WebView2Viewer();
}
