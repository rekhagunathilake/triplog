using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Queries.GetTripById;

public sealed record GetTripByIdQuery(TripId TripId, OwnerId OwnerId) : IRequest<Result<TripDto>>;