using System.Reflection;
using BuildingBlocks.MediatR;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace IAM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ConfigureCqrs(services);
        ConfigureMapster(services);

        return services;
    }

    private static void ConfigureMapster(IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
    }

    private static void ConfigureCqrs(IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
    }

}