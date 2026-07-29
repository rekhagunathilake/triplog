using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Triplog.Contracts.Commands;
using Triplog.Contracts.Events;
using Triplog.Entries.Application.Entries.Commands.CompletePublish;
using Triplog.Entries.Application.Entries.Commands.FailPublish;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Infrastructure.Persistence.Sagas;

public sealed class PublishEntrySaga : MassTransitStateMachine<PublishEntrySagaState>
{
    public State Publishing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<EntryPublishBegan> EntryPublishBegan { get; private set; } = null!;
    public Event<MediaFinalized> MediaFinalized { get; private set; } = null!;
    public Event<MediaFinalizationFailed> MediaFinalizationFailed { get; private set; } = null!;

    public PublishEntrySaga()
    {
        InstanceState(x => x.CurrentState);

        // Correlate all three events by EntryId
        Event(() => EntryPublishBegan, x => x.CorrelateById(ctx => ctx.Message.EntryId));
        Event(() => MediaFinalized, x => x.CorrelateById(ctx => ctx.Message.EntryId));
        Event(() => MediaFinalizationFailed, x => x.CorrelateById(ctx => ctx.Message.EntryId));

        // Initial: saga starts when an EntryPublishBegan arrives
        Initially(
            When(EntryPublishBegan)
                .Then(context =>
                {
                    context.Saga.EntryId = context.Message.EntryId;
                    context.Saga.OwnerId = context.Message.OwnerId;
                    context.Saga.TotalMediaCount = context.Message.MediaReferenceIds.Count;
                    context.Saga.StartedAtUtc = DateTime.UtcNow;
                })
                .ThenAsync(SendFinalizeCommandsAsync)
                .TransitionTo(Publishing));

        // Publishing: waiting for media-api to respond to each FinalizeMediaCommand
        During(Publishing,
            When(MediaFinalized)
                .Then(context => context.Saga.FinalizedCount++)
                .IfElse(AllResponsesReceivedAndAllSucceeded,
                    complete => complete
                        .ThenAsync(CompletePublishAsync)
                        .TransitionTo(Completed),
                    other => other.If(AllResponsesReceived,
                        fail => fail
                            .ThenAsync(FailPublishAsync)
                            .TransitionTo(Failed))),

            When(MediaFinalizationFailed)
                .Then(context =>
                {
                    context.Saga.FailedCount++;
                    context.Saga.FirstFailureReason ??= context.Message.Reason;
                })
                .If(AllResponsesReceived,
                    fail => fail
                        .ThenAsync(FailPublishAsync)
                        .TransitionTo(Failed)));

        During(Failed,
            When(EntryPublishBegan)
            .Then(context =>
            {
                // Reset counters for the retry
                context.Saga.FinalizedCount = 0;
                context.Saga.FailedCount = 0;
                context.Saga.FirstFailureReason = null;
                context.Saga.FailedAtUtc = null;
                context.Saga.TotalMediaCount = context.Message.MediaReferenceIds.Count;
                context.Saga.StartedAtUtc = DateTime.UtcNow;
            })
            .ThenAsync(SendFinalizeCommandsAsync)
            .TransitionTo(Publishing));

        // Remove saga row when it reaches a terminal state
        SetCompletedWhenFinalized();
    }

    // ---- Guards ----

    private static bool AllResponsesReceived(BehaviorContext<PublishEntrySagaState> context) =>
        context.Saga.FinalizedCount + context.Saga.FailedCount >= context.Saga.TotalMediaCount;

    private static bool AllResponsesReceivedAndAllSucceeded(BehaviorContext<PublishEntrySagaState> context) =>
        AllResponsesReceived(context) && context.Saga.FailedCount == 0;

    // ---- Side-effect actions ----

    private static async Task SendFinalizeCommandsAsync(
        BehaviorContext<PublishEntrySagaState, EntryPublishBegan> context)
    {
        var sentAt = DateTime.UtcNow;
        foreach (var mediaId in context.Message.MediaReferenceIds)
        {
            await context.Publish(new FinalizeMediaCommand
            {
                MediaItemId = mediaId,
                EntryId = context.Message.EntryId,
                SentAtUtc = sentAt,
            });
        }
    }

    private static async Task CompletePublishAsync(BehaviorContext<PublishEntrySagaState> context)
    {
        var sender = context.GetPayload<IServiceProvider>().GetRequiredService<ISender>();
        await sender.Send(new CompletePublishCommand(new EntryId(context.Saga.EntryId)));
        context.Saga.CompletedAtUtc = DateTime.UtcNow;
    }

    private static async Task FailPublishAsync(BehaviorContext<PublishEntrySagaState> context)
    {
        var sender = context.GetPayload<IServiceProvider>().GetRequiredService<ISender>();
        await sender.Send(new FailPublishCommand(
            new EntryId(context.Saga.EntryId),
            context.Saga.FirstFailureReason ?? "Media finalization failed."));
        context.Saga.FailedAtUtc = DateTime.UtcNow;
    }
}