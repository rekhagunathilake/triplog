using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Triplog.Entries.Api;

namespace Triplog.IntegrationTests.Fixtures;

public class EntriesApiFactory(TriplogSystemFixture triplogSystemFixture) : WebApplicationFactory<EntriesApi>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // ensures auto-migrate + dev convenience run

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:entries"] = triplogSystemFixture.EntriesConnectionString,
                ["ConnectionStrings:rabbitmq"] = triplogSystemFixture.RabbitMqConnectionString,
                ["Auth:JwtSecret"] = triplogSystemFixture.JwtSecret,
                ["Auth:OwnerEmail"] = triplogSystemFixture.OwnerEmail,
                ["Auth:PublicOwnerId"] = triplogSystemFixture.PublicOwnerId.ToString(),
            });
        });
    }
}
