using Triplog.Media.Domain.Abstractions;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Domain.UnitTests.TestHelpers;

internal static class MediaItemFactory
{
    public static readonly DateTime FixedNowUtc = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

    public static readonly OwnerId DefaultOwner = OwnerId.NewId();

    public static MediaItem CreateProvisional()
    {
        var mediaItemId = MediaItemId.NewId();
        return MediaItem.Create(
            DefaultOwner,
            BlobKey.ForOwner(DefaultOwner, mediaItemId),
            CreateContentType(),
            sizeInBytes: 1_024_000, // 1 MB
            "test.jpg",
            FixedNowUtc).Value;
    }

    public static MediaItem CreateFinalized()
    {
        var mediaItem = CreateProvisional();
        mediaItem.ClearDomainEvents();
        EnsureSuccess(mediaItem.Finalize(FixedNowUtc.AddMinutes(2)),
            nameof(MediaItem.Finalize));
        return mediaItem;
    }

    public static MediaItem CreateFailed(string reason = "Upload timeout")
    {
        var mediaItem = CreateProvisional();
        mediaItem.ClearDomainEvents();
        EnsureSuccess(mediaItem.Fail(reason, FixedNowUtc.AddMinutes(2)),
            nameof(MediaItem.Fail));
        return mediaItem;
    }

    public static ContentType CreateContentType(string value = "image/jpeg") =>
        ContentType.Create(value).Value;

    private static void EnsureSuccess(Result result, string operationName)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Test fixture setup failed at '{operationName}': {result.Error.Code}");
        }
    }
}
