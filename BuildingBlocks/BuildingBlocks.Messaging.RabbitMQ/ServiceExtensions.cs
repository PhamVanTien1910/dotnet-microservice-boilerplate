using BuildingBlocks.Application.Messaging.Abstractions;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ;

public static class ServiceExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string sectionName)
    {
        services.AddOptions<RabbitMQOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart(); // Ensures validation runs on app startup, failing fast if config is invalid

        // Register the core services as singletons
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var rabbitSettings = sp.GetRequiredService<IOptions<RabbitMQOptions>>().Value;
            var factory = new ConnectionFactory()
            {
                HostName = rabbitSettings.HostName,
                Port = rabbitSettings.Port,
                UserName = rabbitSettings.UserName,
                Password = rabbitSettings.Password,
                VirtualHost = rabbitSettings.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(rabbitSettings.ConnectionTimeout),
                RequestedHeartbeat = TimeSpan.FromSeconds(rabbitSettings.RequestedHeartbeat)
            };

            return factory;
        });
        services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        return services;
    }
}
