using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.CompletePublish;

public sealed class CompletePublishCommandHandler(
    IEntryRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<CompletePublishCommand, Result>
{
    public async Task<Result> Handle(CompletePublishCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry is null)
            return Result.Failure(EntryErrors.NotFound);

        var result = entry.CompletePublish(timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}