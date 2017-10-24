using System;
using AutoUpdaterDotNET;

namespace AutoUpdaterTest
{
    public class JsonUpdateInfo : IUpdateAppInfo
    {
        public string ChangeLogURL { get; set; }
        public string DownloadURL { get; set; }
        public Version CurrentVersion { get; set; }
        public bool Mandatory { get; set; }
    }
}
