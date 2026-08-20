using FluentAssertions;
using Triplog.Media.Domain.MediaItems;
using Triplog.Media.Domain.MediaItems.Events;
using Triplog.Media.Domain.UnitTests.TestHelpers;

namespace Triplog.Media.Domain.UnitTests.MediaItemTests;

public class MediaItemTests
{
    // Create Tests

    [Fact]
    public void Create_WithValidInputs_InitializesInProvisionalStatus()
    {
        var mediaItem = MediaItemFactory.CreateProvisional();

        mediaItem.Id.Should().NotBe(default(MediaItemId));
        mediaItem.OwnerId.Should().Be(MediaItemFactory.DefaultOwner);
        mediaItem.Status.Should().Be(MediaItemStatus.Provisional);
        mediaItem.CreatedAtUtc.Should().Be(MediaItemFactory.FixedNowUtc);
        mediaItem.FinalizedAtUtc.Should().BeNull();
        mediaItem.FailedAtUtc.Should().BeNull();
        mediaItem.FailureReason.Should().BeNull();
        mediaItem.OriginalFileName.Should().Be("test.jpg");
        mediaItem.SizeInBytes.Should().Be(1_024_000);
    }

    [Fact]
    public void Create_WithValidInputs_RaisesMediaItemCreatedDomainEvent()
    {
        var mediaItem = MediaItemFactory.CreateProvisional();

        mediaItem.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MediaItemCreatedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(MediaItemFactory.FixedNowUtc);

        var evnt = mediaItem.DomainEvents.OfType<MediaItemCreatedDomainEvent>().Single();
        evnt.MediaItemId.Should().Be(mediaItem.Id);
        evnt.OwnerId.Should().Be(mediaItem.OwnerId);
    }

    // Finalize

    [Fact]
    public void Finalize_FromProvisional_TransitionsToFinalizedAndRaisesEvent()
    {
        var mediaItem = MediaItemFactory.CreateProvisional();
        mediaItem.ClearDomainEvents();
        var finalizeTime = MediaItemFactory.FixedNowUtc.AddMinutes(5);

        var result = mediaItem.Finalize(finalizeTime);

        result.IsSuccess.Should().BeTrue();
        mediaItem.Status.Should().Be(MediaItemStatus.Finalized);
        mediaItem.FinalizedAtUtc.Should().Be(finalizeTime);
        mediaItem.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MediaItemFinalizedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(finalizeTime);
    }

    [Fact]
    public void Finalize_FromFinalized_ReturnsInvalidStatusTransition()
    {
        var mediaItem = MediaItemFactory.CreateFinalized();
        mediaItem.ClearDomainEvents();

        var result = mediaItem.Finalize(MediaItemFactory.FixedNowUtc.AddMinutes(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MediaItemErrors.IsAlreadyFinalized);
        mediaItem.Status.Should().Be(MediaItemStatus.Finalized);
        mediaItem.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Finalize_FromFailed_ReturnsInvalidStatusTransition()
    {
        var mediaItem = MediaItemFactory.CreateFailed();
        mediaItem.ClearDomainEvents();

        var result = mediaItem.Finalize(MediaItemFactory.FixedNowUtc.AddMinutes(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MediaItemErrors.InvalidStatusTransition);
        mediaItem.Status.Should().Be(MediaItemStatus.Failed);
        mediaItem.DomainEvents.Should().BeEmpty();
    }

    // Fail

    [Fact]
    public void Fail_FromProvisional_TransitionsToFailedAndSetsReason()
    {
        var mediaItem = MediaItemFactory.CreateProvisional();
        mediaItem.ClearDomainEvents();
        var failTime = MediaItemFactory.FixedNowUtc.AddMinutes(5);
        var reason = "Thumbnail generation timed out";

        var result = mediaItem.Fail(reason, failTime);

        result.IsSuccess.Should().BeTrue();
        mediaItem.Status.Should().Be(MediaItemStatus.Failed);
        mediaItem.FailedAtUtc.Should().Be(failTime);
        mediaItem.FailureReason.Should().Be(reason);
        mediaItem.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MediaItemFailedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(failTime);
    }

    [Fact]
    public void Fail_FromFinalized_ReturnsInvalidStatusTransition()
    {
        var mediaItem = MediaItemFactory.CreateFinalized();
        mediaItem.ClearDomainEvents();

        var result = mediaItem.Fail("Should not apply", MediaItemFactory.FixedNowUtc.AddMinutes(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MediaItemErrors.InvalidStatusTransition);
        mediaItem.Status.Should().Be(MediaItemStatus.Finalized);
        mediaItem.FailureReason.Should().BeNull();
        mediaItem.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Fail_FromFailed_ReturnsInvalidStatusTransition()
    {
        var mediaItem = MediaItemFactory.CreateFailed("First reason");
        mediaItem.ClearDomainEvents();
        var originalReason = mediaItem.FailureReason;

        var result = mediaItem.Fail("Second reason", MediaItemFactory.FixedNowUtc.AddMinutes(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MediaItemErrors.InvalidStatusTransition);
        mediaItem.FailureReason.Should().Be(originalReason); // unchanged
        mediaItem.DomainEvents.Should().BeEmpty();
    }
}
