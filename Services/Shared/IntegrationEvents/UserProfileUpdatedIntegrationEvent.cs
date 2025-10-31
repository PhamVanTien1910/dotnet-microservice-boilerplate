using BuildingBlocks.Application.Messaging.Events;

namespace Shared.IntegrationEvents
{
    public class UserProfileUpdatedIntegrationEvent : IntegrationEvent
    {
        public Guid UserId { get; }
        public string? FirstName { get; }
        public string? LastName { get; }
        public string Role { get; }
        public string? PhoneNumber { get; }
        public string? AvatarUrl { get; }
        public string? State { get; }
        public string? City { get; }
        public string? Street { get; }

        public UserProfileUpdatedIntegrationEvent(
            Guid userId,
            string? firstName,
            string? lastName,
            string role,
            string? phoneNumber = null,
            string? avatarUrl = null,
            string? state = null,
            string? city = null,
            string? street = null)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            PhoneNumber = phoneNumber;
            AvatarUrl = avatarUrl;
            State = state;
            City = city;
            Street = street;
        }
    }
}