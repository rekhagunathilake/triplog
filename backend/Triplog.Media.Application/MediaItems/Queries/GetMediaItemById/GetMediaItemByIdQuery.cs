using MediatR;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Queries.GetMediaItemById;

public sealed record GetMediaItemByIdQuery(MediaItemId MediaItemId, OwnerId OwnerId)
    : IRequest<Result<MediaItemDto>>;