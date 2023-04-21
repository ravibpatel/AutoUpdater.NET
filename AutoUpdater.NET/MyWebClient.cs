using System;
using System.Net;

namespace AutoUpdaterDotNET
{
    /// <inheritdoc />
    public class MyWebClient : WebClient
    {
        /// <summary>
        ///     Response Uri after any redirects.
        /// </summary>
        public Uri ResponseUri;

        /// <summary>
        /// Container of cookies.
        /// </summary>
        public CookieContainer CookieContainer;

        private bool _useCookies;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="useCookies"></param>
        public MyWebClient(bool useCookies)
        {
            _useCookies = useCookies;
        }

        /// <inheritdoc />
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse webResponse = base.GetWebResponse(request, result);
            ResponseUri = webResponse.ResponseUri;
            
            if (_useCookies)
            {
                string setCookieHeader = webResponse.Headers[HttpResponseHeader.SetCookie];

                //do something if needed to parse out the cookie.
                if (setCookieHeader != null)
                {
                    Cookie cookie = new Cookie(); //create cookie
                    CookieContainer.SetCookies(request.RequestUri, setCookieHeader);
                }
            }

            return webResponse;
        }

        /// <inheritdoc />
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (_useCookies && request is HttpWebRequest)
                (request as HttpWebRequest).CookieContainer = CookieContainer;

            HttpWebRequest httpRequest = (HttpWebRequest)request;
            httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return httpRequest;
        }
    }
}
