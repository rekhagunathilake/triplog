using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryPublishedDomainEvent(EntryId EntryId, DateTime OccurredOnUtc) : IDomainEvent;
