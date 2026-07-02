using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.ArchiveEntry;

public sealed record ArchiveEntryCommand(EntryId EntryId, OwnerId OwnerId) : IRequest<Result>;