using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Trips.Commands.ArchiveTrip;

public sealed record ArchiveTripCommand(TripId TripId, OwnerId OwnerId) : IRequest<Result>;