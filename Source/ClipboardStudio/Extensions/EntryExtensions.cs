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
            if (Uri.IsWellFormedUriString(entry.Text, UriKind.Absolute) &&
                Path.HasExtension(entry.Text))
            {
                var uri = new Uri(entry.Text);
                var path = HttpUtility.UrlDecode(uri.AbsolutePath);

                return Path.GetFileName(path);
            }

            return string.Empty;
        }
    }
}
