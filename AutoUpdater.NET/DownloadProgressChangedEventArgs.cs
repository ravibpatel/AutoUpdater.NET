using System.ComponentModel;

namespace AutoUpdaterDotNET
{
    //
    // Summary:
    //     Provides data for the SSH client DownloadProgressChanged event.
    public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public DownloadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive,
            int progressPercentage, object userState)
                : base(progressPercentage, userState)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }


        //
        // Summary:
        //     Gets the number of bytes received.
        //
        // Returns:
        //     An System.Int64 value that indicates the number of bytes received.
        public long BytesReceived { get; }
        //
        // Summary:
        //     Gets the total number of bytes in a System.Net.WebClient data download operation.
        //
        // Returns:
        //     An System.Int64 value that indicates the number of bytes that will be received.
        public long TotalBytesToReceive { get; }
    }
}
