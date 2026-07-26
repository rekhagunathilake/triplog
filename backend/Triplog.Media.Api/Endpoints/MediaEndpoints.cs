using MediatR;
using Triplog.Media.Api.Http;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Application.MediaItems.Commands.CreateMediaItem;
using Triplog.Media.Application.MediaItems.Commands.FailMediaItem;
using Triplog.Media.Application.MediaItems.Commands.FinalizeMediaItem;
using Triplog.Media.Application.MediaItems.Queries.GetMediaItemById;
using Triplog.Media.Application.MediaItems.Queries.ListMediaItemsByOwner;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Api.Endpoints;

public static class MediaEndpoints
{
    private const string Bucket = "triplog-media";
    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DownloadUrlExpiry = TimeSpan.FromMinutes(60);

    public static void MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/media").WithTags("Media");

        // Request an upload Url - creates a Provitional MediaItem row + presigned MinIO PUT Url
        group.MapPost("/upload-url", async (
            RequestUploadUrlRequest req,
            ICurrentUser user,
            IObjectStorage storage,
            ISender sender,
            CancellationToken ct) =>
        {
            var uploadId = Guid.NewGuid();
            var blobKey = $"owners/{user.UserId.Value}/{uploadId}";

            var createResult = await sender.Send(new CreateMediaItemCommand(
                user.UserId, blobKey, req.ContentType, req.SizeInBytes, req.OriginalFileName), ct);

            if (createResult.IsFailure)
                return createResult.ToNoContentResult(); // maps error -> ProblemDetails

            var presigned = await storage.CreatePresignedPutUrlAsync(Bucket, blobKey, UploadUrlExpiry, ct);

            return Results.Created(
                $"/media/{createResult.Value.Value}",
                new UploadUrlResponse(
                    MediaId: createResult.Value.Value,
                    UploadUrl: presigned.Url,
                    ExpiresAtUtc: presigned.ExpiresAtUtc
                    ));
        });

        // Get metadata by id
        group.MapGet("/{id:guid}", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMediaItemByIdQuery(new MediaItemId(id), user.UserId), ct);
            return result.ToOkResult();
        });

        // List for current owner
        group.MapGet("/", async (
            ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListMediaItemsByOwnerQuery(user.UserId), ct);
            return result.ToOkResult();
        });

        // Get a presigned download URL for viewing
        group.MapGet("/{id:guid}/download-url", async (
            Guid id, ICurrentUser user, IObjectStorage storage, ISender sender, CancellationToken ct) =>
        {
            var metadata = await sender.Send(new GetMediaItemByIdQuery(new MediaItemId(id), user.UserId), ct);
            if (metadata.IsFailure)
                return metadata.ToOkResult();

            var url = await storage.CreatePresignedGetUrlAsync(Bucket, metadata.Value.BlobKey, DownloadUrlExpiry, ct);
            return Results.Ok(new DownloadUrlResponse(url));
        });

        // Saga-called endpoints — no OwnerId
        group.MapPost("/{id:guid}/finalize", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new FinalizeMediaItemCommand(new MediaItemId(id)), ct);
            return result.ToNoContentResult();
        });

        group.MapPost("/{id:guid}/fail", async (
            Guid id, FailMediaRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new FailMediaItemCommand(new MediaItemId(id), req.Reason), ct);
            return result.ToNoContentResult();
        });
    }
}

public sealed record RequestUploadUrlRequest(string ContentType, long SizeInBytes, string OriginalFileName);
public sealed record UploadUrlResponse(Guid MediaId, string UploadUrl, DateTime ExpiresAtUtc);
public sealed record DownloadUrlResponse(string Url);
public sealed record FailMediaRequest(string Reason);