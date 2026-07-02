using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Queries.GetEntryById;

public sealed record EntryDto(
    Guid Id,
    Guid TripId,
    Guid OwnerId,
    string Title,
    string Body,
    LocationDto? Location,
    DateOnly VisitedOn,
    EntryStatus Status,
    IReadOnlyList<MediaReferenceDto> MediaReferences,
    DateTime CreatedAtUtc,
    DateTime? PublishedAtUtc,
    DateTime? ArchivedAtUtc,
    string? LastPublishFailReason);

public sealed record LocationDto(string Name, double Latitude, double Longitude);
public sealed record MediaReferenceDto(Guid Id, int DisplayOrder);