using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Domain.Trips.Events;

public sealed record TripCreatedDomainEvent(TripId TripId, OwnerId OwnerId, DateTime OccurredOnUtc) : IDomainEvent;
