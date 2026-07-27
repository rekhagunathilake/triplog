using MassTransit;
using MediatR;
using Triplog.Contracts.Events;
using Triplog.Entries.Domain.Entries.Events;

namespace Triplog.Entries.Infrastructure.Messaging.IntegrationHandlers;

public sealed class EntryPublishBeganIntegrationHandler(IPublishEndpoint bus)
    : INotificationHandler<EntryPublishBeganDomainEvent>
{
    public Task Handle(EntryPublishBeganDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return bus.Publish(new EntryPublishBegan
        {
            EntryId = domainEvent.EntryId.Value,
            OwnerId = domainEvent.OwnerId.Value,
            MediaReferenceIds = domainEvent.MediaReferenceIds
                .Select(id => id.Value)
                .ToList(),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        }, cancellationToken);
    }
}