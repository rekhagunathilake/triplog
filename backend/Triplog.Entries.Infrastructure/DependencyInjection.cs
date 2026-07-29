using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Infrastructure.Persistence;
using Triplog.Entries.Infrastructure.Persistence.Interceptors;
using Triplog.Entries.Infrastructure.Persistence.Queries;
using Triplog.Entries.Infrastructure.Persistence.Repositories;
using Triplog.Entries.Infrastructure.Persistence.Sagas;

namespace Triplog.Entries.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Interceptor must be resolvable via IoC so it can inject IPublisher (MediatR)
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<TriplogEntriesDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("entries"));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        // Write side
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IEntryRepository, EntryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Read side
        services.AddScoped<ITripQueries, TripQueries>();
        services.AddScoped<IEntryQueries, EntryQueries>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TriplogEntriesDbContext).Assembly);
        });

        services.AddMassTransit(bus =>
        {
            // Note: integration handlers (EntryPublishBeganIntegrationHandler etc.) are
            // MediatR INotificationHandler — not MassTransit IConsumer. They're wired via
            // MediatR's assembly scan above, not here. AddConsumers is registered here so
            // any future MassTransit consumers in this assembly get picked up automatically.
            // Auto-discover consumers/sagas in this assembly
            bus.AddConsumers(typeof(TriplogEntriesDbContext).Assembly);

            // The saga state machine — persisted via EF Core to the same DbContext
            bus.AddSagaStateMachine<PublishEntrySaga, PublishEntrySagaState>()
                .EntityFrameworkRepository(cfg =>
                {
                    cfg.ExistingDbContext<TriplogEntriesDbContext>();
                    cfg.UsePostgres();
                });

            bus.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConnectionString = configuration.GetConnectionString("rabbitmq")
                ?? throw new InvalidOperationException("Missing 'rabbitmq' connection string.");

                cfg.Host(new Uri(rabbitConnectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
