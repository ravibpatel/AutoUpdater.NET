namespace AutoUpdaterDotNET.ChangelogViewers;

/// <summary>
/// Factory for creating changelog viewers
/// </summary>
public interface IChangelogViewerProvider
{
    /// <summary>
    /// Whether this provider can create a viewer in the current environment
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Priority of this provider (higher numbers = higher priority)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Create a new changelog viewer instance
    /// </summary>
    IChangelogViewer CreateViewer();
}