namespace Triplog.Contracts.Events;

public sealed record MediaFinalizationFailed
{
    public required Guid MediaItemId { get; init; }
    public required Guid EntryId { get; init; }
    public required string Reason { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
}