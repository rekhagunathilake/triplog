using MassTransit;
using MediatR;
using Triplog.Contracts.Commands;
using Triplog.Contracts.Events;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Application.MediaItems.Commands.FailMediaItem;
using Triplog.Media.Application.MediaItems.Commands.FinalizeMediaItem;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Infrastructure.Messaging.Consumers;

public sealed class FinalizeMediaCommandConsumer(
    ISender mediator,
    IMediaItemRepository mediaItemRepository,
    IObjectStorage objectStorage,
    TimeProvider timeProvider)
    : IConsumer<FinalizeMediaCommand>
{
    private const string Bucket = "triplog-media";

    public async Task Consume(ConsumeContext<FinalizeMediaCommand> context)
    {
        var command = context.Message;
        var mediaItemId = new MediaItemId(command.MediaItemId);
        var cancellationToken = context.CancellationToken;

        // 1. Load the media item to get its blobkey (repo is write-side, no owner filter)
        var mediaItem = await mediaItemRepository.GetByIdAsync(mediaItemId, cancellationToken);

        if (mediaItem == null)
        {
            await PublishFailedAsync(context, command, "MediaItem not found.");
            return;
        }

        // 2. Verify blob actually exists in MinIO (the v1 "processing" - see ADR)
        var blobExists = await objectStorage.ObjectExistsAsync(Bucket, mediaItem.BlobKey.Value, cancellationToken);

        if (!blobExists) 
        {
            // Transition aggregates -> Failed via Application command (saves + raises domain event)
            await mediator.Send(
                new FailMediaItemCommand(mediaItemId, "Blob not found in storage."), cancellationToken);

            await PublishFailedAsync(context, command, "Blob not found in storage.");
            return;
        }

        // 3. Blob exists - transition aggregate -> Finalized
        var finalizeResult = await mediator.Send(
            new FinalizeMediaItemCommand(mediaItemId), cancellationToken);

        if (finalizeResult.IsSuccess)
        {
            await context.Publish(new MediaFinalized
            {
                MediaItemId = command.MediaItemId,
                EntryId = command.EntryId,
                OccurredOnUtc = timeProvider.GetUtcNow().UtcDateTime
            }, cancellationToken);
        }
        else
        {
            // Aggregate rejected the transition (already Finalized, already Failed, etc)
            await PublishFailedAsync(context, command, finalizeResult.Error.Message);
        }
    }

    private async Task PublishFailedAsync(
        ConsumeContext context, FinalizeMediaCommand command, string reason)
    {
        await context.Publish(new MediaFinalizationFailed
        {
            MediaItemId = command.MediaItemId,
            EntryId = command.EntryId,
            Reason = reason,
            OccurredOnUtc = timeProvider.GetUtcNow().UtcDateTime,
        }, context.CancellationToken);
    }
}
