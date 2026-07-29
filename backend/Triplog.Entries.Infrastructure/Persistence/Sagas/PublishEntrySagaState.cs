using MassTransit;

namespace Triplog.Entries.Infrastructure.Persistence.Sagas;

public sealed class PublishEntrySagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }        // = EntryId (natural correlation)
    public int Version { get; set; }                // optimistic concurrency
    public string CurrentState { get; set; } = null!;

    // Business data captured from EntryPublishBegan
    public Guid EntryId { get; set; }
    public Guid OwnerId { get; set; }

    // Tally of media finalization results
    public int TotalMediaCount { get; set; }
    public int FinalizedCount { get; set; }
    public int FailedCount { get; set; }
    public string? FirstFailureReason { get; set; }

    // Lifecycle timestamps
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? FailedAtUtc { get; set; }
}