using FluentAssertions;
using Triplog.IntegrationTests.Fixtures;

namespace Triplog.IntegrationTests;

public class SagaFailurePathTests(TriplogSystemFixture fx) : IClassFixture<TriplogSystemFixture>
{
    private static readonly Guid TestUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private ApiClient EntriesApi() => new(fx.Entries.CreateClient(), TestUserId);
    private ApiClient MediaApi() => new(fx.Media.CreateClient(), TestUserId);

    [Fact]
    public async Task Publishing_Entry_When_Blob_Is_Missing_Resets_To_Draft_And_Fails_Media()
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
            $"/trips/{trip.Id.Value}/entries",
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

        // 4. Skip uploading real media.

        // 5. Attach the media reference to the entry
        await entries.PostVoidAsync($"/entries/{entry.Id.Value}/media/{upload.MediaId}");

        // 6. Kick off the saga
        await entries.PostVoidAsync($"/entries/{entry.Id.Value}/publish");

        // 7. Poll until saga finishes (Published) or blows up (Failed)
        var finalEntry = await TestWait.ForAsync(
            fetch: () => entries.GetAsync<EntryBody>($"/entries/{entry.Id.Value}"),
            predicate: e => (e.Status == "Draft" && e.LastPublishFailReason is not null)
                 || e.Status == "Published",   // include for fast-fail if saga took wrong 
            timeout: TimeSpan.FromSeconds(15));

        finalEntry.Status.Should().Be("Draft", "the domain resets to Draft after a failed publish so the user can retry");
        finalEntry.LastPublishFailReason.Should().Contain("Blob not found");
        finalEntry.PublishedAtUtc.Should().BeNull();

        // 8. Media item should have been failed as part of the saga
        var mediaItem = await media.GetAsync<MediaItemBody>($"/media/{upload.MediaId}");
        mediaItem.Status.Should().Be("Failed");
    }

    // Test-local DTOs matching wire shapes
    private record CreatedResponse(IdBody Id);
    private record IdBody(Guid Value);

    private record UploadUrlBody(Guid MediaId);

    private record EntryBody(Guid Id, string Status, DateTime? PublishedAtUtc, string? LastPublishFailReason);

    private record MediaItemBody(Guid Id, string Status);
}
