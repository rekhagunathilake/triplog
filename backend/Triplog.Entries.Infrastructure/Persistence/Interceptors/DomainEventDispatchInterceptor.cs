using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Infrastructure.Persistence.Interceptors;

public sealed class DomainEventDispatchInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
            return result;

        // Snapshot aggregates with domain events before clearing them
        // Some handlers may trigger more saveChanges calls that re-enter this method
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            // Snapshot the events, then clear them to avoid re-dispatching
            var events = aggregate.DomainEvents.ToList();

            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }
        }

        return result;
    }
}
