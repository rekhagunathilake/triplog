using MediatR;
using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Application.MediaItems.Commands.FinalizeMediaItem;

public sealed record FinalizeMediaItemCommand(MediaItemId MediaItemId) : IRequest<Result>;
