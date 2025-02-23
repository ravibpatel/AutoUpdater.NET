using System.Windows.Forms;

namespace AutoUpdaterDotNET.ChangelogViewers;

public interface IChangelogViewer
{
    Control Control { get; }
    bool SupportsUrl { get; }
    void LoadContent(string content);
    void LoadUrl(string url);
    void Cleanup();
}