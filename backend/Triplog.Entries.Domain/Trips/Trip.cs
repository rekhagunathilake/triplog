using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips.Events;

namespace Triplog.Entries.Domain.Trips;

public sealed class Trip : AggregateRoot<TripId>
{
    public OwnerId OwnerId { get; private set; }

    public TripTitle Title { get; private set; } = null!;
    
    public string? Description { get; private set; }

    public DateRange Dates { get; private set; } = null!;

    public TripStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ArchivedAtUtc { get; private set; }

    private Trip() { } // For EF Core 

    private Trip(TripId id, OwnerId ownerId, TripTitle title, string? description, DateRange dates, TripStatus status, DateTime createdAtUtc) : base(id)
    {
        OwnerId = ownerId;
        Title = title;
        Description = description;
        Dates = dates;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    public static Result<Trip> Create(
        OwnerId ownerId,
        TripTitle title,
        string? description,
        DateRange dates,
        DateTime nowUtc) 
    {
        var trip = new Trip(TripId.NewId(), ownerId, title, description, dates, TripStatus.Planning, nowUtc);

        trip.RaiseDomainEvent(new TripCreatedDomainEvent(trip.Id, ownerId, nowUtc));

        return Result.Success(trip);
    }

    public Result UpdateDetails(TripTitle title, string? description, DateRange dates, DateTime nowUtc)
    {
        if (Status == TripStatus.Archived)
            return Result.Failure(TripErrors.IsArchived);

        Title = title;
        Description = description;
        Dates = dates;

        RaiseDomainEvent(new TripDetailsUpdatedDomainEvent(Id, nowUtc));

        return Result.Success();
    }

    public Result Activate(DateTime nowUtc)
    {
        if (Status == TripStatus.Archived)
            return Result.Failure(TripErrors.IsArchived);

        if (Status is not TripStatus.Planning)
            return Result.Failure(TripErrors.InvalidStatusTransition);

        Status = TripStatus.Active;

        RaiseDomainEvent(new TripActivatedDomainEvent(Id, nowUtc));

        return Result.Success();
    }

    public Result Complete(DateTime nowUtc)
    {
        if (Status == TripStatus.Archived)
            return Result.Failure(TripErrors.IsArchived);

        if (Status is not TripStatus.Active)
            return Result.Failure(TripErrors.InvalidStatusTransition);

        Status = TripStatus.Completed;

        RaiseDomainEvent(new TripCompletedDomainEvent(Id, nowUtc));

        return Result.Success();
    }

    public Result Archive(DateTime nowUtc)
    {
        if (Status == TripStatus.Archived)
            return Result.Failure(TripErrors.AlreadyArchived);

        Status = TripStatus.Archived;
        ArchivedAtUtc = nowUtc;

        RaiseDomainEvent(new TripArchivedDomainEvent(Id, nowUtc));

        return Result.Success();
    }
}
