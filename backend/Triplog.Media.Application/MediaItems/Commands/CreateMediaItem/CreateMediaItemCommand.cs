using MediatR;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Commands.CreateMediaItem;

public sealed record CreateMediaItemCommand(
    OwnerId OwnerId,
    string BlobKey,
    string ContentType,
    long SizeInBytes,
    string OriginalFileName
    ) : IRequest<Result<MediaItemId>>;
