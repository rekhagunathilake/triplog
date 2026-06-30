using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Abstractions;

public interface ITripRepository
{
    Task AddAsync(Trip trip, CancellationToken ct = default);

    Task<Trip?> GetByIdAsync(TripId id, CancellationToken ct = default);
}
