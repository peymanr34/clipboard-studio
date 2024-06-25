using System;
using System.IO;
using System.Web;
using ClipboardStudio.Data.Models;

namespace ClipboardStudio
{
    public static class EntryExtensions
    {
        public static string GetTitle(this Entry entry)
        {
            if (Uri.TryCreate(entry.Text, UriKind.Absolute, out var uri))
            {
                var path = HttpUtility.UrlDecode(uri.AbsolutePath);

                if (Path.HasExtension(path))
                {
                    return Path.GetFileName(path);
                }
            }

            return string.Empty;
        }
    }
}
