using System;

namespace AutoUpdaterDotNET.ChangelogViewers;

public static class ChangelogViewerFactory
{
    public static IChangelogViewer Create(ChangelogViewerType type)
    {
        return type switch
        {
            ChangelogViewerType.RichTextBox => new RichTextBoxViewer(),
            ChangelogViewerType.WebBrowser => new WebBrowserViewer(),
            ChangelogViewerType.WebView2 when WebView2Viewer.IsAvailable() => new WebView2Viewer(),
            ChangelogViewerType.WebView2 => throw new InvalidOperationException("WebView2 runtime is not available"),
            _ => throw new ArgumentException($"Unknown viewer type: {type}")
        };
    }

    public static ChangelogViewerType GetDefaultViewerType()
    {
        if (WebView2Viewer.IsAvailable())
            return ChangelogViewerType.WebView2;
            
        return ChangelogViewerType.WebBrowser;
    }
}