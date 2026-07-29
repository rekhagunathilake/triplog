using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryPublishedDomainEvent(EntryId EntryId, OwnerId OwnerId, DateTime OccurredOnUtc) : IDomainEvent;
