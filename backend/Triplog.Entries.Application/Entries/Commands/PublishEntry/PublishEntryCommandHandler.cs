using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.PublishEntry;

public sealed class PublishEntryCommandHandler(
    IEntryRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<PublishEntryCommand, Result>
{
    public async Task<Result> Handle(PublishEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry is null || entry.OwnerId != request.OwnerId)
            return Result.Failure(EntryErrors.NotFound);

        var result = entry.BeginPublish(timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}