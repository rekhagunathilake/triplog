using Triplog.Media.Domain.Abstractions;

namespace Triplog.Media.Domain.MediaItems;

public static class MediaItemErrors
{
    public static readonly Error ContentTypeEmpty =
        Error.Validation("MediaItem.ContentTypeEmpty", "Content type cannot be empty.");

    public static Error ContentTypeNotAllowed(string contentType) =>
        Error.Validation("MediaItem.ContentTypeNotAllowed", $"Content type: {contentType} is not allowed.");

    public static readonly Error IsAlreadyFinalized =
            Error.Validation("MediaItem.IsAlreadyFinalized", "Media item is already fianlized.");

    public static readonly Error InvalidStatusTransition =
        Error.Validation("MediaItem.InvalidStatusTransition", "The requested status transition is not allowed from the current status.");
}
