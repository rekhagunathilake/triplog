using Microsoft.EntityFrameworkCore;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Application.Entries.Queries.GetEntryById;
using Triplog.Entries.Application.Entries.Queries.ListEntriesByTrip;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Infrastructure.Persistence.Queries;

public sealed class EntryQueries(TriplogEntriesDbContext dbContext) : IEntryQueries
{
    public async Task<EntryDto?> GetByIdAsync(EntryId id, OwnerId ownerId, CancellationToken ct = default)
    {
        return await dbContext.Entries
            .AsNoTracking()
            .Where(e => e.Id == id && e.OwnerId == ownerId)
            .Select(e => new EntryDto(
                e.Id.Value,
                e.TripId.Value,
                e.OwnerId.Value,
                e.Title.Value,
                e.Body.Value,
                e.Location == null ? null : new LocationDto(
                    e.Location.Name,
                    e.Location.Latitude,
                    e.Location.Longitude
                ),
                e.VisitedOn,
                e.Status,
                e.MediaReferences
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => new MediaReferenceDto(m.Id.Value, m.DisplayOrder))
                    .ToList(),
                e.CreatedAtUtc,
                e.PublishedAtUtc,
                e.ArchivedAtUtc,
                e.LastPublishFailReason))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<EntrySummaryDto>> ListByTripAsync(TripId tripId, OwnerId ownerId, CancellationToken ct = default)
    {
        return await dbContext.Entries
            .AsNoTracking()
            .Where(e => e.TripId == tripId && e.OwnerId == ownerId)
            .OrderByDescending(e => e.VisitedOn)
            .Select(e => new EntrySummaryDto(
                e.Id.Value,
                e.TripId.Value,
                e.Title.Value,
                e.VisitedOn,
                e.Status,
                e.MediaReferences.Count
                ))
            .ToListAsync(ct);
    }
}
