using MediatR;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Queries.GetMediaItemById;

public sealed class GetMediaItemByIdQueryHandler(IMediaItemQueries queries)
    : IRequestHandler<GetMediaItemByIdQuery, Result<MediaItemDto>>
{
    public async Task<Result<MediaItemDto>> Handle(GetMediaItemByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(request.MediaItemId, request.OwnerId, cancellationToken);
        return dto is null
            ? Result.Failure<MediaItemDto>(MediaItemErrors.NotFound)
            : Result.Success(dto);
    }
}