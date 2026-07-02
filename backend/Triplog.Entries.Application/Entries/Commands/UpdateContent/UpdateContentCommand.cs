using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.UpdateContent;

public sealed record UpdateContentCommand(
    EntryId EntryId,
    OwnerId OwnerId,
    string Title,
    string Body,
    DateOnly VisitedOn,
    string? LocationName,
    double? Latitude,
    double? Longitude) : IRequest<Result>;