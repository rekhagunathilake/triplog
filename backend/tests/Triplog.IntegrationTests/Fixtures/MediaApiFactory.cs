using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Triplog.Media.Api;

namespace Triplog.IntegrationTests.Fixtures;

public class MediaApiFactory(TriplogSystemFixture triplogSystemFixture) : WebApplicationFactory<MediaApi>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:media"] = triplogSystemFixture.MediaConnectionString,
                ["ConnectionStrings:rabbitmq"] = triplogSystemFixture.RabbitMqConnectionString,
                ["Minio:Endpoint"] = triplogSystemFixture.MinioEndpoint,
                ["Minio:RootUser"] = triplogSystemFixture.MinioAccessKey,
                ["Minio:RootPassword"] = triplogSystemFixture.MinioSecretKey,
            });
        });
    }
}
