using FluentAssertions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Entries.Events;
using Triplog.Entries.Domain.UnitTests.TestHelpers;

namespace Triplog.Entries.Domain.UnitTests.EntryTests;

public class EntryTests
{
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
}
