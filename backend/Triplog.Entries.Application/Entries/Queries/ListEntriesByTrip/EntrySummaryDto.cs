using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Queries.ListEntriesByTrip;

public sealed record EntrySummaryDto(
    Guid Id,
    Guid TripId,
    string Title,
    DateOnly VisitedOn,
    EntryStatus Status,
    int MediaCount);