using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Models;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Domain.Aggregates.Users.Entities;

public class UserSession : Entity, ICreatedAuditable
{
    public Guid UserId { get; private set; }
    public TokenHash RefreshTokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private UserSession() { } // For EF Core

    private UserSession(Guid userId, TokenHash refreshTokenHash, DateTime expiresAt)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public static UserSession Create(Guid userId, TokenHash refreshTokenHash, TimeSpan lifetime)
    {
        return new UserSession(userId, refreshTokenHash, DateTime.UtcNow.Add(lifetime));
    }

    public void Refresh(string newPlainRefreshToken, TimeSpan lifetime)
    {
        if (!IsActive())
            throw new InvalidOperationException("Cannot refresh an inactive session");

        RefreshTokenHash = TokenHash.FromPlainToken(newPlainRefreshToken);
        ExpiresAt = DateTime.UtcNow.Add(lifetime);
    }

    public bool IsActive()
    {
        return RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (RevokedAt != null)
            return;

        RevokedAt = DateTime.UtcNow;
    }
}