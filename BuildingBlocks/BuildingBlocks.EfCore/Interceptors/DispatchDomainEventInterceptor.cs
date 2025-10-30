using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.EfCore.Interceptors;

public sealed class DispatchDomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private List<IDomainEvent>? _domainEvents;

    public DispatchDomainEventInterceptor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            _domainEvents = GetDomainEvents(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _domainEvents = GetDomainEvents(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_domainEvents is { Count: > 0 })
        {
            DispatchDomainEventsAsync(_domainEvents, eventData.Context!, CancellationToken.None)
                .GetAwaiter().GetResult();
        }

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_domainEvents is { Count: > 0 })
        {
            await DispatchDomainEventsAsync(_domainEvents, eventData.Context!, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static List<IDomainEvent> GetDomainEvents(DbContext context)
    {
        var aggregateRoots = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents)
            .ToList();

        aggregateRoots.ForEach(x => x.ClearDomainEvents());

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(List<IDomainEvent> domainEvents, DbContext context, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
