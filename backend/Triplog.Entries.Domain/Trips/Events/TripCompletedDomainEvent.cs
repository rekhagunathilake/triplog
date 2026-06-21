using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Trips.Events;

public sealed record TripCompletedDomainEvent(TripId TripId, DateTime OccurredOnUtc) : IDomainEvent;
