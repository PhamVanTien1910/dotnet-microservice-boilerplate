namespace IAM.Application.DTOs;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public long AccessTokenExpiresAt { get; set; }

    public RefreshTokenResponse WithoutRefreshToken()
    {
        return new RefreshTokenResponse
        {
            AccessToken = AccessToken,
            RefreshToken = null,
            AccessTokenExpiresAt = AccessTokenExpiresAt
        };
    }
}