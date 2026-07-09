using Microsoft.EntityFrameworkCore;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Infrastructure.Persistence.Repositories;

public sealed class TripRepository(TriplogEntriesDbContext dbContext) : ITripRepository
{
    public async Task AddAsync(Trip trip, CancellationToken ct = default) => await dbContext.Trips.AddAsync(trip, ct).AsTask();

    public async Task<Trip?> GetByIdAsync(TripId id, CancellationToken ct = default) =>
        await dbContext.Trips
        .FirstOrDefaultAsync(t => t.Id == id, ct);
}
