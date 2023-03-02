using System;
using System.Net;

namespace AutoUpdaterDotNET
{
    /// <inheritdoc />
    public class MyWebClient : WebClient
    {
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        ///     Response Uri after any redirects.
        /// </summary>
        public Uri ResponseUri;

        /// <inheritdoc />
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse webResponse = base.GetWebResponse(request, result);
            ResponseUri = webResponse.ResponseUri;
            return webResponse;
        }

        protected override void OnDownloadProgressChanged(System.Net.DownloadProgressChangedEventArgs e)
        {
            EventHandler<DownloadProgressChangedEventArgs> handler = DownloadProgressChanged;
            if (handler != null)
            {
                handler(this, new DownloadProgressChangedEventArgs(
                    e.BytesReceived,
                    e.TotalBytesToReceive,
                    e.ProgressPercentage,
                    e.UserState));
            }
            // base.OnDownloadProgressChanged(e);
        }
    }
}
