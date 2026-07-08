using Microsoft.EntityFrameworkCore;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Application.Trips.Queries.GetTripById;
using Triplog.Entries.Application.Trips.Queries.ListTripsByOwner;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Infrastructure.Persistence.Queries;

public sealed class TripQueries(TriplogEntriesDbContext dbContext) : ITripQueries
{
    public async Task<TripDto?> GetByIdAsync(TripId id, OwnerId ownerId, CancellationToken ct = default)
    {
        return await dbContext.Trips
            .AsNoTracking()
            .Where(t => t.Id == id && t.OwnerId == ownerId)
            .Select(t => new TripDto(
                t.Id.Value,
                t.OwnerId.Value,
                t.Title.Value,
                t.Description,
                t.Dates.StartDate,
                t.Dates.EndDate,
                t.Status,
                t.CreatedAtUtc,
                t.ArchivedAtUtc))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<TripSummaryDto>> ListByOwnerAsync(OwnerId ownerId, CancellationToken ct = default)
    {
        return await dbContext.Trips
            .AsNoTracking()
            .Where(t => t.OwnerId == ownerId)
            .OrderByDescending(t => t.Dates.StartDate)
            .Select(t => new TripSummaryDto(
                t.Id.Value,
                t.Title.Value,
                t.Dates.StartDate,
                t.Dates.EndDate,
                t.Status))
            .ToListAsync(ct);
    }
}
