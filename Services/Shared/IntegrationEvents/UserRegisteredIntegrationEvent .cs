using BuildingBlocks.Application.Messaging.Events;

namespace Shared.IntegrationEvents
{
    public class UserRegisteredIntegrationEvent : IntegrationEvent
    {
        public Guid UserId { get; init; }
        public string UserEmail { get; init; } = default!;
        public string UserFirstName { get; init; } = default!;
        public string UserLastName { get; init; } = default!;
        public string ConfirmationToken { get; init; } = default!;

        public UserRegisteredIntegrationEvent(
            Guid userId,
            string userEmail,
            string userFirstName,
            string userLastName,
            string confirmationToken)
        {
            UserId = userId;
            UserEmail = userEmail;
            UserFirstName = userFirstName;
            UserLastName = userLastName;
            ConfirmationToken = confirmationToken;
        }
    }
}