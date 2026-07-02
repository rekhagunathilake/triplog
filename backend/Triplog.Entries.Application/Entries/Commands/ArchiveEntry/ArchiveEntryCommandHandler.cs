using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.ArchiveEntry;

public sealed class ArchiveEntryCommandHandler(
    IEntryRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<ArchiveEntryCommand, Result>
{
    public async Task<Result> Handle(ArchiveEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry is null || entry.OwnerId != request.OwnerId)
            return Result.Failure(EntryErrors.NotFound);

        var result = entry.Archive(timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}