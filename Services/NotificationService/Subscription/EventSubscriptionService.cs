
using BuildingBlocks.Application.Messaging.Abstractions;
using NotificationService.Handlers;
using Shared.IntegrationEvents;

namespace NotificationService.Subscription
{
    public class EventSubscriptionService : BackgroundService
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<EventSubscriptionService> _logger;

        public EventSubscriptionService(IEventBus eventBus, ILogger<EventSubscriptionService> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventSubscriptionService starting...");

            try
            {
                _logger.LogInformation("Subscribing to UserRegisteredIntegrationEvent...");
                await _eventBus.SubscribeAsync<UserRegisteredIntegrationEvent, UserRegisteredEmailHandler>(stoppingToken);
                _logger.LogInformation("Successfully subscribed to UserRegisteredIntegrationEvent");

                _logger.LogInformation("Subscribing to PasswordResetRequestedIntegrationEvent...");
                await _eventBus.SubscribeAsync<PasswordResetRequestedIntegrationEvent, PasswordResetRequestedEmailHandler>(stoppingToken);
                _logger.LogInformation("Successfully subscribed to PasswordResetRequestedIntegrationEvent");

                _logger.LogInformation("All event subscriptions completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during event subscription setup");
                throw;
            }

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
