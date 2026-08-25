using FluentAssertions;
using System.Net;
using Triplog.IntegrationTests.Fixtures;

namespace Triplog.IntegrationTests;

public class TripCrudTests(TriplogSystemFixture triplogSystemFixture) : IClassFixture<TriplogSystemFixture>
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private ApiClient CreateApi() => new(triplogSystemFixture.Entries.CreateClient(), TestUserId);

    [Fact]
    public async Task Create_Trip_Returns_201_With_Id()
    {
        var api = CreateApi();

        var response = await api.PostRawAsync("/trips", new
        {
            title = "France 2026",
            description = "Two weeks in Paris",
            startDate = "2026-08-01",
            endDate = "2026-08-14"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Created_Trip_Appears_In_List()
    {
        var api = CreateApi();

        var created = await api.PostAsync<CreatedResponse>("/trips", new
        {
            title = "Japan 2027",
            startDate = "2027-04-01",
            endDate = "2027-04-14",
        });

        var trips = await api.GetAsync<List<TripSummary>>("/trips");

        trips.Should().Contain(t => t.Id == created.Id && t.Title == "Japan 2027");
    }

    [Fact]
    public async Task Get_By_Id_Returns_The_Trip()
    {
        var api = CreateApi();

        var created = await api.PostAsync<CreatedResponse>("/trips", new
        {
            title = "Norway 2026",
            startDate = "2026-06-01",
            endDate = "2026-06-10",
        });

        var trip = await api.GetAsync<TripDetail>($"/trips/{created.Id}");

        trip.Title.Should().Be("Norway 2026");
        trip.Status.Should().Be("Planning");
        trip.StartDate.Should().Be("2026-06-01");
    }

    [Fact]
    public async Task Activate_Then_Complete_Transitions_Correctly()
    {
        var api = CreateApi();
        var created = await api.PostAsync<CreatedResponse>("/trips", new
        {
            title = "Iceland 2026",
            startDate = "2026-09-01",
            endDate = "2026-09-10",
        });

        await api.PostVoidAsync($"/trips/{created.Id}/activate");
        var afterActivate = await api.GetAsync<TripDetail>($"/trips/{created.Id}");
        afterActivate.Status.Should().Be("Active");

        await api.PostVoidAsync($"/trips/{created.Id}/complete");
        var afterComplete = await api.GetAsync<TripDetail>($"/trips/{created.Id}");
        afterComplete.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Activate_A_Completed_Trip_Returns_409()
    {
        var api = CreateApi();
        var created = await api.PostAsync<CreatedResponse>("/trips", new
        {
            title = "France 2026",
            startDate = "2026-10-01",
            endDate = "2026-10-10",
        });

        await api.PostVoidAsync($"/trips/{created.Id}/activate");
        await api.PostVoidAsync($"/trips/{created.Id}/complete");

        // Try to activate again — should conflict
        var response = await api.PostRawAsync($"/trips/{created.Id}/activate");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Local DTOs for JSON deserialization

    private record CreatedResponse(Guid Id);
    private record TripSummary(Guid Id, string Title, string StartDate, string EndDate, string Status);
    private record TripDetail(Guid Id, string Title, string? Description, string StartDate, string EndDate, string Status);
}