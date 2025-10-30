
using BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.EfCore.Interceptors;

public sealed class CreatedAuditingInterceptor : SaveChangesInterceptor, IAuditInterceptor
{
    public CreatedAuditingInterceptor()
    {
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleCreatedAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        HandleCreatedAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void HandleCreatedAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker
                                     .Entries<ICreatedAuditable>()
                                     .Where(e => e.State == EntityState.Added))
        {
            var property = entry.Property(nameof(ICreatedAuditable.CreatedAt));

            if (property.CurrentValue is not DateTime dt || dt == default)
            {
                property.CurrentValue = DateTime.UtcNow;
            }
        }
    }

}
