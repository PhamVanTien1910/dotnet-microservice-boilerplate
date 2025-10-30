using NotificationService.Email;
using BuildingBlocks.Application.Messaging.Abstractions;
using Shared.IntegrationEvents;

namespace NotificationService.Handlers
{
    public class PasswordResetRequestedEmailHandler : IIntegrationEventHandler<PasswordResetRequestedIntegrationEvent>
    {
        private readonly ILogger<PasswordResetRequestedEmailHandler> _logger;
        private readonly IEmailSender _sender;
        private readonly EmailComposer _composer;

        public PasswordResetRequestedEmailHandler(
            ILogger<PasswordResetRequestedEmailHandler> logger, 
            IEmailSender sender, 
            EmailComposer composer)
        { 
            _logger = logger; 
            _sender = sender; 
            _composer = composer; 
        }

        public async Task HandleAsync(PasswordResetRequestedIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending password reset email to {Email}", @event.UserEmail);
            
            try
            {
                var (subject, html) = await _composer.ComposeResetAsync(
                    @event.UserEmail,
                    @event.UserRole,
                    @event.ResetToken,
                    @event.ExpiresAt,
                    cancellationToken);
                    
                await _sender.SendAsync(@event.UserEmail, subject, html, cancellationToken);
                
                _logger.LogInformation("Password reset email sent successfully to {Email}", @event.UserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", @event.UserEmail);
                throw; // Re-throw to let the caller handle it appropriately
            }
        }
    }
}
