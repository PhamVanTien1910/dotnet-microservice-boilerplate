using YarpApiGateway.Configurations;
using StackExchange.Redis;

namespace YarpApiGateway.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind and validate Redis options
        services.AddOptions<RedisOptions>()
                .Bind(configuration.GetSection(RedisOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();


        // Get Redis options for configuration
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();

        if (redisOptions?.Enabled == true && !string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
        {
            // Configure Redis connection
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RedisConnection");

                try
                {
                    var configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                    configurationOptions.ConnectTimeout = redisOptions.TimeoutSeconds * 1000;
                    configurationOptions.SyncTimeout = redisOptions.TimeoutSeconds * 1000;

                    var connection = ConnectionMultiplexer.Connect(configurationOptions);

                    logger.LogInformation("Redis connection established successfully");
                    return connection;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to establish Redis connection");
                    throw;
                }
            });

            // Add Redis database accessor
            services.AddScoped<IDatabase>(provider =>
            {
                var connection = provider.GetRequiredService<IConnectionMultiplexer>();
                return connection.GetDatabase();
            });

            // Add distributed caching using Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = "BoilerplateGateway";
            });
        }

        return services;
    }
}
