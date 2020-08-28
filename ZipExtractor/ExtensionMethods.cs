#if NET45
using System.IO.Compression;
#endif

namespace ZipExtractor
{
    public static class ExtensionMethod
    {
#if NET45
        public static bool IsDirectory(this ZipArchiveEntry entry)
        {
            return string.IsNullOrEmpty(entry.Name) && (entry.FullName.EndsWith("/") || entry.FullName.EndsWith(@"\"));
        }
#endif
    }
}
