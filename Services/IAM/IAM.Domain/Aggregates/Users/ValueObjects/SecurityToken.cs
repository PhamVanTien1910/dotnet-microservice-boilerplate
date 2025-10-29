using BuildingBlocks.Domain.Exceptions;
using IAM.Domain.Aggregates.Users.Enums;

namespace IAM.Domain.Aggregates.Users.ValueObjects;

public class SecurityToken
{
    public string PlainValue { get; private set; } = string.Empty;
    public string HashedValue { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public TokenType Type { get; private set; }

    // For EF Core materialization
    private SecurityToken() { }

    private SecurityToken(string plainValue, string hashedValue, DateTime expiresAt, TokenType type)
    {
        PlainValue = plainValue;
        HashedValue = hashedValue;
        ExpiresAt = expiresAt;
        Type = type;
    }

    public static SecurityToken Generate(TokenType type, Func<string, string> hashFunction)
    {
        var plainToken = GenerateSecureToken();
        var hashedToken = hashFunction(plainToken);
        var expiresAt = GetExpirationTime(type);

        return new SecurityToken(plainToken, hashedToken, expiresAt, type);
    }

    public bool IsValid(string providedToken, Func<string, string> hashFunction)
    {
        if (IsExpired) return false;

        var providedTokenHash = hashFunction(providedToken);
        return HashedValue == providedTokenHash;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "");
    }

    private static DateTime GetExpirationTime(TokenType type) => type switch
    {
        TokenType.EmailConfirmation => DateTime.UtcNow.AddHours(24),
        TokenType.PasswordReset => DateTime.UtcNow.AddHours(1),
        TokenType.RefreshToken => DateTime.UtcNow.AddDays(30),
        _ => throw new BadRequestException($"Unknown token type: {type}")
    };
}