using MediatR;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Commands.CreateMediaItem;

public sealed class CreateMediaItemCommandHandler(
    IMediaItemRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
    ) : IRequestHandler<CreateMediaItemCommand, Result<MediaItemId>>
{
    public async Task<Result<MediaItemId>> Handle(CreateMediaItemCommand request, CancellationToken cancellationToken)
    {
        // BlobKey is a raw string from API layer, was constructed there after MinIO upload
        var blobKey = BlobKey.Materialize(request.BlobKey);

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return Result.Failure<MediaItemId>(contentTypeResult.Error);

        var mediaItemResult = MediaItem.Create(
            ownerId: request.OwnerId,
            blobKey: blobKey,
            contentType: contentTypeResult.Value,
            sizeInBytes: request.SizeInBytes,
            originalFileName: request.OriginalFileName,
            createTimeUtc: timeProvider.GetUtcNow().UtcDateTime
        );

        if (mediaItemResult.IsFailure)
            return Result.Failure<MediaItemId>(mediaItemResult.Error);

        await repository.AddAsync(mediaItemResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mediaItemResult.Value.Id);
    }
}
