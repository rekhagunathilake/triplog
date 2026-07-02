using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Application.Trips.Queries.ListTripsByOwner;

public sealed record ListTripsByOwnerQuery(OwnerId OwnerId) : IRequest<Result<IReadOnlyList<TripSummaryDto>>>;