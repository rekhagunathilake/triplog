using MassTransit;
using MediatR;
using Triplog.Contracts.Events;
using Triplog.Entries.Domain.Entries.Events;

namespace Triplog.Entries.Infrastructure.Messaging.IntegrationHandlers;

public sealed class EntryPublishFailedIntegrationHandler(IPublishEndpoint bus)
    : INotificationHandler<EntryPublishFailedDomainEvent>
{
    public Task Handle(EntryPublishFailedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return bus.Publish(new EntryPublishFailed
        {
            EntryId = domainEvent.EntryId.Value,
            OwnerId = domainEvent.OwnerId.Value,
            Reason = domainEvent.Reason,
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        }, cancellationToken);
    }
}