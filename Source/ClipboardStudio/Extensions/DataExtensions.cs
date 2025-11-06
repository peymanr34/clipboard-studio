using System;
using System.Linq;
using System.Threading.Tasks;
using ClipboardStudio.Data;
using ClipboardStudio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardStudio
{
    public static class DataExtensions
    {
        public static async Task<bool?> AddOrUpdateEntryAsync(this DatabaseContext context, DataPackageView package)
        {
            bool? result = null;

            if (package.Contains(StandardDataFormats.Text))
            {
                var text = await package.GetTextAsync();

                var entry = context.Entries
                    .FirstOrDefault(x => EF.Functions.Collate(x.Text, "NOCASE") == text);

                if (entry is null)
                {
                    entry = new Entry
                    {
                        Text = text,
                    };

                    context.Entries.Add(entry);
                    result = true;
                }
                else
                {
                    entry.ModifyDateUtc = DateTime.UtcNow;
                    result = false;
                }

                context.SaveChanges();
            }

            return result;
        }
    }
}
