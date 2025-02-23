using System.Windows.Forms;

namespace AutoUpdaterDotNET.ChangelogViewers;

public class RichTextBoxViewer : IChangelogViewer
{
    private readonly RichTextBox _richTextBox = new()
    {
        ReadOnly = true,
        Dock = DockStyle.Fill,
        BackColor = System.Drawing.SystemColors.Control,
        BorderStyle = BorderStyle.Fixed3D
    };
    public Control Control => _richTextBox;
    public bool SupportsUrl => false;

    public void LoadContent(string content)
    {
        _richTextBox.Text = content;
    }

    public void LoadUrl(string url)
    {
        throw new System.NotSupportedException("RichTextBox does not support loading from URL");
    }

    public void Cleanup()
    {
        _richTextBox.Dispose();
    }
}