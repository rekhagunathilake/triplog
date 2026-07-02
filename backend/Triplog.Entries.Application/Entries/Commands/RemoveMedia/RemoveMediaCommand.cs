using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.RemoveMedia;

public sealed record RemoveMediaCommand(
    EntryId EntryId, OwnerId OwnerId, MediaReferenceId MediaReferenceId) : IRequest<Result>;