using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.Common;

namespace Triplog.Media.Domain.MediaItems.Events;

public sealed record MediaItemCreatedDomainEvent(MediaItemId MediaItemId, OwnerId OwnerId, DateTime OccurredOnUtc) : IDomainEvent;