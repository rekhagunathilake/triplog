namespace Triplog.Contracts.Events;

public sealed record EntryPublishBegan
{
    public required Guid EntryId { get; init; }
    public required Guid OwnerId { get; init; }
    public required IReadOnlyList<Guid> MediaReferenceIds { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
}