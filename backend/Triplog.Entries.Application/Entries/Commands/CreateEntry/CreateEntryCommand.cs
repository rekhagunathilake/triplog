using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Entries.Commands.CreateEntry;

public sealed record CreateEntryCommand(
    TripId TripId,
    OwnerId OwnerId,
    string Title,
    string Body,
    DateOnly VisitedOn,
    string? LocationName,
    double? Latitude,
    double? Longitude) : IRequest<Result<EntryId>>;