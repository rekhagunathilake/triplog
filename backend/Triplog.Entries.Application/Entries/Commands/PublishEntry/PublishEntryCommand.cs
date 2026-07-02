using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.PublishEntry;

public sealed record PublishEntryCommand(EntryId EntryId, OwnerId OwnerId) : IRequest<Result>;