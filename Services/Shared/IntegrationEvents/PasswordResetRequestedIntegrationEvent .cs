using BuildingBlocks.Application.Messaging.Events;

namespace Shared.IntegrationEvents
{
    public class PasswordResetRequestedIntegrationEvent : IntegrationEvent
    {
        public Guid UserId { get; init; }
        public string UserEmail { get; init; } = default!;
        public string UserFirstName { get; init; } = default!;
        public string UserRole { get; init; } = default!;
        public string ResetToken { get; init; } = default!;
        public DateTimeOffset ExpiresAt { get; init; }

        public PasswordResetRequestedIntegrationEvent(
            Guid userId,
            string userEmail,
            string userFirstName,
            string userRole,
            string resetToken,
            DateTimeOffset expiresAt)
        {
            UserId = userId;
            UserEmail = userEmail;
            UserFirstName = userFirstName;
            UserRole = userRole;
            ResetToken = resetToken;
            ExpiresAt = expiresAt;
        }
    }
}