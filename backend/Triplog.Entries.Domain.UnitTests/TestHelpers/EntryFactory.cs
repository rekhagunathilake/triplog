using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Domain.UnitTests.TestHelpers;

internal static class EntryFactory
{
    public static readonly DateTime FixedNowUtc = new(2026, 6, 28, 12, 0, 0, DateTimeKind.Utc);
    public static readonly Location Location = Location.Create("Paris", 48.0, 22.0).Value;

    public static Entry CreateDraftEntry()
    {
        var trip = TripFactory.CreatePlanningTrip();

        return Entry.Create(
            trip.Id,
            trip.OwnerId,
            CreateTitle(),
            CreateBody(),
            DateOnly.FromDateTime(FixedNowUtc),
            Location,
            FixedNowUtc
            ).Value;
    }

    public static Entry CreateDraftEntryWithMedia()
    {
        var entry = CreateDraftEntry();
        var mediaId = MediaReferenceId.NewId();

        EnsureSuccess(entry.AttachMedia(mediaId, FixedNowUtc.AddDays(1)), nameof(Entry.AttachMedia));

        return entry;
    }

    public static Entry CreatePublishingEntry()
    {
        var entry = CreateDraftEntryWithMedia();

        EnsureSuccess(entry.BeginPublish(FixedNowUtc.AddDays(2)), nameof(Entry.BeginPublish));

        return entry;
    }

    public static Entry CreateEntryWithFailedPublish()
    {
        var entry = CreatePublishingEntry();

        EnsureSuccess(entry.FailPublish("Test reason", FixedNowUtc.AddDays(3)), nameof(Entry.FailPublish));

        return entry;
    }

    public static Entry CreatePublishedEntry()
    {
        var entry = CreatePublishingEntry();

        EnsureSuccess(entry.CompletePublish(FixedNowUtc.AddDays(4)), nameof(Entry.CompletePublish));

        return entry;
    }

    public static Entry CreateArchivedEntry()
    {
        var entry = CreatePublishedEntry();

        EnsureSuccess(entry.Archive(FixedNowUtc.AddDays(5)), nameof(Entry.Archive));

        return entry;
    }

    // Extension to throw on failure
    private static void EnsureSuccess(Result result, string operation)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(
                $"Test fixture setup failed at {operation}: {result.Error.Code}");
    }

    public static EntryTitle CreateTitle(string value = "Test title") => EntryTitle.Create(value).Value;

    public static EntryBody CreateBody(string value = "Test body") => EntryBody.Create(value).Value;
}
