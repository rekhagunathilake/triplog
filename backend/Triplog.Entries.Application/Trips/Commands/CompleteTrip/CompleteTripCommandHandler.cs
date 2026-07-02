using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Commands.CompleteTrip;

public sealed class CompleteTripCommandHandler(
    ITripRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteTripCommand, Result>
{
    public async Task<Result> Handle(CompleteTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await repository.GetByIdAsync(request.TripId, cancellationToken);
        if (trip is null || trip.OwnerId != request.OwnerId)
            return Result.Failure(TripErrors.NotFound);

        var result = trip.Complete(timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}