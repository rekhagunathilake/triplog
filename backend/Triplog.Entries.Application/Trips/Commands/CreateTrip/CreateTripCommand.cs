using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Commands.CreateTrip;

public sealed record CreateTripCommand(
    OwnerId UserId,
    string TripTitle,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<Result<TripId>>;
