using MediatR;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Commands.CompletePublish;

public sealed record CompletePublishCommand(EntryId EntryId) : IRequest<Result>;