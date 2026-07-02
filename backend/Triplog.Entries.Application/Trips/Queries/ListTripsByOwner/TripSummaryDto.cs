using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Queries.ListTripsByOwner;

public sealed record TripSummaryDto(
    Guid Id,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    TripStatus Status);