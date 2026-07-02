// Query
using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Queries.GetEntryById;

public sealed record GetEntryByIdQuery(EntryId EntryId, OwnerId OwnerId) : IRequest<Result<EntryDto>>;