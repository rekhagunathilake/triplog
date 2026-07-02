using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Commands.UpdateTripDetails;

public sealed class UpdateTripDetailsCommandHandler(
    ITripRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateTripDetailsCommand, Result>
{
    public async Task<Result> Handle(UpdateTripDetailsCommand request, CancellationToken cancellationToken)
    {
        var trip = await repository.GetByIdAsync(request.TripId, cancellationToken);
        if (trip is null || trip.OwnerId != request.OwnerId)
            return Result.Failure(TripErrors.NotFound);

        var titleResult = TripTitle.Create(request.Title);
        if (titleResult.IsFailure) return Result.Failure(titleResult.Error);

        var datesResult = DateRange.Create(request.StartDate, request.EndDate);
        if (datesResult.IsFailure) return Result.Failure(datesResult.Error);

        var result = trip.UpdateDetails(
            titleResult.Value, request.Description, datesResult.Value,
            timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}