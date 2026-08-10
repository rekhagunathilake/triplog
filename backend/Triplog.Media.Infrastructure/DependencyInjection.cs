using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Infrastructure.Messaging.Consumers;
using Triplog.Media.Infrastructure.Persistence.Interceptors;
using Triplog.Media.Infrastructure.Persistence.Queries;
using Triplog.Media.Infrastructure.Persistence.Repositories;
using Triplog.Media.Infrastructure.Storage;

namespace Triplog.Media.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Interceptor must be resolvable via IoC so it can inject IPublisher (MediatR)
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<TriplogMediaDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("media"));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        // Write side
        services.AddScoped<IMediaItemRepository, MediaItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Read side
        services.AddScoped<IMediaItemQueries, MediaItemQueries>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TriplogMediaDbContext).Assembly);
        });

        services.AddMassTransit(bus =>
        {
            // Auto-discover consumers/sagas in this assembly
            bus.AddConsumers(typeof(FinalizeMediaCommandConsumer).Assembly);

            bus.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConnectionString = configuration.GetConnectionString("rabbitmq") ??
                    throw new InvalidOperationException("Missing 'rabbitmq' connection string.");

                cfg.Host(new Uri(rabbitConnectionString));

                // Retry on transient DB errors (concurrent update, deadlock, or connectin blip)
                cfg.UseMessageRetry(r =>
                {
                    r.Interval(5, TimeSpan.FromMilliseconds(200));
                });

                // Auto-configure receive endpoints per consumer
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<IMinioClient>(sp =>
        {
            var endpoint = configuration["Minio:Endpoint"]
            ?? throw new InvalidOperationException("Missing Minio:Endpoint configuration.");
            var user = configuration["Minio:RootUser"] ?? "minioadmin";
            var password = configuration["Minio:RootPassword"] ?? "minioadmin";

            // Aspire injects Minio:Endpoint as "http://localhost:9000" - string scheme
            var host = new Uri(endpoint).Authority;

            return new MinioClient()
            .WithEndpoint(host)
            .WithCredentials(user, password)
            .WithSSL(false)
            .Build();
        });

        services.AddSingleton<IObjectStorage, MinioObjectStorage>();

        return services;
    }
}
