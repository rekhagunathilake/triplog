namespace Triplog.Entries.Api.Endpoints.Requests
{
    public sealed record UpdateTripDetailsRequest(
        string Title,
        string? Description,
        DateOnly StartDate,
        DateOnly EndDate
        );
}
