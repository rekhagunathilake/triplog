using FluentAssertions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Domain.UnitTests.TripTests;

public class DateRangeTests
{
    private static readonly DateOnly ValidStartDate = new(2026, 7, 1);
    private static readonly DateOnly ValidEndDate = new(2026, 7, 14);

    private static readonly DateOnly InvalidStartDate = new(2026, 7, 14);
    private static readonly DateOnly InvalidEndDate = new(2026, 7, 1);

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = DateRange.Create(ValidStartDate, ValidEndDate);

        result.IsSuccess.Should().BeTrue();
        result.Value.StartDate.Should().Be(ValidStartDate);
        result.Value.EndDate.Should().Be(ValidEndDate);
    }

    [Fact]
    public void Create_WithStartDateExceedingEndDate_ReturnsDateRangeOutOfRangeError()
    {
        var result = DateRange.Create(InvalidStartDate, InvalidEndDate);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.DateRangeOutOfRange);
    }

    [Fact]
    public void Create_WithStartEqualEnd_ReturnsSuccess()
    {
        var date = new DateOnly(2026, 7, 1);

        var result = DateRange.Create(date, date);

        result.IsSuccess.Should().BeTrue();
        result.Value.StartDate.Should().Be(date);
        result.Value.EndDate.Should().Be(date);
    }
}
