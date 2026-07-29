namespace Triplog.Contracts.Commands;

public sealed record FinalizeMediaCommand
{
    public required Guid MediaItemId { get; init; }
    public required Guid EntryId { get; init; }        // correlation back to the saga
    public required DateTime SentAtUtc { get; init; }
}