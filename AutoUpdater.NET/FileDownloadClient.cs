using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdaterDotNET
{
    public abstract class FileDownloadClient : IFileDownloadClient
    {
        /// <summary>
        /// The required constructor for the class.
        /// </summary>
        /// <param name="type"></param>
        public FileDownloadClient(FileDownloadClientType type)
        {
            this.Type = type;
        }


        public virtual FileDownloadClientType Type { get; private set; }


    }
}
