using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryMediaAttachedDomainEvent(EntryId EntryId, MediaReferenceId MediaReferenceId, DateTime OccurredOnUtc) : IDomainEvent;
