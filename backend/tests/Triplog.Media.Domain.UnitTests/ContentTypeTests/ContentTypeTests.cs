using FluentAssertions;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Domain.UnitTests.ContentTypeTests;

public class ContentTypeTests
{
    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Create_WithAllowedType_ReturnsSuccess(string value)
    {
        var result = ContentType.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmpty_ReturnsContentTypeEmptyError(string value)
    {
        var result = ContentType.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MediaItemErrors.ContentTypeEmpty);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("video/mp4")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void Create_WithDisallowedType_ReturnsContentTypeNotAllowedError(string value)
    {
        var result = ContentType.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MediaItem.ContentTypeNotAllowed");
    }

    [Theory]
    [InlineData("IMAGE/JPEG", "image/jpeg")]
    [InlineData("Image/PNG", "image/png")]
    [InlineData("image/WebP", "image/webp")]
    public void Create_IsCaseInsensitive_NormalizesToLowercase(string input, string expected)
    {
        var result = ContentType.Create(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }
}
