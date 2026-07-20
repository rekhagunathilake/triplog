using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Queries.ListMediaItemsByOwner;

public sealed record MediaItemSummaryDto(
    Guid Id,
    string OriginalFileName,
    long SizeBytes,
    MediaItemStatus Status,
    DateTime CreatedAtUtc);