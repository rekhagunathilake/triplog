using MediatR;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Commands.FailMediaItem
{
    public sealed record FailMediaItemCommand(MediaItemId MediaItemId, string Reason) : IRequest<Result>;
}
