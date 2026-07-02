using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.UpdateContent;

public sealed class UpdateContentCommandHandler(
    IEntryRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateContentCommand, Result>
{
    public async Task<Result> Handle(UpdateContentCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry is null || entry.OwnerId != request.OwnerId)
            return Result.Failure(EntryErrors.NotFound);

        var titleResult = EntryTitle.Create(request.Title);
        if (titleResult.IsFailure) return Result.Failure(titleResult.Error);

        var bodyResult = EntryBody.Create(request.Body);
        if (bodyResult.IsFailure) return Result.Failure(bodyResult.Error);

        Location? location = null;
        if (request.LocationName is not null)
        {
            var locationResult = Location.Create(request.LocationName, request.Latitude!.Value, request.Longitude!.Value);
            if (locationResult.IsFailure) return Result.Failure(locationResult.Error);
            location = locationResult.Value;
        }

        var result = entry.UpdateContent(
            titleResult.Value, bodyResult.Value, location, request.VisitedOn,
            timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}