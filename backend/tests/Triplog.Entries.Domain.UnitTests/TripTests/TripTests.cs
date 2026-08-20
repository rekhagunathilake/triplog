using FluentAssertions;
using Triplog.Entries.Domain.Trips;
using Triplog.Entries.Domain.Trips.Events;
using Triplog.Entries.Domain.UnitTests.TestHelpers;

namespace Triplog.Entries.Domain.UnitTests.TripTests;

public class TripTests
{
    // Create

    [Fact]
    public void Crate_WithValidInputs_InitializesTripInPlanningStatus()
    {
        // Act
        var trip = TripFactory.CreatePlanningTrip();

        // Assert
        trip.Id.Should().NotBe(default(TripId));
        trip.Status.Should().Be(TripStatus.Planning);
        trip.CreatedAtUtc.Should().Be(TripFactory.FixedNowUtc);
        trip.ArchivedAtUtc.Should().BeNull();
        trip.Title.Should().NotBeNull();
        trip.Dates.Should().NotBeNull();
        trip.OwnerId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithValidInputs_RaisesTripCreatedDomainEvent()
    {
        // Act
        var trip = TripFactory.CreatePlanningTrip();

        // Assert
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripCreatedDomainEvent>();
        var domainEvent = trip.DomainEvents.OfType<TripCreatedDomainEvent>().Single();
        domainEvent.TripId.Should().Be(trip.Id);
        domainEvent.OwnerId.Should().Be(trip.OwnerId);
        domainEvent.OccurredOnUtc.Should().Be(TripFactory.FixedNowUtc);
    }

    // Update details

    [Fact]
    public void UpdateDetails_FromPlanning_AppliesChangesAndRaisesEvent()
    {
        // Arrange
        var trip = TripFactory.CreatePlanningTrip();
        trip.ClearDomainEvents();
        var newTitle = TripFactory.CreateTitle("Updated title");
        var newDates = TripFactory.CreateDateRange(
            DateOnly.FromDateTime(TripFactory.FixedNowUtc).AddDays(60),
            DateOnly.FromDateTime(TripFactory.FixedNowUtc).AddDays(74));
        var updateTime = TripFactory.FixedNowUtc.AddDays(2);

        // Act
        var result = trip.UpdateDetails(newTitle, "Updated description", newDates, updateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        trip.Title.Should().Be(newTitle);
        trip.Description.Should().Be("Updated description");
        trip.Dates.Should().Be(newDates);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripDetailsUpdatedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(updateTime);
    }

    [Fact]
    public void UpdateDetails_FromActive_AppliesChangesAndRaisesEvent()
    {
        var trip = TripFactory.CreateActiveTrip();
        trip.ClearDomainEvents();
        var newTitle = TripFactory.CreateTitle("Active updated");
        var updateTime = TripFactory.FixedNowUtc.AddDays(3);

        var result = trip.UpdateDetails(newTitle, "Mid-trip update", trip.Dates, updateTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Active);
        trip.Title.Should().Be(newTitle);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripDetailsUpdatedDomainEvent>();
    }

    [Fact]
    public void UpdateDetails_FromCompleted_AppliesChangesAndRaisesEvent()
    {
        var trip = TripFactory.CreateCompletedTrip();
        trip.ClearDomainEvents();
        var newTitle = TripFactory.CreateTitle("Completed updated");
        var updateTime = TripFactory.FixedNowUtc.AddDays(20);

        var result = trip.UpdateDetails(newTitle, "Post-trip update", trip.Dates, updateTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Completed);
        trip.Title.Should().Be(newTitle);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripDetailsUpdatedDomainEvent>();
    }

    [Fact]
    public void UpdateDetails_FromArchived_ReturnsIsArchivedError()
    {
        var trip = TripFactory.CreateArchivedTrip();
        var originalTitle = trip.Title;
        trip.ClearDomainEvents();
        var newTitle = TripFactory.CreateTitle("Should not apply");

        var result = trip.UpdateDetails(newTitle, "New", trip.Dates, TripFactory.FixedNowUtc.AddDays(40));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.IsArchived);
        trip.Title.Should().Be(originalTitle);
        trip.DomainEvents.Should().BeEmpty();
    }

    // Activate

    [Fact]
    public void Activate_FromPlanning_TransitionsToActiveAndRaisesEvent()
    {
        var trip = TripFactory.CreatePlanningTrip();
        trip.ClearDomainEvents();
        var activateTime = TripFactory.FixedNowUtc.AddDays(1);

        var result = trip.Activate(activateTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Active);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripActivatedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(activateTime);
    }

    [Fact]
    public void Activate_FromActive_ReturnsInvalidStatusTransition()
    {
        var trip = TripFactory.CreateActiveTrip();
        trip.ClearDomainEvents();

        var result = trip.Activate(TripFactory.FixedNowUtc.AddDays(5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.InvalidStatusTransition);
        trip.Status.Should().Be(TripStatus.Active);
        trip.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_FromCompleted_ReturnsInvalidStatusTransition()
    {
        var trip = TripFactory.CreateCompletedTrip();
        trip.ClearDomainEvents();

        var result = trip.Activate(TripFactory.FixedNowUtc.AddDays(20));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.InvalidStatusTransition);
        trip.Status.Should().Be(TripStatus.Completed);
        trip.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_FromArchived_ReturnsIsArchivedError()
    {
        var trip = TripFactory.CreateArchivedTrip();
        trip.ClearDomainEvents();

        var result = trip.Activate(TripFactory.FixedNowUtc.AddDays(40));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.IsArchived);
        trip.Status.Should().Be(TripStatus.Archived);
        trip.DomainEvents.Should().BeEmpty();
    }

    // Complete

    [Fact]
    public void Complete_FromActive_TransitionsToCompletedAndRaisesEvent()
    {
        var trip = TripFactory.CreateActiveTrip();
        trip.ClearDomainEvents();
        var completeTime = TripFactory.FixedNowUtc.AddDays(15);

        var result = trip.Complete(completeTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Completed);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripCompletedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(completeTime);
    }

    [Fact]
    public void Complete_FromPlanning_ReturnsInvalidStatusTransition()
    {
        var trip = TripFactory.CreatePlanningTrip();
        trip.ClearDomainEvents();

        var result = trip.Complete(TripFactory.FixedNowUtc.AddDays(5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.InvalidStatusTransition);
        trip.Status.Should().Be(TripStatus.Planning);
        trip.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Complete_FromCompleted_ReturnsInvalidStatusTransition()
    {
        var trip = TripFactory.CreateCompletedTrip();
        trip.ClearDomainEvents();

        var result = trip.Complete(TripFactory.FixedNowUtc.AddDays(20));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.InvalidStatusTransition);
        trip.Status.Should().Be(TripStatus.Completed);
        trip.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Complete_FromArchived_ReturnsIsArchivedError()
    {
        var trip = TripFactory.CreateArchivedTrip();
        trip.ClearDomainEvents();

        var result = trip.Complete(TripFactory.FixedNowUtc.AddDays(40));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.IsArchived);
        trip.Status.Should().Be(TripStatus.Archived);
        trip.DomainEvents.Should().BeEmpty();
    }

    // Archive

    [Fact]
    public void Archive_FromPlanning_TransitionsToArchivedAndRaisesEvent()
    {
        var trip = TripFactory.CreatePlanningTrip();
        trip.ClearDomainEvents();
        var archiveTime = TripFactory.FixedNowUtc.AddDays(5);

        var result = trip.Archive(archiveTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Archived);
        trip.ArchivedAtUtc.Should().Be(archiveTime);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripArchivedDomainEvent>()
            .Which.OccurredOnUtc.Should().Be(archiveTime);
    }

    [Fact]
    public void Archive_FromActive_TransitionsToArchivedAndRaisesEvent()
    {
        var trip = TripFactory.CreateActiveTrip();
        trip.ClearDomainEvents();
        var archiveTime = TripFactory.FixedNowUtc.AddDays(10);

        var result = trip.Archive(archiveTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Archived);
        trip.ArchivedAtUtc.Should().Be(archiveTime);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripArchivedDomainEvent>();
    }

    [Fact]
    public void Archive_FromCompleted_TransitionsToArchivedAndRaisesEvent()
    {
        var trip = TripFactory.CreateCompletedTrip();
        trip.ClearDomainEvents();
        var archiveTime = TripFactory.FixedNowUtc.AddDays(30);

        var result = trip.Archive(archiveTime);

        result.IsSuccess.Should().BeTrue();
        trip.Status.Should().Be(TripStatus.Archived);
        trip.ArchivedAtUtc.Should().Be(archiveTime);
        trip.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TripArchivedDomainEvent>();
    }

    [Fact]
    public void Archive_FromArchived_ReturnsAlreadyArchivedError()
    {
        var trip = TripFactory.CreateArchivedTrip();
        var originalArchivedAt = trip.ArchivedAtUtc;
        trip.ClearDomainEvents();

        var result = trip.Archive(TripFactory.FixedNowUtc.AddDays(60));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TripErrors.AlreadyArchived);
        trip.Status.Should().Be(TripStatus.Archived);
        trip.ArchivedAtUtc.Should().Be(originalArchivedAt);
        trip.DomainEvents.Should().BeEmpty();
    }
}
