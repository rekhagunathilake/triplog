using Triplog.Media.Domain.Abstractions;

namespace Triplog.Media.Domain.MediaItems.Events;

public sealed record MediaItemFinalizedDomainEvent(MediaItemId MediaItemId, DateTime OccurredOnUtc) : IDomainEvent;
