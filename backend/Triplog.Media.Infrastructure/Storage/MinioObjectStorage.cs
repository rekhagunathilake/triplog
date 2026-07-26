using Minio;
using Minio.DataModel.Args;
using Triplog.Media.Application.Abstractions;

namespace Triplog.Media.Infrastructure.Storage;

public sealed class MinioObjectStorage(IMinioClient minio, TimeProvider timeProvider) : IObjectStorage
{
    public Task<string> CreatePresignedGetUrlAsync(string bucket, string key, TimeSpan expiry, CancellationToken ct = default)
    {
        return minio.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithExpiry((int)expiry.TotalSeconds));
    }

    public async Task<PresignedUploadUrl> CreatePresignedPutUrlAsync(string bucket, string key, TimeSpan expiry, CancellationToken ct = default)
    {
        var url = await minio.PresignedPutObjectAsync(
            new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithExpiry((int)expiry.TotalSeconds));

        var expiresAt = timeProvider.GetUtcNow().Add(expiry).UtcDateTime;
        return new PresignedUploadUrl(url, expiresAt);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken ct = default)
    {
        try 
        {
            await minio.StatObjectAsync(
                new StatObjectArgs().WithBucket(bucket).WithObject(key), ct);
            return true;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return false;
        }
    }
}
