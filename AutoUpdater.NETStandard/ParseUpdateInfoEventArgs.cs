namespace AutoUpdater.NETStandard
{
    public class ParseUpdateInfoEventArgs
    {
        public ParseUpdateInfoEventArgs(string remoteData)
        {
            RemoteData = remoteData;
        }

        /// <summary>
        ///     Remote data received from the XML file.
        /// </summary>
        public string RemoteData { get; }

        /// <summary>
        ///      Set this object with values received from the update information file.
        /// </summary>
        public UpdateInfo UpdateInfo { get; set; }
    }
}