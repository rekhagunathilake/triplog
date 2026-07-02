using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Entries.Commands.CreateEntry;

public sealed class CreateEntryCommandHandler(
    ITripRepository tripRepository,
    IEntryRepository entryRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CreateEntryCommand, Result<EntryId>>
{
    public async Task<Result<EntryId>> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
    {
        // Cross-aggregate rule: Entry must reference an existing, non-archived Trip owned by the same user.
        var trip = await tripRepository.GetByIdAsync(request.TripId, cancellationToken);

        if (trip is null || trip.OwnerId != request.OwnerId)
            return Result.Failure<EntryId>(TripErrors.NotFound);
        
        if (trip.Status == TripStatus.Archived)
            return Result.Failure<EntryId>(TripErrors.IsArchived);

        var titleResult = EntryTitle.Create(request.Title);
        if (titleResult.IsFailure) return Result.Failure<EntryId>(titleResult.Error);

        var bodyResult = EntryBody.Create(request.Body);
        if (bodyResult.IsFailure) return Result.Failure<EntryId>(bodyResult.Error);

        Location? location = null;
        if (request.LocationName is not null)
        {
            var locationResult = Location.Create(request.LocationName, request.Latitude!.Value, request.Longitude!.Value);
            if (locationResult.IsFailure) return Result.Failure<EntryId>(locationResult.Error);
            location = locationResult.Value;
        }

        var entryResult = Entry.Create(
            request.TripId, request.OwnerId, titleResult.Value, bodyResult.Value,
            request.VisitedOn, location, timeProvider.GetUtcNow().UtcDateTime);
        if (entryResult.IsFailure) return Result.Failure<EntryId>(entryResult.Error);

        await entryRepository.AddAsync(entryResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(entryResult.Value.Id);
    }
}