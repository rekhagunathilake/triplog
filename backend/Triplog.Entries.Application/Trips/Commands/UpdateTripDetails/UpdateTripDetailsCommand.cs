using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Commands.UpdateTripDetails;

public sealed record UpdateTripDetailsCommand(
    TripId TripId,
    OwnerId OwnerId,
    string Title,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<Result>;