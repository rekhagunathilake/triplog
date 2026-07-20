using MediatR;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.Common;

namespace Triplog.Media.Application.MediaItems.Queries.ListMediaItemsByOwner;

public sealed record ListMediaItemsByOwnerQuery(OwnerId OwnerId)
    : IRequest<Result<IReadOnlyList<MediaItemSummaryDto>>>;