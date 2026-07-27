namespace Triplog.Contracts.Events;

public sealed record EntryPublishFailed
{
    public required Guid EntryId { get; init; }
    public required Guid OwnerId { get; init; }
    public required string Reason { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
}