using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryPublishFailedDomainEvent(EntryId EntryId, string Reason, DateTime OccurredOnUtc) : IDomainEvent;
