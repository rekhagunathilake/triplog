using FluentAssertions;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Domain.UnitTests.TripTests;

public class TripTitleTests
{
    [Fact]
    public void Create_WithValidText_ReturnsSuccess()
    {
        var result = TripTitle.Create("Valid Title Text");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Title Text");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankTitle_Fails(string blank)
    {
        var result = TripTitle.Create(blank);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.TitleEmpty);
    }

    [Fact]
    public void Create_WithTextExceedingMaxLength_Fails()
    {
        var tooLong = new string('a', TripTitle.MaxLength + 1);

        var result = TripTitle.Create(tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.TitleTooLong");
    }
}
