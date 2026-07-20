using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Triplog.Media.Application.Behaviors;
using Triplog.Media.Application.MediaItems.Commands.CreateMediaItem;

namespace Triplog.Media.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateMediaItemCommand).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)); // outermost behavior
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // inside logging, outside of handlers
        });

        services.AddValidatorsFromAssembly(assembly);

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
