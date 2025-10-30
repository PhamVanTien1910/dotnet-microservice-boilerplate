using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Application.Common.Interfaces
{
    public interface IJwtService
    {
        Task<(string accessToken, string refreshToken, long accessTokenExpiresAt)> GenerateTokensAsync(User user);
        TimeSpan GetRefreshTokenLifetime(bool rememberMe);
    }
}