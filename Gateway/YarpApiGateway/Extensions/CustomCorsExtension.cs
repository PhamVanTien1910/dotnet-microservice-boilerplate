using YarpApiGateway.Configurations;

namespace YarpApiGateway.Extensions;

public static class CustomCorsExtension
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("CorsSettings").Get<CorsSettings>()
            ?? throw new InvalidOperationException("CORS settings are not configured.");

        services.AddCors(options =>
        {
            options.AddPolicy("Development", builder =>
            {
                builder.WithOrigins(corsSettings.AllowedOrigins)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });

            options.AddPolicy("Production", builder =>
            {
                // For Docker environment, allow any origin with credentials for cross-origin cookie support
                if (corsSettings.AllowAnyOriginForDocker)
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAgeSeconds));
                }
                else
                {
                    builder.WithOrigins(corsSettings.AllowedOrigins)
                           .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAgeSeconds));
                }
            });
        });

        return services;
    }
}