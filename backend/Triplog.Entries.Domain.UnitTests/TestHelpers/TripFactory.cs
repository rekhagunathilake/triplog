using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Domain.UnitTests.TestHelpers;

internal static class TripFactory
{
    public static readonly DateTime FixedNowUtc = new(2026, 6, 22, 12, 0, 0, DateTimeKind.Utc);

    public static Trip CreatePlanningTrip()
    {
        return Trip.Create(
                        ownerId: OwnerId.NewId(),
                        title: CreateTitle("Test Title EBC"),
                        description: "Description",
                        dates: CreateDateRange(DateOnly.FromDateTime(FixedNowUtc).AddDays(30), DateOnly.FromDateTime(FixedNowUtc).AddDays(44)),
                        nowUtc: FixedNowUtc).Value;
    }

    public static Trip CreateActiveTrip()
    {
        var trip = CreatePlanningTrip();

        EnsureSuccess(trip.Activate(FixedNowUtc.AddDays(1)), nameof(Trip.Activate));

        return trip;
    }

    public static Trip CreateCompletedTrip()
    {
        var trip = CreateActiveTrip();

        EnsureSuccess(trip.Complete(FixedNowUtc.AddDays(15)), nameof(Trip.Complete));

        return trip;
    }

    public static Trip CreateArchivedTrip()
    {
        var trip = CreateCompletedTrip();

        EnsureSuccess(trip.Archive(FixedNowUtc.AddDays(30)), nameof(Trip.Archive));

        return trip;
    }

    public static TripTitle CreateTitle(string value = "Test title") => TripTitle.Create(value).Value;

    public static DateRange CreateDateRange(DateOnly startDate, DateOnly endDate) => DateRange.Create(startDate, endDate).Value;

    // Extension to throw on failure
    private static void EnsureSuccess(Result result, string operation)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(
                $"Test fixture setup failed at {operation}: {result.Error.Code}");
    }
}
