using Triplog.Media.Application.MediaItems.Queries.GetMediaItemById;
using Triplog.Media.Application.MediaItems.Queries.ListMediaItemsByOwner;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.Abstractions;

public interface IMediaItemQueries
{
    Task<MediaItemDto?> GetByIdAsync(MediaItemId id, OwnerId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<MediaItemSummaryDto>> ListByOwnerAsync(OwnerId ownerId, CancellationToken ct = default);
}
