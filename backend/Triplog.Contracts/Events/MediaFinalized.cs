namespace Triplog.Contracts.Events;

public sealed record MediaFinalized
{
    public required Guid MediaItemId { get; init; }
    public required Guid EntryId { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
}