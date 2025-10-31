using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;
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

        // Factory method
        public static User Create(
            PersonName name,
            Email email,
            PhoneNumber? phoneNumber,
            PasswordHash passwordHash,
            UserRole role)
        {
            var user = new User
            {
                Name = name,
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = passwordHash,
                Role = role,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
            };

            return user;
        }

        // Credential operations
        public void ChangePassword(PasswordHash newPasswordHash)
        {
            if (newPasswordHash == null)
                throw new BadRequestException("Password hash cannot be null");

            PasswordHash = newPasswordHash;
            PasswordResetToken = null;
        }

        public void SetEmailConfirmationToken(SecurityToken token)
        {
            EmailConfirmationToken = token;
        }

        public void ConfirmEmail()
        {
            if (!IsEmailConfirmed)
            {
                IsEmailConfirmed = true;
                EmailConfirmationToken = null;
            }
        }

        public void SetPasswordResetToken(SecurityToken token)
        {
            PasswordResetToken = token;
        }

        public void CompletePasswordReset(PasswordHash newPasswordHash)
        {
            ChangePassword(newPasswordHash);
            PasswordResetToken = null;
        }

        // Session operations
        public UserSession CreateSession(string plainRefreshToken, TimeSpan lifetime)
        {
            if (IsDeleted)
                throw new ForbiddenException("Account is inactive");

            var refreshTokenHash = TokenHash.FromPlainToken(plainRefreshToken);
            var session = UserSession.Create(Id, refreshTokenHash, lifetime);
            _sessions.Add(session);
            return session;
        }

        public void RefreshSession(Guid sessionId, string newPlainRefreshToken, TimeSpan lifetime)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId && s.IsActive());
            if (session == null)
                throw new NotFoundException("Cannot refresh an inactive or non-existent session");

            session.Refresh(newPlainRefreshToken, lifetime);
        }

        public void RevokeSession(Guid sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
                session.Revoke();
        }

        // Login tracking
        public void RecordLogin()
        {
            if (IsDeleted)
                throw new ForbiddenException("Account is inactive");

            LastLoginAt = DateTime.UtcNow;
        }

        // Profile management
        public void UpdateProfile(PersonName? name, PhoneNumber? phoneNumber, string? avatarUrl)
        {
            if (IsDeleted)
                throw new ForbiddenException("Account is inactive");

            if (name != null && !Name.Equals(name))
                Name = name;

            if (!Equals(PhoneNumber, phoneNumber))
                PhoneNumber = phoneNumber;

            if (AvatarUrl != avatarUrl)
                AvatarUrl = avatarUrl;
        }

        // Account management
        public void Inactivate()
        {
            if (IsDeleted)
                throw new BadRequestException("Account is already inactive");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (!IsDeleted)
                throw new BadRequestException("Account is already active");

            IsDeleted = false;
            DeletedAt = null;
        }
    }
}