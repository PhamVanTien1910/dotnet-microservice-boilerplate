using System.Reflection;
using BuildingBlocks.MediatR.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.MediatR;

public static class ServiceExtension
{
    public static IServiceCollection AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
    {

        // Register FluentValidation
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(FluentValidationBehavior<,>));
        });

        return services;
    }
}
