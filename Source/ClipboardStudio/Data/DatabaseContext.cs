using ClipboardStudio.Data.Interceptors;
using ClipboardStudio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ClipboardStudio.Data
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> options)
        : DbContext(options)
    {
        public DbSet<Entry> Entries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder?.AddInterceptors(new DateTimeInterceptor());
        }
    }
}
