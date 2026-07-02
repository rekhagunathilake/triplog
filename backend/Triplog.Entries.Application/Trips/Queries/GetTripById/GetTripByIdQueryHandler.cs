using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Queries.GetTripById;

public sealed class GetTripByIdQueryHandler(ITripQueries queries)
    : IRequestHandler<GetTripByIdQuery, Result<TripDto>>
{
    public async Task<Result<TripDto>> Handle(GetTripByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(request.TripId, request.OwnerId, cancellationToken);
        return dto is null
            ? Result.Failure<TripDto>(TripErrors.NotFound)
            : Result.Success(dto);
    }
}