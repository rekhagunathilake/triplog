using FluentAssertions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Entries.Events;
using Triplog.Entries.Domain.UnitTests.TestHelpers;

namespace Triplog.Entries.Domain.UnitTests.EntryTests;

public class EntryTests
{
    //Create 

    [Fact]
    public void Create_WithValidInputs_InitializesEntryInDraftStatus()
    {
        var entry = EntryFactory.CreateDraftEntry();

        entry.TripId.Should().NotBeNull();
        entry.OwnerId.Should().NotBeNull();
        entry.Status.Should().Be(EntryStatus.Draft);
        entry.MediaReferences.Should().BeEmpty();
        entry.PublishedAtUtc.Should().BeNull();
        entry.ArchivedAtUtc.Should().BeNull();
        entry.LastPublishFailReason.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidInputs_RaisesEntryCreatedDomainEvent()
    {
        var entry = EntryFactory.CreateDraftEntry();

        entry.DomainEvents.Should().ContainSingle()
        .Which.Should().BeOfType<EntryCreatedDomainEvent>();
        var domainEvent = entry.DomainEvents.OfType<EntryCreatedDomainEvent>().Single();
        domainEvent.TripId.Should().Be(entry.TripId);
        domainEvent.OwnerId.Should().Be(entry.OwnerId);
        domainEvent.OccurredOnUtc.Should().Be(EntryFactory.FixedNowUtc);
    }

    // Update content 

    [Fact]
    public void UpdateContent_FromDraft_AppliesChangesAndRaisesEvent()
    {
        // Arrange
        var entry = EntryFactory.CreateDraftEntry();
        entry.ClearDomainEvents();
        var newTitle = EntryFactory.CreateTitle("Updated title");
        var newBody = EntryFactory.CreateBody("Updated body");
        var newLocation = Location.Create("India", 22.00, -88.00).Value;
        var newVisitedOn = DateOnly.FromDateTime(EntryFactory.FixedNowUtc).AddDays(1);
        var updateTime = TripFactory.FixedNowUtc.AddDays(2);

        // Act
        var result = entry.UpdateContent(newTitle, newBody, newLocation, newVisitedOn, updateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();

        entry.Title.Should().Be(newTitle);
        entry.Body.Should().Be(newBody);
        entry.Location.Should().Be(newLocation);
        entry.VisitedOn.Should().Be(newVisitedOn);

        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EntryContentUpdatedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(updateTime);
    }

    [Fact]
    public void UpdateContent_FromPublishing_ReturnsNotDraftError()
    {
        // Arrange
        var entry = EntryFactory.CreatePublishingEntry();
        var originalTitle = entry.Title;
        entry.ClearDomainEvents();

        var newTitle = EntryFactory.CreateTitle("Updated title");
        var newBody = EntryFactory.CreateBody("Updated body");
        var newLocation = Location.Create("India", 22.00, -88.00).Value;
        var newVisitedOn = DateOnly.FromDateTime(EntryFactory.FixedNowUtc).AddDays(1);
        var updateTime = TripFactory.FixedNowUtc.AddDays(2);

        // Act
        var result = entry.UpdateContent(newTitle, newBody, newLocation, newVisitedOn, updateTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EntryErrors.NotDraft);
        entry.Title.Should().Be(originalTitle);
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateContent_FromPublished_ReturnsNotDraftError()
    {
        // Arrange
        var entry = EntryFactory.CreatePublishedEntry();
        var originalTitle = entry.Title;
        entry.ClearDomainEvents();

        var newTitle = EntryFactory.CreateTitle("Updated title");
        var newBody = EntryFactory.CreateBody("Updated body");
        var newLocation = Location.Create("India", 22.00, -88.00).Value;
        var newVisitedOn = DateOnly.FromDateTime(EntryFactory.FixedNowUtc).AddDays(1);
        var updateTime = TripFactory.FixedNowUtc.AddDays(2);

        // Act
        var result = entry.UpdateContent(newTitle, newBody, newLocation, newVisitedOn, updateTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EntryErrors.NotDraft);
        entry.Title.Should().Be(originalTitle);
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateContent_FromArchived_ReturnsIsArchivedError()
    {
        // Arrange
        var entry = EntryFactory.CreateArchivedEntry();
        var originalTitle = entry.Title;
        entry.ClearDomainEvents();

        var newTitle = EntryFactory.CreateTitle("Updated title");
        var newBody = EntryFactory.CreateBody("Updated body");
        var newLocation = Location.Create("India", 22.00, -88.00).Value;
        var newVisitedOn = DateOnly.FromDateTime(EntryFactory.FixedNowUtc).AddDays(1);
        var updateTime = TripFactory.FixedNowUtc.AddDays(2);

        // Act
        var result = entry.UpdateContent(newTitle, newBody, newLocation, newVisitedOn, updateTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EntryErrors.IsArchived);
        entry.Title.Should().Be(originalTitle);
        entry.DomainEvents.Should().BeEmpty();
    }

    // Attach media

    [Fact]
    public void AttachMedia_FromDraft_AddsReferenceAndRaisesEvent()
    {
        // Arrange
        var entry = EntryFactory.CreateDraftEntryWithMedia();

        entry.Status.Should().Be(EntryStatus.Draft);
        entry.MediaReferences.Should().ContainSingle().Which.DisplayOrder.Should().Be(0);
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EntryMediaAttachedDomainEvent>();
    }

    [Fact]
    public void AttachMedia_SecondAttachment_AssignsDisplayOrder1()
    {
        // Arrange
        var entry = EntryFactory.CreateDraftEntryWithMedia();
        entry.ClearDomainEvents();
        var newMediaId = MediaReferenceId.NewId();

        var result = entry.AttachMedia(newMediaId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(EntryStatus.Draft);
        entry.MediaReferences.Should().HaveCount(2);
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EntryMediaAttachedDomainEvent>();
        entry.MediaReferences.Last().Id.Should().Be(newMediaId);
        entry.MediaReferences.Last().DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void AttachMedia_DuplicateMediaReferenceId_ReturnsMediaAlreadyAttachedError()
    {
        // Arrange
        var entry = EntryFactory.CreateDraftEntryWithMedia();
        var newMediaId = MediaReferenceId.NewId();
        entry.AttachMedia(newMediaId, EntryFactory.FixedNowUtc.AddDays(10));
        entry.ClearDomainEvents();

        var result = entry.AttachMedia(newMediaId, EntryFactory.FixedNowUtc.AddDays(11));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.MediaAlreadyAttached");
        entry.MediaReferences.Should().HaveCount(2);
        entry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AttachMedia_FromPublishing_ReturnsNotDraftError()
    {
        // Arrange
        var entry = EntryFactory.CreatePublishedEntry();

        var newMediaId = MediaReferenceId.NewId();
        var result = entry.AttachMedia(newMediaId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.NotDraft");
    }

    [Fact]
    public void AttachMedia_FromArchived_ReturnsIsArchivedError()
    {
        // Arrange
        var entry = EntryFactory.CreateArchivedEntry();

        var newMediaId = MediaReferenceId.NewId();
        var result = entry.AttachMedia(newMediaId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.IsArchived");
    }

    // Remove media

    [Fact]
    public void RemoveMedia_FromDraft_RemovesReferenceAndRaisesEvent()
    {
        var entry = EntryFactory.CreateDraftEntryWithMedia();
        var mediaReferenceId = entry.MediaReferences.First().Id;

        var result = entry.RemoveMedia(mediaReferenceId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsSuccess.Should().BeTrue();
        var domainEvent = entry.DomainEvents.OfType<EntryMediaRemovedDomainEvent>().Single();
        domainEvent.EntryId.Should().Be(entry.Id);
        domainEvent.MediaReferenceId.Should().Be(mediaReferenceId);
    }

    [Fact]
    public void RemoveMedia_UnknownMediaReferenceId_ReturnsMediaNotFoundError()
    {
        var entry = EntryFactory.CreateDraftEntryWithMedia();

        var result = entry.RemoveMedia(MediaReferenceId.NewId(), EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.MediaNotFound");
    }

    [Fact]
    public void RemoveMedia_FromPublishing_ReturnsNotDraftError()
    {
        // Arrange
        var entry = EntryFactory.CreatePublishingEntry();
        var mediaReferenceId = entry.MediaReferences.First().Id;

        var result = entry.RemoveMedia(mediaReferenceId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.NotDraft");
    }

    [Fact]
    public void RemoveMedia_FromPublished_ReturnsNotDraftError()
    {
        // Arrange
        var entry = EntryFactory.CreatePublishedEntry();
        var mediaReferenceId = entry.MediaReferences.First().Id;

        var result = entry.RemoveMedia(mediaReferenceId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.NotDraft");
    }

    [Fact]
    public void RemoveMedia_FromArchived_ReturnsIsArchivedError()
    {
        // Arrange
        var entry = EntryFactory.CreateArchivedEntry();
        var mediaReferenceId = entry.MediaReferences.First().Id;

        var result = entry.RemoveMedia(mediaReferenceId, EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.IsArchived");
    }

    // Begin publish

    [Fact]
    public void BeginPublish_FromDraftWithMedia_TransitionsToPublishingAndRaisesEvent()
    {
        var entry = EntryFactory.CreatePublishingEntry();

        entry.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<EntryPublishBeganDomainEvent>();
        entry.Status.Should().Be(EntryStatus.Publishing);
    }

    [Fact]
    public void BeginPublish_FromDraftWithNoMedia_ReturnsNoMediaAttachedError()
    {
        var entry = EntryFactory.CreateDraftEntry();

        var result = entry.BeginPublish(EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.NoMediaAttached");
    }

    [Fact]
    public void BeginPublish_FromPublishing_ReturnsNotDraftError()
    {
        var entry = EntryFactory.CreatePublishingEntry();

        var result = entry.BeginPublish(EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.NotDraft");
    }

    [Fact]
    public void BeginPublish_FromPublished_ReturnsNotDraftError()
    {
        var entry = EntryFactory.CreatePublishedEntry();

        var result = entry.BeginPublish(EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.NotDraft");
    }

    [Fact]
    public void BeginPublish_FromArchived_ReturnsIsArchivedError()
    {
        var entry = EntryFactory.CreateArchivedEntry();

        var result = entry.BeginPublish(EntryFactory.FixedNowUtc.AddDays(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.IsArchived");
    }

    [Fact]
    public void BeginPublish_AfterFailedPublish_ClearsLastPublishFailReason()
    {
        var entry = EntryFactory.CreateEntryWithFailedPublish();
        var lastFailedReason = entry.LastPublishFailReason;

        var result = entry.BeginPublish(EntryFactory.FixedNowUtc.AddDays(10));

        result.IsSuccess.Should().BeTrue();
        entry.LastPublishFailReason.Should().NotBe(lastFailedReason);
    }

    // Complete publish

    [Fact]
    public void CompletePublish_FromPublishing_TransitionsToPublishedAndSetsPublishedAtUtc()
    {
        var entry = EntryFactory.CreatePublishedEntry();

        entry.Status.Should().Be(EntryStatus.Published);
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EntryPublishedDomainEvent>();
        entry.PublishedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void CompletePublish_FromDraft_ReturnsInvalidStatusTransition()
    {
        var entry = EntryFactory.CreateDraftEntry();

        var result = entry.CompletePublish(EntryFactory.FixedNowUtc.AddDays(19));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.InvalidStatusTransition");
    }

    [Fact]
    public void CompletePublish_FromPublished_ReturnsInvalidStatusTransition()
    {
        var entry = EntryFactory.CreatePublishedEntry();

        var result = entry.CompletePublish(EntryFactory.FixedNowUtc.AddDays(19));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.InvalidStatusTransition");
    }

    [Fact]
    public void CompletePublish_FromArchived_ReturnsIsArchivedError()
    {
        var entry = EntryFactory.CreateArchivedEntry();

        var result = entry.CompletePublish(EntryFactory.FixedNowUtc.AddDays(19));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.IsArchived");
    }

    // Fail publish

    [Fact]
    public void FailPublish_FromPublishing_TransitionsToDraftAndSetsReason()
    {
        var entry = EntryFactory.CreateEntryWithFailedPublish();

        entry.Status.Should().Be(EntryStatus.Draft);
        entry.LastPublishFailReason.Should().NotBeNull();
    }

    [Fact]
    public void FailPublish_FromDraft_ReturnsInvalidStatusTransition()
    {
        var entry = EntryFactory.CreateDraftEntry();

        var result = entry.FailPublish("Failing reason" ,EntryFactory.FixedNowUtc.AddDays(19));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.InvalidStatusTransition");
    }

    [Fact]
    public void FailPublish_FromPublished_ReturnsInvalidStatusTransition()
    {
        var entry = EntryFactory.CreatePublishedEntry();

        var result = entry.FailPublish("Failing reason", EntryFactory.FixedNowUtc.AddDays(19));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.InvalidStatusTransition");
    }

    [Fact]
    public void FailPublish_FromArchived_ReturnsIsArchivedError()
    {
        var entry = EntryFactory.CreateArchivedEntry();

        var result = entry.FailPublish("Failing reason", EntryFactory.FixedNowUtc.AddDays(19));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Entry.IsArchived");
    }
}
