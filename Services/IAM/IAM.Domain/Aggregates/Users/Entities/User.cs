using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Model;
using IAM.Domain.Aggregates.Users.Enums;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Domain.Aggregates.Users.Entities
{
    public class User : AggregateRoot, ICreatedAuditable, IModifiedAuditable, ISoftDeletable
    {
        public PersonName Name { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public PhoneNumber? PhoneNumber { get; private set; }
        public string? AvatarUrl { get; private set; }
        public PasswordHash PasswordHash { get; private set; } = null!;
        public SecurityToken? EmailConfirmationToken { get; private set; }
        public SecurityToken? PasswordResetToken { get; private set; }
        public bool IsEmailConfirmed { get; private set; }
        private readonly List<UserSession> _sessions = [];
        public IReadOnlyCollection<UserSession> Sessions => _sessions.AsReadOnly();
        public DateTime? LastLoginAt { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ModifiedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private User()
        {
        }
    }
}