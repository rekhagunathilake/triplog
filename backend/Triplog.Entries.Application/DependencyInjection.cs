using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Triplog.Entries.Application.Behaviors;

namespace Triplog.Entries.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)); // outermost behavior
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // inside logging, outside of handlers
        });

        services.AddValidatorsFromAssembly(assembly);

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
