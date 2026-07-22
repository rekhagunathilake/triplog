using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems.Events;

namespace Triplog.Media.Domain.MediaItems;

public sealed class MediaItem : AggregateRoot<MediaItemId>
{

    public OwnerId OwnerId { get; private set; }

    public BlobKey BlobKey { get; private set; } = null!;

    public ContentType ContentType { get; private set; } = null!;

    public long SizeInBytes { get; private set; }

    public string OriginalFileName { get; private set; } = null!;

    public MediaItemStatus Status { get; private set; }


    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? FinalizedAtUtc { get; private set; }

    public DateTime? FailedAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    private MediaItem() { } // For EF Core

    private MediaItem(MediaItemId id, OwnerId ownerId, BlobKey blobKey, ContentType contentType, long sizeInBytes, string originalFileName, 
        MediaItemStatus status, DateTime createdAtUtc) : base(id)
    {
        OwnerId = ownerId;
        BlobKey = blobKey;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        OriginalFileName = originalFileName;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    public static Result<MediaItem> Create(
        OwnerId ownerId, BlobKey blobKey, ContentType contentType, long sizeInBytes, string originalFileName, DateTime createTimeUtc)
    {
        var mediaItem = new MediaItem(MediaItemId.NewId(), ownerId, blobKey, contentType, sizeInBytes, originalFileName, MediaItemStatus.Provisional, createTimeUtc);

        mediaItem.RaiseDomainEvent(new MediaItemCreatedDomainEvent(mediaItem.Id, ownerId, createTimeUtc));

        return Result.Success(mediaItem);
    }

    public Result Finalize(DateTime finalizeTimeUtc)
    {
        if (Status == MediaItemStatus.Finalized)
            return Result.Failure(MediaItemErrors.IsAlreadyFinalized);

        if (Status == MediaItemStatus.Failed)
            return Result.Failure(MediaItemErrors.InvalidStatusTransition);

        Status = MediaItemStatus.Finalized;
        FinalizedAtUtc = finalizeTimeUtc;

        RaiseDomainEvent(new MediaItemFinalizedDomainEvent(Id, finalizeTimeUtc));

        return Result.Success();
    }

    public Result Fail(string reason, DateTime failTimeUtc)
    {
        if (Status == MediaItemStatus.Finalized)
            return Result.Failure(MediaItemErrors.InvalidStatusTransition);

        if (Status == MediaItemStatus.Failed)
            return Result.Failure(MediaItemErrors.InvalidStatusTransition);

        Status = MediaItemStatus.Failed;
        FailedAtUtc = failTimeUtc;
        FailureReason = reason;

        RaiseDomainEvent(new MediaItemFailedDomainEvent(Id, reason, failTimeUtc));

        return Result.Success();
    }
}
