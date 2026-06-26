using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Domain.Entries.Events;

public sealed record EntryPublishBeganDomainEvent(
    EntryId EntryId,
    OwnerId OwnerId,
    IReadOnlyList<MediaReferenceId> MediaReferenceIds,
    DateTime OccurredOnUtc) : IDomainEvent;
