using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdaterDotNET
{
    public class SFTPFileDownloadClient : FileDownloadClient
    {
        private MySSHClient sshClient;

        public SFTPFileDownloadClient(MySSHClient client) : base(FileDownloadClientType.SFTPClient)
        {
            this.sshClient = client;
        }

        public SFTPFileDownloadClient(string host, string userName, string password, int port = 22) : base(FileDownloadClientType.SFTPClient)
        {
            sshClient = new MySSHClient(host, userName, password, port);
        }



    }
}
