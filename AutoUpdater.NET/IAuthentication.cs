namespace AutoUpdaterDotNET
{

    /// <summary>
    ///     Interface for authentication
    /// </summary>
    public interface IAuthentication
    {
        /// <summary>
        ///     Apply the authentication to webclient.
        /// </summary>
        /// <param name="webClient">WebClient for which you want to use this authentication method.</param>
        void Apply(ref MyWebClient webClient);
    }
}
