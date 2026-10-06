using Arrapa.ClinicOS.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Arrapa.ClinicOS.Persistence.Interceptors;

/// <summary>
/// Dispatches domain events raised by aggregate roots after SaveChanges succeeds. This runs after audit interception so
/// downstream handlers observe the fully persisted state.
/// </summary>
public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DispatchDomainEventsInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context).ConfigureAwait(false);
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var events = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await _publisher.Publish(domainEvent).ConfigureAwait(false);
        }
    }
}
