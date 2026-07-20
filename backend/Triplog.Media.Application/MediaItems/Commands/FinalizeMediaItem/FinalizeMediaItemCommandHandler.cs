using MediatR;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Commands.FinalizeMediaItem
{
    public sealed class FinalizeMediaItemCommandHandler(
        IMediaItemRepository mediaItemRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider
        ) : IRequestHandler<FinalizeMediaItemCommand, Result>
    {
        public async Task<Result> Handle(FinalizeMediaItemCommand request, CancellationToken cancellationToken)
        {
            var mediaItem = await mediaItemRepository.GetByIdAsync(request.MediaItemId, cancellationToken);

            if (mediaItem is null)
                return Result.Failure(MediaItemErrors.NotFound);

            var result = mediaItem.Finalize(timeProvider.GetUtcNow().UtcDateTime);
            if (result.IsFailure)
                return result;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
