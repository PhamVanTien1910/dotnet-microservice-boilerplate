
using BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.EfCore.Interceptors;

public sealed class UpdatedAuditingInterceptor : SaveChangesInterceptor, IAuditInterceptor
{
    public UpdatedAuditingInterceptor()
    {
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleUpdatedAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        HandleUpdatedAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void HandleUpdatedAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<IModifiedAuditable>()
                                                   .Where(e => e.State == EntityState.Modified))
        {
            entry.Property(nameof(IModifiedAuditable.ModifiedAt)).CurrentValue = DateTime.UtcNow;
        }
    }
}
