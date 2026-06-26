using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryContentUpdatedDomainEvent(EntryId EntryId, DateTime OccurredOnUtc) : IDomainEvent;
