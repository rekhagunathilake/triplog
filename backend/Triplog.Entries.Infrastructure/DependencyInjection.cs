using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Infrastructure.Persistence;
using Triplog.Entries.Infrastructure.Persistence.Interceptors;
using Triplog.Entries.Infrastructure.Persistence.Queries;
using Triplog.Entries.Infrastructure.Persistence.Repositories;

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

        return services;
    }
}
