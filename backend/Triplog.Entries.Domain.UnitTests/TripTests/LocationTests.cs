using FluentAssertions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Domain.UnitTests.TripTests;

public class LocationTests
{
    private const string ValidName = "Paris, France";
    private const double ValidLatitude = 48.8566;
    private const double ValidLongitude = 2.3522;

    // Success

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Location.Create(ValidName, ValidLatitude, ValidLongitude);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(ValidName);
        result.Value.Latitude.Should().Be(ValidLatitude);
        result.Value.Longitude.Should().Be(ValidLongitude);
    }

    [Fact]
    public void Create_TrimsWhitespaceFromName()
    {
        var result = Location.Create("   Paris  ", ValidLatitude, ValidLongitude);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Paris");
    }

    [Fact]
    public void Create_WithMaxLengthName_ReturnsSuccess()
    {
        var maxLengthName = new string('a', Location.MaxLocationNameLength);

        var result = Location.Create(maxLengthName, ValidLatitude, ValidLongitude);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Length.Should().Be(Location.MaxLocationNameLength);
    }

    [Fact]
    public void Create_WithBoundaryLatitudes_ReturnsSuccess()
    {
        Location.Create(ValidName, 90, ValidLongitude).IsSuccess.Should().BeTrue();
        Location.Create(ValidName, -90, ValidLongitude).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithBoundaryLongitudes_ReturnsSuccess()
    {
        Location.Create(ValidName, ValidLatitude, 180).IsSuccess.Should().BeTrue();
        Location.Create(ValidName, ValidLatitude, -180).IsSuccess.Should().BeTrue();
    }

    // Name validation

    [Fact]
    public void Create_WithEmptyName_ReturnsNameEmptyError()
    {
        var result = Location.Create(string.Empty, ValidLatitude, ValidLongitude);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.NameEmpty);
    }

    [Fact]
    public void Create_WithWhitespaceName_ReturnsNameEmptyError()
    {
        var result = Location.Create("   ", ValidLatitude, ValidLongitude);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.NameEmpty);
    }

    [Fact]
    public void Create_WithNullName_ReturnsNameEmptyError()
    {
        var result = Location.Create(null!, ValidLatitude, ValidLongitude);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.NameEmpty);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ReturnsNameTooLongError()
    {
        var tooLong = new string('a', Location.MaxLocationNameLength + 1);

        var result = Location.Create(tooLong, ValidLatitude, ValidLongitude);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.NameTooLong(Location.MaxLocationNameLength + 1));
    }

    // Latitude validation

    [Fact]
    public void Create_WithLatitudeAbove90_ReturnsLatitudeOutOfRangeError()
    {
        var result = Location.Create(ValidName, 90.0001, ValidLongitude);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.LatitudeOutOfRange(90.0001));
    }

    [Fact]
    public void Create_WithLatitudeBelowMinus90_ReturnsLatitudeOutOfRangeError()
    {
        var result = Location.Create(ValidName, -90.0001, ValidLongitude);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.LatitudeOutOfRange(-90.0001));
    }

    // Longitude validation

    [Fact]
    public void Create_WithLongitudeAbove180_ReturnsLongitudeOutOfRangeError()
    {
        var result = Location.Create(ValidName, ValidLatitude, 180.0001);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.LongitudeOutOfRange(180.0001));
    }

    [Fact]
    public void Create_WithLongitudeBelowMinus180_ReturnsLongitudeOutOfRangeError()
    {
        var result = Location.Create(ValidName, ValidLatitude, -180.0001);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.LongitudeOutOfRange(-180.0001));
    }

    // First-failure-wins ordering

    [Fact]
    public void Create_WithMultipleInvalidInputs_ReturnsFirstFailureFromValidationOrder()
    {
        // Name validation executes first - should fail on name even if rest are invalid.
        var result = Location.Create(string.Empty, 999, 999);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LocationErrors.NameEmpty);
    }
}
