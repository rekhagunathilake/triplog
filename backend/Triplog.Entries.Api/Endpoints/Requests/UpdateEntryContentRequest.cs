namespace Triplog.Entries.Api.Endpoints.Requests;

public sealed record UpdateEntryContentRequest(
    string Title,
    string Body,
    DateOnly VisitedOn,
    string? LocationName,
    double? Latitude,
    double? Longitude
    );
