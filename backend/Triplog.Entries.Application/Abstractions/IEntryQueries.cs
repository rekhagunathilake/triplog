using Triplog.Entries.Application.Entries.Queries.GetEntryById;
using Triplog.Entries.Application.Entries.Queries.ListEntriesByTrip;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Abstractions;

public interface IEntryQueries
{
    Task<EntryDto?> GetByIdAsync(EntryId id, OwnerId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<EntrySummaryDto>> ListByTripAsync(TripId tripId, OwnerId ownerId, CancellationToken ct = default);
}