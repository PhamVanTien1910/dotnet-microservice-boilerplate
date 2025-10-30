using BuildingBlocks.Application.Messaging.Abstractions;
using NotificationService.Handlers;
using Shared.IntegrationEvents;

namespace NotificationService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationEventHandlers(this IServiceCollection services)
    {
        // Register integration event handlers
        services.AddScoped<UserRegisteredEmailHandler>();
        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredEmailHandler>();

        services.AddScoped<PasswordResetRequestedEmailHandler>();
        services.AddScoped<IIntegrationEventHandler<PasswordResetRequestedIntegrationEvent>, PasswordResetRequestedEmailHandler>();
        return services;
    }
}
