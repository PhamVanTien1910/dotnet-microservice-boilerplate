using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YarpApiGateway.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Add basic health check
        healthChecksBuilder.AddCheck("gateway", () => HealthCheckResult.Healthy("Gateway is running"));

        return services;
    }
}
