using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Queries.GetTripById;

public sealed record TripDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    TripStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ArchivedAtUtc);