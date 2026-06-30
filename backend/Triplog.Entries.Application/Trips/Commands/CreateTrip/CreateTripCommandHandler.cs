using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Commands.CreateTrip;

public sealed class CreateTripCommandHandler(
    ITripRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CreateTripCommand, Result<TripId>>
{
    public async Task<Result<TripId>> Handle(
        CreateTripCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var titleResult = TripTitle.Create(request.TripTitle);
        if (titleResult.IsFailure)
            return Result.Failure<TripId>(titleResult.Error);

        var datesResult = DateRange.Create(request.StartDate, request.EndDate);
        if (datesResult.IsFailure)
            return Result.Failure<TripId>(datesResult.Error);

        var tripResult = Trip.Create(
            request.UserId,
            titleResult.Value,
            request.Description,
            datesResult.Value,
            nowUtc);

        if (tripResult.IsFailure)
            return Result.Failure<TripId>(tripResult.Error);

        await repository.AddAsync(tripResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tripResult.Value.Id);
    }
}
