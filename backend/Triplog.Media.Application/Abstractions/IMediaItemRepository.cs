using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.Abstractions;

public interface IMediaItemRepository
{
    Task AddAsync(MediaItem mediaItem, CancellationToken ct = default);

    Task<MediaItem?> GetByIdAsync(MediaItemId id, CancellationToken ct = default);
}
