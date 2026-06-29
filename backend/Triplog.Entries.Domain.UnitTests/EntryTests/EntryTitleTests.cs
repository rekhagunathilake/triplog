using FluentAssertions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Domain.UnitTests.EntryTests;

public class EntryTitleTests
{
    [Fact]
    public void Create_WithValidText_ReturnsSuccess()
    {
        var result = EntryTitle.Create("Valid Title Text");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Title Text");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankTitle_Fails(string blank)
    {
        var result = EntryTitle.Create(blank);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.TitleEmpty");
    }

    [Fact]
    public void Create_WithTextExceedingMaxLength_Fails()
    {
        var tooLong = new string('a', EntryTitle.MaxLength + 1);

        var result = EntryTitle.Create(tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.TitleTooLong");
    }
}
