namespace IAM.Application.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public long AccessTokenExpiresAt { get; set; }
}