using System;
using System.IO;
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
                return Path.GetFileName(entry.Text);
            }

            return string.Empty;
        }
    }
}
