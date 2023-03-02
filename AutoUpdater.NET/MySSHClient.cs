using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdaterDotNET
{
    public class MySSHClient
    {
        private string _Host;
        private int _Port;
        private string _UserName;
        private string _Password;

        public Action<object, DownloadProgressChangedEventArgs> DownloadProgressChanged { get; internal set; }

        public AsyncCompletedEventHandler DownloadFileCompleted;

        public MySSHClient(string host, string userName, string password, int port = 22)
        {
            _Host = host;
            _Port = port;
            _UserName = userName;
            _Password = password;
        }

        public byte[] GetFileContent(string remoteFilePath)
        {
            var tempFile = Path.GetTempFileName();
            using var client = new SftpClient(_Host, _Port <= 0 ? 22 : _Port, _UserName, _Password);

            try
            {
                client.Connect();

                using (var stream = File.Create(tempFile))
                {
                    client.DownloadFile(remoteFilePath, stream);
                }

                if (!File.Exists(tempFile))
                {
                    throw new Exception("The temporary file could not be written.");
                }

                FileInfo fileInfo = new FileInfo(tempFile);
                if (fileInfo.Length == 0)
                {
                    throw new Exception("The file is zero bytes.");
                }

                return File.ReadAllBytes(tempFile);
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in downloading to file [{tempFile}] from [{remoteFilePath}]");
                throw;
            }
            finally
            {
                client.Disconnect();

                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            return null;
        }

        public string GetFileContentAsString(string remoteFilePath, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var bytes = GetFileContent(remoteFilePath);
            return encoding.GetString(bytes);
        }

        public void DownloadFile(Uri uri, string destination)
        {
            string remoteFilePath = uri.AbsolutePath;

            using (var client = new SftpClient(_Host, _Port <= 0 ? 22 : _Port, _UserName, _Password))
            {
                try
                {
                    client.Connect();
                    RemoteFileName = uri.Segments.Last();
                    RemoteFileInfo = client.GetAttributes(remoteFilePath);

                    using (var saveFile = File.OpenWrite(destination))
                    {
                        client.DownloadFile(remoteFilePath, saveFile, OnDownloadProgressChanged);
                    }

                    DownloadFileCompleted.Invoke(null, 
                        new AsyncCompletedEventArgs(null, false, destination));
                }
                catch (Exception ex)
                {
                    DownloadFileCompleted.Invoke(null, new AsyncCompletedEventArgs(ex, false, null));
                }
                finally
                {
                    client.Disconnect();
                }
            }
        }

        private string RemoteFileName { get; set; }

        private SftpFileAttributes RemoteFileInfo { get; set; }

        private void OnDownloadProgressChanged(ulong bytesReceived)
        {
            int percent = (int)bytesReceived / (int)RemoteFileInfo.Size;
            var args = new DownloadProgressChangedEventArgs(
                (long)bytesReceived, 
                RemoteFileInfo.Size, 
                percent, 
                new { Name = RemoteFileName, Info = RemoteFileInfo });

            DownloadProgressChanged.Invoke(null, args);
        }
    }
}
