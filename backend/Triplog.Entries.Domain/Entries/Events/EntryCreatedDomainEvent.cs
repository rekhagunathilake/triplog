using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryCreatedDomainEvent(EntryId EntryId, TripId TripId, OwnerId OwnerId, DateTime OccurredOnUtc) : IDomainEvent;
