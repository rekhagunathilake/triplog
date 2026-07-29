using MassTransit;
using MediatR;
using Triplog.Contracts.Events;
using Triplog.Entries.Domain.Entries.Events;

namespace Triplog.Entries.Infrastructure.Messaging.IntegrationHandlers;

public sealed class EntryPublishedIntegrationHandler(IPublishEndpoint bus)
    : INotificationHandler<EntryPublishedDomainEvent>
{
    public Task Handle(EntryPublishedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return bus.Publish(new EntryPublished
        {
            EntryId = domainEvent.EntryId.Value,
            OwnerId = domainEvent.OwnerId.Value,
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        }, cancellationToken);
    }
}