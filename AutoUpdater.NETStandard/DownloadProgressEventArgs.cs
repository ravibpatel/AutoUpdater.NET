using System;

namespace AutoUpdater.NETStandard
{
    public class DownloadProgressEventArgs : EventArgs
    {
        public string Speed { get; set; }

        public string CompletedSize { get; set; }

        public int ProgressPercentage { get; set; }
    }
}
