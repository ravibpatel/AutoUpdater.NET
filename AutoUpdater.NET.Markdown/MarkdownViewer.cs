using System.Windows.Forms;
using AutoUpdaterDotNET.ChangelogViewers;
using Markdig;

namespace AutoUpdaterDotNET.Markdown;

/// <summary>
/// A changelog viewer that renders Markdown content using either a provided viewer or a WebBrowser control.
/// </summary>
public class MarkdownViewer : IChangelogViewer
{
    private readonly IChangelogViewer _innerViewer;
    private readonly bool _cleanup;
    private const string DefaultStyle = @"
        <style>
            @font-face {
                font-family: 'Segoe UI Emoji';
                src: local('Segoe UI Emoji');
            }
            body { 
                font-family: 'Segoe UI Emoji', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                font-size: 10.5pt;
                line-height: 1.4;
                word-wrap: break-word;
                padding: 12px;
                margin: 0;
                background-color: #F4F4F4;
                color: #000000;
            }
            h1, h2, h3, h4, h5, h6 {
                margin-top: 16px;
                margin-bottom: 8px;
                font-weight: 600;
                line-height: 1.25;
            }
            h1 { font-size: 20pt; }
            h2 { font-size: 16pt; }
            h3 { font-size: 14pt; }
            h4 { font-size: 12pt; }
            p { margin: 8px 0; }
            code {
                font-family: Consolas, 'Courier New', monospace;
                padding: 2px 4px;
                background-color: rgba(0, 0, 0, 0.03);
                border-radius: 3px;
                font-size: 10pt;
            }
            pre code {
                display: block;
                padding: 8px;
                margin: 8px 0;
                overflow: auto;
                line-height: 1.45;
            }
            blockquote {
                padding: 0 8px;
                margin: 8px 0;
                color: #666666;
                border-left: 3px solid #CCCCCC;
            }
            ul, ol {
                padding-left: 24px;
                margin: 8px 0;
            }
            table {
                border-spacing: 0;
                border-collapse: collapse;
                margin: 8px 0;
                width: 100%;
            }
            table th, table td {
                padding: 4px 8px;
                border: 1px solid #CCCCCC;
            }
            table tr:nth-child(2n) {
                background-color: rgba(0, 0, 0, 0.02);
            }
            hr {
                height: 1px;
                padding: 0;
                margin: 16px 0;
                background-color: #CCCCCC;
                border: 0;
            }
            img { 
                max-width: 100%;
                height: auto;
            }
            a {
                color: #0066CC;
                text-decoration: none;
            }
            a:hover {
                text-decoration: underline;
            }
        </style>";

    /// <summary>
    /// Initializes a new instance of the MarkdownViewer class.
    /// </summary>
    /// <param name="viewer">Optional viewer to use for rendering. If not provided, uses WebBrowser control.</param>
    /// <param name="cleanup">Whether to clean up the inner viewer when this viewer is disposed.</param>
    public MarkdownViewer(IChangelogViewer viewer = null, bool cleanup = true)
    {
        _innerViewer = viewer ?? new WebBrowserViewer();
        _cleanup = cleanup || viewer == null;
    }

    /// <inheritdoc />
    public Control Control => _innerViewer.Control;

    /// <inheritdoc />
    public bool SupportsUrl => true;

    /// <inheritdoc />
    public void LoadContent(string content)
    {
        if (_innerViewer is RichTextBoxViewer or MarkdownViewer)
        {
            _innerViewer.LoadContent(content);
            return;
        }

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        var html = Markdig.Markdown.ToHtml(content, pipeline);
        var fullHtml = $"<!DOCTYPE html><html><head>{DefaultStyle}</head><body>{html}</body></html>";
        
        _innerViewer.LoadContent(fullHtml);
    }

    /// <inheritdoc />
    public void LoadUrl(string url)
    {
        using var client = new System.Net.WebClient();
        if (AutoUpdater.BasicAuthChangeLog != null)
        {
            var auth = (BasicAuthentication)AutoUpdater.BasicAuthChangeLog;
            client.Credentials = new System.Net.NetworkCredential(auth.Username, auth.Password);
        }

        var content = client.DownloadString(url);
        LoadContent(content);
    }

    /// <inheritdoc />
    public void Cleanup()
    {
        if (_cleanup)
            _innerViewer.Cleanup();
    }
}