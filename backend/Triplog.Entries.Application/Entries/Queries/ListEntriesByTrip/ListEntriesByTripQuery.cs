using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Application.Entries.Queries.ListEntriesByTrip;

public sealed record ListEntriesByTripQuery(TripId TripId, OwnerId OwnerId)
    : IRequest<Result<IReadOnlyList<EntrySummaryDto>>>;