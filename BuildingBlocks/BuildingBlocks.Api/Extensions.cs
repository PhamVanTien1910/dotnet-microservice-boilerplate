using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using BuildingBlocks.Api.VersionedSwagger;

namespace BuildingBlocks.Api;

public static class ApiExtensions
{
    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services, Action<IApiBuilder> configure)
    {
        services.AddOptions<VersionedSwaggerOptions>().BindConfiguration("Swagger");

        var builder = new ApiBuilder(services);
        configure(builder);
        return services;
    }

    public static IApplicationBuilder UseVersionedSwaggerUI(this IApplicationBuilder app, string? titleOverride = null)
    {
        // Only enable Swagger if the required services are registered (allows skipping Swagger in non-dev)
        var swaggerProvider = app.ApplicationServices.GetService<ISwaggerProvider>();
        var versionProvider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();

        if (swaggerProvider is not null && versionProvider is not null)
        {
            
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var desc in versionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                        $"{titleOverride ?? "API"} {desc.ApiVersion}");
                }

                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
                options.EnableTryItOutByDefault();
            });
        }

        return app;
    }
}
