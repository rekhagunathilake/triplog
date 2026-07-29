using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryPublishFailedDomainEvent(EntryId EntryId, OwnerId OwnerId, string Reason, DateTime OccurredOnUtc) : IDomainEvent;
