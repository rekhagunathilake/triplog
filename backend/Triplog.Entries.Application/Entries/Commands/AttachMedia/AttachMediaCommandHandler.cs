using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.AttachMedia;

public sealed class AttachMediaCommandHandler(
    IEntryRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<AttachMediaCommand, Result>
{
    public async Task<Result> Handle(AttachMediaCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry is null || entry.OwnerId != request.OwnerId)
            return Result.Failure(EntryErrors.NotFound);

        var result = entry.AttachMedia(request.MediaReferenceId, timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}