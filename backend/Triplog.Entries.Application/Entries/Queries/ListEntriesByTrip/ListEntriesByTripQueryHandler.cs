using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Application.Entries.Queries.ListEntriesByTrip;

public sealed class ListEntriesByTripQueryHandler(IEntryQueries queries)
    : IRequestHandler<ListEntriesByTripQuery, Result<IReadOnlyList<EntrySummaryDto>>>
{
    public async Task<Result<IReadOnlyList<EntrySummaryDto>>> Handle(
        ListEntriesByTripQuery request, CancellationToken cancellationToken)
    {
        var entries = await queries.ListByTripAsync(request.TripId, request.OwnerId, cancellationToken);
        return Result.Success(entries);
    }
}