using NotificationService.Email;
using BuildingBlocks.Application.Messaging.Abstractions;
using Shared.IntegrationEvents;

namespace NotificationService.Handlers
{
    public class UserRegisteredEmailHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
    {
        private readonly ILogger<UserRegisteredEmailHandler> _logger;
        private readonly IEmailSender _sender;
        private readonly EmailComposer _composer;

        public UserRegisteredEmailHandler(ILogger<UserRegisteredEmailHandler> logger, IEmailSender sender, EmailComposer composer)
        { _logger = logger; _sender = sender; _composer = composer; }

        public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("=== HANDLER INVOKED === UserRegisteredEmailHandler.HandleAsync called for user {UserId}", @event.UserId);
            _logger.LogInformation("Sending verification email to {Email}", @event.UserEmail);
            
            // Validate required fields
            if (string.IsNullOrEmpty(@event.ConfirmationToken))
            {
                _logger.LogError("ConfirmationToken is null or empty for user {UserId} - {Email}", @event.UserId, @event.UserEmail);
                return;
            }
            
            try
            {
                var (subject, html) = await _composer.ComposeVerificationAsync(
                    @event.UserEmail,
                    @event.UserFirstName,
                    @event.UserLastName,
                    @event.ConfirmationToken,
                    DateTimeOffset.UtcNow.AddHours(24), // if publisher didn't include expiry
                    cancellationToken);
                    
                await _sender.SendAsync(@event.UserEmail, subject, html, cancellationToken);
                
                _logger.LogInformation("Verification email sent successfully to {Email}", @event.UserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", @event.UserEmail);
                throw;
            }
        }
    }
}
