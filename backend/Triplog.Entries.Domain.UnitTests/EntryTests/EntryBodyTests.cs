using FluentAssertions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Domain.UnitTests.EntryTests;

public class EntryBodyTests
{
    [Fact]
    public void Create_WithValidText_ReturnsSuccess()
    {
        var result = EntryBody.Create("Valid Body Text");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Body Text");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankBody_Fails(string blank)
    {
        var result = EntryBody.Create(blank);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EntryErrors.BodyEmpty);
    }

    [Fact]
    public void Create_WithTextExceedingMaxLength_Fails()
    {
        var tooLong = new string('a', EntryBody.MaxLength + 1);

        var result = EntryBody.Create(tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.BodyTooLong");
    }
}
