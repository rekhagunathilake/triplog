using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.FailPublish;

public sealed record FailPublishCommand(EntryId EntryId, string Reason) : IRequest<Result>;