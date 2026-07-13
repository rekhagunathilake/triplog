namespace Triplog.Entries.Api.Endpoints.Requests;

public sealed record CreateEntryRequest(
    string Title,
    string Body,
    DateOnly VisitedOn,
    string? LocationName,
    double? Latitude,
    double? Longitude);
