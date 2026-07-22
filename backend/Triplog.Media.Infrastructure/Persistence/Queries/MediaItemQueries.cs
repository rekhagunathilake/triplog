using Microsoft.EntityFrameworkCore;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Application.MediaItems.Queries.GetMediaItemById;
using Triplog.Media.Application.MediaItems.Queries.ListMediaItemsByOwner;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Infrastructure.Persistence.Queries;

public sealed class MediaItemQueries(TriplogMediaDbContext dbContext) : IMediaItemQueries
{
    public async Task<MediaItemDto?> GetByIdAsync(MediaItemId id, OwnerId ownerId, CancellationToken ct = default)
    {
        return await dbContext.MediaItems
            .AsNoTracking()
            .Where(e => e.Id == id && e.OwnerId == ownerId)
            .Select(e => new MediaItemDto(
                e.Id.Value,
                e.OwnerId.Value,
                e.BlobKey.Value,
                e.ContentType,
                e.SizeInBytes,
                e.OriginalFileName,
                e.Status,
                e.CreatedAtUtc,
                e.FinalizedAtUtc,
                e.FailedAtUtc,
                e.FailureReason
                ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<MediaItemSummaryDto>> ListByOwnerAsync(OwnerId ownerId, CancellationToken ct = default)
    {
        return await dbContext.MediaItems
            .AsNoTracking()
            .Where(e => e.OwnerId == ownerId)
            .Select(e => new MediaItemSummaryDto(
                e.Id.Value,
                e.OriginalFileName,
                e.SizeInBytes,
                e.Status,
                e.CreatedAtUtc
                ))
            .ToListAsync(ct);
    }
}
