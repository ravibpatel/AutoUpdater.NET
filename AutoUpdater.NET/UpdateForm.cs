using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using AutoUpdaterDotNET.ChangelogViewers;

namespace AutoUpdaterDotNET;

internal sealed partial class UpdateForm : Form
{
    private readonly UpdateInfoEventArgs _args;
    private IChangelogViewer _changelogViewer;

    public UpdateForm(UpdateInfoEventArgs args)
    {
        _args = args;
        InitializeComponent();
        InitializeChangelogViewer();
        TopMost = AutoUpdater.TopMost;

        if (AutoUpdater.Icon != null)
        {
            pictureBoxIcon.Image = AutoUpdater.Icon;
            Icon = Icon.FromHandle(AutoUpdater.Icon.GetHicon());
        }

        buttonSkip.Visible = AutoUpdater.ShowSkipButton;
        buttonRemindLater.Visible = AutoUpdater.ShowRemindLaterButton;
        var resources = new ComponentResourceManager(typeof(UpdateForm));
        Text = string.Format(resources.GetString("$this.Text", CultureInfo.CurrentCulture)!,
            AutoUpdater.AppTitle, _args.CurrentVersion);
        labelUpdate.Text = string.Format(resources.GetString("labelUpdate.Text", CultureInfo.CurrentCulture)!,
            AutoUpdater.AppTitle);
        labelDescription.Text =
            string.Format(resources.GetString("labelDescription.Text", CultureInfo.CurrentCulture)!,
                AutoUpdater.AppTitle, _args.CurrentVersion, _args.InstalledVersion);

        if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.Forced)
        {
            ControlBox = false;
        }
    }

    private void InitializeChangelogViewer()
    {
        if (string.IsNullOrEmpty(_args.ChangelogURL) && string.IsNullOrEmpty(_args.ChangelogText))
        {
            var reduceHeight = labelReleaseNotes.Height + pnlChangelog.Height;
            labelReleaseNotes.Hide();
            Height -= reduceHeight;
            return;
        }

        // Create and configure the new viewer
        _changelogViewer = ChangelogViewerFactory.Create(AutoUpdater.ChangelogViewerType);
        var viewerControl = _changelogViewer.Control;
        viewerControl.Dock = DockStyle.Fill;
        pnlChangelog.Controls.Add(viewerControl);

        if (!string.IsNullOrEmpty(_args.ChangelogText))
        {
            _changelogViewer.LoadContent(_args.ChangelogText);
        }
        else if (_changelogViewer.SupportsUrl && !string.IsNullOrEmpty(_args.ChangelogURL))
        {
            _changelogViewer.LoadUrl(_args.ChangelogURL);
        }
        else
        {
            labelReleaseNotes.Hide();
            viewerControl.Hide();
            Height -= (labelReleaseNotes.Height + viewerControl.Height);
        }
    }

    private void UpdateFormLoad(object sender, EventArgs e)
    {
        var labelSize = new Size(pnlChangelog.Width, 0);
        labelDescription.MaximumSize = labelUpdate.MaximumSize = labelSize;
    }

    private void ButtonUpdateClick(object sender, EventArgs e)
    {
        if (AutoUpdater.OpenDownloadPage)
        {
            
            var processStartInfo = new ProcessStartInfo(_args.DownloadURL);
#if NETCOREAPP
            // for .NET Core, UseShellExecute must be set to true, otherwise
            // opening URLs via Process.Start() fails 
            processStartInfo.UseShellExecute = true;
#endif
            Process.Start(processStartInfo);

            DialogResult = DialogResult.OK;
        }
        else
        {
            if (AutoUpdater.DownloadUpdate(_args))
            {
                DialogResult = DialogResult.OK;
            }
        }
    }

    private void ButtonSkipClick(object sender, EventArgs e)
    {
        AutoUpdater.PersistenceProvider.SetSkippedVersion(new Version(_args.CurrentVersion));
    }

    private void ButtonRemindLaterClick(object sender, EventArgs e)
    {
        if (AutoUpdater.LetUserSelectRemindLater)
        {
            using var remindLaterForm = new RemindLaterForm();
            DialogResult dialogResult = remindLaterForm.ShowDialog(this);

            switch (dialogResult)
            {
                case DialogResult.OK:
                    AutoUpdater.RemindLaterTimeSpan = remindLaterForm.RemindLaterFormat;
                    AutoUpdater.RemindLaterAt = remindLaterForm.RemindLaterAt;
                    break;
                case DialogResult.Abort:
                    ButtonUpdateClick(sender, e);
                    return;
                default:
                    return;
            }
        }

        AutoUpdater.PersistenceProvider.SetSkippedVersion(null);

        DateTime remindLaterDateTime = AutoUpdater.RemindLaterTimeSpan switch
        {
            RemindLaterFormat.Days => DateTime.Now + TimeSpan.FromDays(AutoUpdater.RemindLaterAt),
            RemindLaterFormat.Hours => DateTime.Now + TimeSpan.FromHours(AutoUpdater.RemindLaterAt),
            RemindLaterFormat.Minutes => DateTime.Now + TimeSpan.FromMinutes(AutoUpdater.RemindLaterAt),
            _ => DateTime.Now
        };

        AutoUpdater.PersistenceProvider.SetRemindLater(remindLaterDateTime);
        AutoUpdater.SetTimer(remindLaterDateTime);

        DialogResult = DialogResult.Cancel;
    }

    private void UpdateForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        AutoUpdater.Running = false;
    }

    private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.Forced)
        {
            AutoUpdater.Exit();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _changelogViewer?.Cleanup();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}