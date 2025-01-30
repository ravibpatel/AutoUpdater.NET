﻿using System;
using System.Net;

namespace AutoUpdaterDotNET;

/// <inheritdoc />
public class MyWebClient : WebClient
{
    /// <summary>
    ///     Response Uri after any redirects.
    /// </summary>
    public Uri ResponseUri;

    /// <summary>
    ///     Set Request timeout in milliseconds
    /// </summary>
    public int Timeout;

    /// <inheritdoc />
    protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
    {
        WebResponse webResponse = base.GetWebResponse(request, result);
        ResponseUri = webResponse.ResponseUri;
        return webResponse;
    }

    /// <inheritdoc />
    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest webRequest = base.GetWebRequest(address);
        webRequest.Timeout = Timeout;
        return webRequest;
    }
}