using FluentAssertions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Domain.UnitTests.EntryTests;

public class MediaReferenceTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        // Arrange
        var id = MediaReferenceId.NewId();

        // Act
        var result = MediaReference.Create(id: id,
            displayOrder: 0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayOrder.Should().Be(0);
        result.Value.Id.Should().Be(id);
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_ReturnsInvalidDisplayOrderError()
    {
        var result = MediaReference.Create(id: MediaReferenceId.NewId(),
            displayOrder: -1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EntryErrors.InvalidDisplayOrder);
    }
}
