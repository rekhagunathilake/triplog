using Triplog.Entries.Application.Trips.Queries.GetTripById;
using Triplog.Entries.Application.Trips.Queries.ListTripsByOwner;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Abstractions;

public interface ITripQueries
{
    Task<TripDto?> GetByIdAsync(TripId id, OwnerId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<TripSummaryDto>> ListByOwnerAsync(OwnerId ownerId, CancellationToken ct = default);
}