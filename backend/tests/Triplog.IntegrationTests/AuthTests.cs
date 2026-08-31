using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Triplog.IntegrationTests.Fixtures;

namespace Triplog.IntegrationTests;

public class AuthTests(TriplogSystemFixture triplogSystemFixture) : IClassFixture<TriplogSystemFixture>
{
    [Fact]
    public async Task Create_Trip_Without_Token_Returns_401()
    {
        var httpClient = triplogSystemFixture.Entries.CreateClient();

        var response = await httpClient.PostAsJsonAsync("/trips", new
        {
            title = "no-auth",
            startDate = "2026-01-01",
            endDate = "2026-01-02",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Trip_With_Wrong_Owner_Email_Returns_403()
    {
        var wrongToken = TestJwt.ForEmail("nobody@example.com", triplogSystemFixture.JwtSecret);
        var api = new ApiClient(triplogSystemFixture.Entries.CreateClient(), wrongToken);

        var response = await api.PostRawAsync("/trips", new
        {
            title = "wrong-owner",
            startDate = "2026-01-01",
            endDate = "2026-01-02",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task List_Trips_Without_Token_Returns_200()
    {
        // GET endpoints are public — no auth required
        var httpClient = triplogSystemFixture.Entries.CreateClient();

        var response = await httpClient.GetAsync("/trips");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}