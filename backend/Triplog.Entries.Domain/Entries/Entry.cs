using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries.Events;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Domain.Entries;

public sealed class Entry : AggregateRoot<EntryId>
{
    private readonly List<MediaReference> _mediaReferences = [];

    public TripId TripId { get; private set; }

    public OwnerId OwnerId { get; private set; }

    public EntryTitle Title { get; private set; } = null!;

    public EntryBody Body { get; private set; } = null!;

    public DateOnly VisitedOn { get; private set; }

    public EntryStatus Status { get; private set; }

    public Location? Location { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public string? LastPublishFailReason { get; private set; }

    public DateTime? ArchivedAtUtc { get; private set; }

    public IReadOnlyCollection<MediaReference> MediaReferences => _mediaReferences.AsReadOnly();

    private Entry() { } // For EF Core 

    private Entry(EntryId id, TripId tripId, OwnerId ownerId, EntryTitle title, EntryBody entryBody, DateOnly visitedOn, EntryStatus status, Location? location, DateTime createdAtUtc) : base(id)
    {
        TripId = tripId;
        OwnerId = ownerId;
        Title = title;
        Body = entryBody;
        VisitedOn = visitedOn;
        Status = status;
        Location = location;
        CreatedAtUtc = createdAtUtc;
    }

    public static Result<Entry> Create(
        TripId tripId,
        OwnerId ownerId,
        EntryTitle title,
        EntryBody entryBody,
        DateOnly visitedOn,
        Location? location,
        DateTime createTimeUtc)
    {
        var entry = new Entry(EntryId.NewId(), tripId, ownerId, title, entryBody, visitedOn, EntryStatus.Draft, location, createTimeUtc);

        entry.RaiseDomainEvent(new EntryCreatedDomainEvent(entry.Id, tripId, ownerId, createTimeUtc));

        return Result.Success(entry);
    }

    public Result UpdateContent(EntryTitle title, EntryBody entryBody, Location? location, DateOnly visitedOn, DateTime updateTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.IsArchived);

        if (Status != EntryStatus.Draft)
            return Result.Failure(EntryErrors.NotDraft);

        Title = title;
        Body = entryBody;
        Location = location;
        VisitedOn = visitedOn;

        RaiseDomainEvent(new EntryContentUpdatedDomainEvent(Id, updateTimeUtc));

        return Result.Success();
    }

    public Result AttachMedia(MediaReferenceId mediaReferenceId, DateTime updateTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.IsArchived);

        if (Status != EntryStatus.Draft)
            return Result.Failure(EntryErrors.NotDraft);

        if (_mediaReferences.Any(x => x.Id == mediaReferenceId))
            return Result.Failure(EntryErrors.MediaAlreadyAttached);

        var displayOrder = _mediaReferences.Count;

        var mediaReferenceResult = MediaReference.Create(mediaReferenceId, displayOrder);

        if (mediaReferenceResult.IsFailure)
            return Result.Failure(mediaReferenceResult.Error);

        _mediaReferences.Add(mediaReferenceResult.Value);

        RaiseDomainEvent(new EntryMediaAttachedDomainEvent(Id, mediaReferenceId, updateTimeUtc));

        return Result.Success();
    }

    public Result RemoveMedia(MediaReferenceId mediaReferenceId, DateTime updateTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.IsArchived);

        if (Status is not EntryStatus.Draft)
            return Result.Failure(EntryErrors.NotDraft);

        if (!_mediaReferences.Any(x => x.Id == mediaReferenceId))
            return Result.Failure(EntryErrors.MediaNotFound);

        _mediaReferences.RemoveAll(x => x.Id == mediaReferenceId);

        RaiseDomainEvent(new EntryMediaRemovedDomainEvent(Id, mediaReferenceId, updateTimeUtc));

        return Result.Success();
    }

    public Result BeginPublish(DateTime beginPublishTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.IsArchived);

        if (Status is not EntryStatus.Draft)
            return Result.Failure(EntryErrors.NotDraft);

        if (_mediaReferences.Count == 0)
            return Result.Failure(EntryErrors.NoMediaAttached);

        Status = EntryStatus.Publishing;
        LastPublishFailReason = null;

        RaiseDomainEvent(new EntryPublishBeganDomainEvent(Id, OwnerId, [.. _mediaReferences.Select(x => x.Id)], beginPublishTimeUtc));

        return Result.Success();
    }

    public Result CompletePublish(DateTime publishTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.IsArchived);

        if (Status is not EntryStatus.Publishing)
            return Result.Failure(EntryErrors.InvalidStatusTransition);

        Status = EntryStatus.Published;
        PublishedAtUtc = publishTimeUtc;

        RaiseDomainEvent(new EntryPublishedDomainEvent(Id, OwnerId, publishTimeUtc));

        return Result.Success();
    }

    public Result FailPublish(string reason, DateTime failPublishTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.IsArchived);

        if (Status is not EntryStatus.Publishing)
            return Result.Failure(EntryErrors.InvalidStatusTransition);

        Status = EntryStatus.Draft;
        LastPublishFailReason = reason;

        RaiseDomainEvent(new EntryPublishFailedDomainEvent(Id, OwnerId, reason, failPublishTimeUtc));

        return Result.Success();
    }

    public Result Archive(DateTime archiveTimeUtc)
    {
        if (Status == EntryStatus.Archived)
            return Result.Failure(EntryErrors.AlreadyArchived);

        Status = EntryStatus.Archived;
        ArchivedAtUtc = archiveTimeUtc;

        RaiseDomainEvent(new EntryArchivedDomainEvent(Id, archiveTimeUtc));

        return Result.Success();
    }
}
