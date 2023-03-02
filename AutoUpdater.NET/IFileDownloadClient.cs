using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdaterDotNET
{
    public interface IFileDownloadClient
    {
        FileDownloadClientType Type { get; }


    }
}
