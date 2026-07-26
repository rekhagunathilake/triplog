using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Infrastructure.Persistence.Interceptors;
using Triplog.Media.Infrastructure.Persistence.Queries;
using Triplog.Media.Infrastructure.Persistence.Repositories;

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

        return services;
    }
}
