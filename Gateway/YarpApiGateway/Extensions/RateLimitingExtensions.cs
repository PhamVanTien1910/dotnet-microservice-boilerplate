using YarpApiGateway.Configurations;
using Microsoft.AspNetCore.RateLimiting;

namespace YarpApiGateway.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind and validate rate limiting options
        services.AddOptions<RateLimitingOptions>()
                .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        // Get rate limiting options for configuration
        var rateLimitingOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        if (rateLimitingOptions.Enabled)
        {
            services.AddRateLimiter(rateLimiterOptions =>
            {
                // Configure fixed window rate limiter
                rateLimiterOptions.AddFixedWindowLimiter(rateLimitingOptions.FixedWindow.PolicyName, options =>
                {
                    options.Window = TimeSpan.FromSeconds(rateLimitingOptions.FixedWindow.WindowSeconds);
                    options.PermitLimit = rateLimitingOptions.FixedWindow.PermitLimit;
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0; // No queuing
                });

                // Global rate limiting policy (optional)
                rateLimiterOptions.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = rateLimitingOptions.FixedWindow.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingOptions.FixedWindow.WindowSeconds)
                        }));

                // Configure rejection response
                rateLimiterOptions.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429; // Too Many Requests

                    if (context.Lease.TryGetMetadata(System.Threading.RateLimiting.MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
                    }

                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
                };
            });
        }

        return services;
    }
}
