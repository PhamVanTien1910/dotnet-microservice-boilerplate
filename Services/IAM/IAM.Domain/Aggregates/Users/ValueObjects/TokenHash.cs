using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Domain.Exceptions;

namespace IAM.Domain.Aggregates.Users.ValueObjects;

public record TokenHash
{
    public string Value { get; init; }

    private TokenHash(string value)
    {
        Value = value;
    }

    public static TokenHash FromPlainToken(string plainToken)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
            throw new BadRequestException("Plain token cannot be empty");

        var hashedValue = HashToken(plainToken);
        return new TokenHash(hashedValue);
    }

    public static TokenHash Create(string hashedValue)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
            throw new BadRequestException("Token hash cannot be empty");

        return new TokenHash(hashedValue);
    }

    public bool VerifyToken(string plainToken)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
            return false;

        var hashedToken = HashToken(plainToken);
        return Value == hashedToken;
    }

    private static string HashToken(string plainToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plainToken);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    public static implicit operator string(TokenHash tokenHash) => tokenHash.Value;
}