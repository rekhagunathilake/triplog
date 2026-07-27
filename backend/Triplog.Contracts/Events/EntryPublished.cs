namespace Triplog.Contracts.Events;

public sealed record EntryPublished
{
    public required Guid EntryId { get; init; }
    public required Guid OwnerId { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
}