using System;
using System.Threading;
using System.Threading.Tasks;
using ClipboardStudio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClipboardStudio.Data.Interceptors
{
    public class DateTimeInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            SetDateTimeValues(eventData?.Context);
            return base.SavingChanges(eventData!, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            SetDateTimeValues(eventData?.Context);
            return base.SavingChangesAsync(eventData!, result, cancellationToken);
        }

        private static void SetDateTimeValues(DbContext context)
        {
            if (context is null)
            {
                return;
            }

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity entity)
                {
                    var now = DateTime.UtcNow;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedDateUtc = now;
                        entity.ModifyDateUtc = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.ModifyDateUtc = now;
                    }
                }
            }
        }
    }
}