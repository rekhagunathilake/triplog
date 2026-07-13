namespace Triplog.Entries.Api.Endpoints.Requests
{
    public sealed record CreateTripRequest(
        string Title,
        string? Description,
        DateOnly StartDate,
        DateOnly EndDate
        );
}
