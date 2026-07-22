using Microsoft.EntityFrameworkCore;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Infrastructure.Persistence.Repositories;

public sealed class MediaItemRepository(TriplogMediaDbContext dbContext) : IMediaItemRepository
{
    public async Task AddAsync(MediaItem mediaItem, CancellationToken ct = default) => await dbContext.MediaItems.AddAsync(mediaItem, ct).AsTask();

    public async Task<MediaItem?> GetByIdAsync(MediaItemId id, CancellationToken ct = default) =>
        await dbContext.MediaItems
        .FirstOrDefaultAsync(e => e.Id == id, ct);
}
