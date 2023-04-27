using System.Net;

namespace AutoUpdaterDotNET;

/// <summary>
///     Provides credentials for Network Authentication.
/// </summary>
public class NetworkAuthentication : IAuthentication
{
    /// <summary>
    ///     Initializes credentials for Network Authentication.
    /// </summary>
    /// <param name="username">Username to use for Network Authentication</param>
    /// <param name="password">Password to use for Network Authentication</param>
    public NetworkAuthentication(string username, string password)
    {
        Username = username;
        Password = password;
    }

    private string Username { get; }

    private string Password { get; }

    /// <inheritdoc />
    public void Apply(ref MyWebClient webClient)
    {
        webClient.Credentials = new NetworkCredential(Username, Password);
    }
}