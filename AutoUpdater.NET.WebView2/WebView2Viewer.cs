using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using AutoUpdaterDotNET.ChangelogViewers;

namespace AutoUpdaterDotNET.WebView2;

/// <summary>
/// A changelog viewer for displaying changelogs using a modern browser.
/// </summary>
public class WebView2Viewer : IChangelogViewer
{
    private bool _isInitialized;
    private readonly Microsoft.Web.WebView2.WinForms.WebView2 _webView = new()
    {
        Dock = DockStyle.Fill,
        AllowExternalDrop = false
    };

    /// <inheritdoc />
    public Control Control => _webView;

    /// <inheritdoc />
    public bool SupportsUrl => true;

    private async Task EnsureInitialized()
    {
        if (_isInitialized) return;

        try
        {
            await _webView.EnsureCoreWebView2Async(await CoreWebView2Environment.CreateAsync(null, Path.GetTempPath()));
            _isInitialized = true;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("WebView2 runtime is not available");
        }
    }

    /// <inheritdoc />
    public async void LoadContent(string content)
    {
        await EnsureInitialized();
        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping("local.files", Path.GetTempPath(), CoreWebView2HostResourceAccessKind.Allow);

        // Write content to a temporary HTML file
        var tempFile = Path.Combine(Path.GetTempPath(), "changelog.html");
        File.WriteAllText(tempFile, content, System.Text.Encoding.UTF8);

        // Navigate to the local file
        _webView.CoreWebView2.Navigate("https://local.files/changelog.html");
    }

    /// <inheritdoc />
    public async void LoadUrl(string url)
    {
        await EnsureInitialized();

        if (AutoUpdater.BasicAuthChangeLog != null)
        {
            _webView.CoreWebView2.BasicAuthenticationRequested += delegate (object _, CoreWebView2BasicAuthenticationRequestedEventArgs args)
                {
                args.Response.UserName = ((BasicAuthentication)AutoUpdater.BasicAuthChangeLog).Username;
                args.Response.Password = ((BasicAuthentication)AutoUpdater.BasicAuthChangeLog).Password;
            };
        }

        _webView.CoreWebView2.Navigate(url);
    }

    /// <inheritdoc />
    public void Cleanup()
    {
        if (File.Exists(Path.Combine(Path.GetTempPath(), "changelog.html")))
        {
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "changelog.html"));
            }
            catch
            {
                // Ignore deletion errors
            }
        }
        _webView.Dispose();
    }

    /// <summary>
    /// Checks if WebView2 is available on the current environment
    /// </summary>
    /// <returns>True if WebView2 is available, false otherwise</returns>
    public static bool IsAvailable()
    {
        try
        {
            var availableBrowserVersion = CoreWebView2Environment.GetAvailableBrowserVersionString(null);
            const string requiredMinBrowserVersion = "86.0.616.0";
            return !string.IsNullOrEmpty(availableBrowserVersion) &&
                   CoreWebView2Environment.CompareBrowserVersions(availableBrowserVersion,
                       requiredMinBrowserVersion) >= 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}