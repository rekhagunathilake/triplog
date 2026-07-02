using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.AttachMedia;

public sealed record AttachMediaCommand(
    EntryId EntryId, OwnerId OwnerId, MediaReferenceId MediaReferenceId) : IRequest<Result>;