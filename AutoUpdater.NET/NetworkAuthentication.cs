using System;
using System.Net;

namespace AutoUpdaterDotNET
{
    /// <summary>
    ///     Provides credentials for Network Authentication
    /// </summary>
    public class NetworkAuthentication : IAuthentication, ICredentials
    {
        private string Username { get; }

        private string Password { get; }

        /// <summary>
        /// Initializes credentials for Network Authentication.
        /// </summary>
        /// <param name="username">Username to use for Network Authentication</param>
        /// <param name="password">Password to use for Network Authentication</param>
        public NetworkAuthentication(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Provides network credentials
        /// </summary>
        public NetworkCredential GetCredential(Uri uri, string authType) => new NetworkCredential(Username, Password);
    }
}
