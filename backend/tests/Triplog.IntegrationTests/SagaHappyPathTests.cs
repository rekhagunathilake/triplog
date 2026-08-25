using FluentAssertions;
using System.Net.Http.Headers;
using Triplog.IntegrationTests.Fixtures;

namespace Triplog.IntegrationTests;

public class SagaHappyPathTests(TriplogSystemFixture triplogSystemFixture) :
    IClassFixture<TriplogSystemFixture>
{
    private static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private ApiClient EntriesApi() => new(triplogSystemFixture.Entries.CreateClient(), TestUserId);
    private ApiClient MediaApi() => new(triplogSystemFixture.Media.CreateClient(), TestUserId);

    [Fact]
    public async Task Publishing_Entry_With_Media_Transitions_To_Published_And_Finalizes_Media()
    {
        var entries = EntriesApi();
        var media = MediaApi();

        // 1. Trip
        var trip = await entries.PostAsync<CreatedResponse>("/trips", new
        {
            title = "Saga happy path",
            startDate = "2026-11-01",
            endDate = "2026-11-05",
        });

        // 2. Draft entry under that trip
        var entry = await entries.PostAsync<CreatedResponse>(
            $"/trips/{trip.Id}/entries",
            new
            {
                title = "Day 1",
                body = "First entry body",
                visitedOn = "2026-11-01",
            });

        // 3. Request an upload URL — creates a provisional MediaItem row
        var upload = await media.PostAsync<UploadUrlBody>("/media/upload-url", new
        {
            contentType = "image/jpeg",
            sizeInBytes = 12,
            originalFileName = "photo.jpg",
        });

        // 4. PUT bytes straight to MinIO — bypass ApiClient, presigned URL is standalone
        await UploadBytesAsync(upload.UploadUrl, "test-jpeg!!!"u8.ToArray(), "image/jpeg");

        // 5. Attach the media reference to the entry
        await entries.PostVoidAsync($"/entries/{entry.Id}/media/{upload.MediaId}");

        // 6. Kick off the saga
        await entries.PostVoidAsync($"/entries/{entry.Id}/publish");

        // 7. Poll until saga finishes (Published) or blows up (Failed)
        var finalEntry = await TestWait.ForAsync(
            fetch: () => entries.GetAsync<EntryBody>($"/entries/{entry.Id}"),
            predicate: e => e.Status == "Draft" || e.Status == "Published",
            timeout: TimeSpan.FromSeconds(15));

        finalEntry.Status.Should().Be("Published");
        finalEntry.PublishedAtUtc.Should().NotBeNull();

        // 8. Media item should have been finalized as part of the saga
        var mediaItem = await media.GetAsync<MediaItemBody>($"/media/{upload.MediaId}");
        mediaItem.Status.Should().Be("Finalized");
    }

    private static async Task UploadBytesAsync(string presignedUrl, byte[] bytes, string contentType)
    {
        using var httpClient = new HttpClient();
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var response = await httpClient.PutAsync(presignedUrl, content);
        response.EnsureSuccessStatusCode();
    }

    // Test-local DTOs matching wire shapes
    private record CreatedResponse(Guid Id);

    private record UploadUrlBody(Guid MediaId, string UploadUrl, DateTime ExpiresAtUtc);

    private record EntryBody(Guid Id, string Status, DateTime? PublishedAtUtc);

    private record MediaItemBody(Guid Id, string Status);
}
