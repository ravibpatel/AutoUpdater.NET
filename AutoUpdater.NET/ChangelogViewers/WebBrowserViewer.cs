using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AutoUpdaterDotNET.ChangelogViewers;

public class WebBrowserViewer : IChangelogViewer
{
    private const string EmulationKey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
    private readonly int _emulationValue;
    private readonly string _executableName;
    private readonly WebBrowser _webBrowser;
    public Control Control => _webBrowser;
    public bool SupportsUrl => true;

    public WebBrowserViewer()
    {
        _webBrowser = new WebBrowser
        {
            Dock = DockStyle.Fill,
            ScriptErrorsSuppressed = true,
            AllowWebBrowserDrop = false,
            WebBrowserShortcutsEnabled = false,
            IsWebBrowserContextMenuEnabled = false
        };

        _executableName = Path.GetFileName(
            Process.GetCurrentProcess().MainModule?.FileName
            ?? System.Reflection.Assembly.GetEntryAssembly()?.Location
            ?? Application.ExecutablePath);

        _emulationValue = _webBrowser.Version.Major switch
        {
            11 => 11001,
            10 => 10001,
            9 => 9999,
            8 => 8888,
            7 => 7000,
            _ => 0
        };

        SetupEmulation();
    }

    public void LoadContent(string content) => _webBrowser.DocumentText = content;

    public void LoadUrl(string url)
    {
        if (AutoUpdater.BasicAuthChangeLog != null)
            _webBrowser.Navigate(url, "", null, $"Authorization: {AutoUpdater.BasicAuthChangeLog}");
        else
            _webBrowser.Navigate(url);
    }
    private void SetupEmulation()
    {
        if (_emulationValue == 0)
            return;

        try
        {
            using var registryKey = Registry.CurrentUser.OpenSubKey(EmulationKey, true);
            registryKey?.SetValue(_executableName, _emulationValue, RegistryValueKind.DWord);
        }
        catch
        {
            // ignored
        }
    }
    private void RemoveEmulation()
    {
        if (_emulationValue == 0)
            return;

        try
        {
            using var registryKey = Registry.CurrentUser.OpenSubKey(EmulationKey, true);
            registryKey?.DeleteValue(_executableName, false);
        }
        catch
        {
            // ignored
        }
    }

    public void Cleanup()
    {
        _webBrowser.Dispose();
        RemoveEmulation();
    }
}