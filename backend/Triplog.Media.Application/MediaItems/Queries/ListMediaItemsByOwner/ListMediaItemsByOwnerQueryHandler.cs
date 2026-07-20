using MediatR;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.Abstractions;

namespace Triplog.Media.Application.MediaItems.Queries.ListMediaItemsByOwner;

public sealed class ListMediaItemsByOwnerQueryHandler(IMediaItemQueries queries)
    : IRequestHandler<ListMediaItemsByOwnerQuery, Result<IReadOnlyList<MediaItemSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<MediaItemSummaryDto>>> Handle(
        ListMediaItemsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var items = await queries.ListByOwnerAsync(request.OwnerId, cancellationToken);
        return Result.Success(items);
    }
}