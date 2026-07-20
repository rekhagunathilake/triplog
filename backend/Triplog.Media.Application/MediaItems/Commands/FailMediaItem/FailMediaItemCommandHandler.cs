using MediatR;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Commands.FailMediaItem;

public sealed class FailMediaItemCommandHandler(
    IMediaItemRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<FailMediaItemCommand, Result>
{
    public async Task<Result> Handle(FailMediaItemCommand request, CancellationToken cancellationToken)
    {
        var mediaItem = await repository.GetByIdAsync(request.MediaItemId, cancellationToken);
        if (mediaItem is null)
            return Result.Failure(MediaItemErrors.NotFound);

        var result = mediaItem.Fail(request.Reason, timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
