using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Queries.GetMediaItemById;

public sealed record MediaItemDto(
    Guid Id,
    Guid OwnerId,
    string BlobKey,
    ContentType ContentType,
    long SizeBytes,
    string OriginalFileName,
    MediaItemStatus Status,
    DateTime CreatedAtUtc,
    DateTime? FinalizedAtUtc,
    DateTime? FailedAtUtc,
    string? FailureReason);