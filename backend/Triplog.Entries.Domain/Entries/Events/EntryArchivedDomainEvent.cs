using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryArchivedDomainEvent(EntryId EntryId, DateTime OccurredOnUtc) : IDomainEvent;
