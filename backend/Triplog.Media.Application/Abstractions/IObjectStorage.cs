namespace Triplog.Media.Application.Abstractions;

public interface IObjectStorage
{
    Task<PresignedUploadUrl> CreatePresignedPutUrlAsync(
        string bucket,
        string key,
        TimeSpan expiry,
        CancellationToken ct = default);

    Task<string> CreatePresignedGetUrlAsync(
        string bucket,
        string key,
        TimeSpan expiry,
        CancellationToken ct = default);

    Task<bool> ObjectExistsAsync(
        string bucket,
        string key,
        CancellationToken ct = default);
}

public sealed record PresignedUploadUrl(string Url, DateTime ExpiresAtUtc);
