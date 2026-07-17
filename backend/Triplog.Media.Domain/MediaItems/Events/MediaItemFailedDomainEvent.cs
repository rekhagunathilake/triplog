using Triplog.Media.Domain.Abstractions;

namespace Triplog.Media.Domain.MediaItems.Events;

public sealed record MediaItemFailedDomainEvent(MediaItemId MediaItemId, string Reason, DateTime OccurredOnUtc) : IDomainEvent;
