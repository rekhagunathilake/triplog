using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Application.Trips.Queries.ListTripsByOwner;

public sealed class ListTripsByOwnerQueryHandler(ITripQueries queries)
    : IRequestHandler<ListTripsByOwnerQuery, Result<IReadOnlyList<TripSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<TripSummaryDto>>> Handle(
        ListTripsByOwnerQuery request,
        CancellationToken cancellationToken)
    {
        var trips = await queries.ListByOwnerAsync(request.OwnerId, cancellationToken);
        return Result.Success(trips);
    }
}