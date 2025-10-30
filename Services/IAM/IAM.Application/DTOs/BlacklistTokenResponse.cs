namespace IAM.Application.DTOs;

public record BlacklistTokenResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Jti { get; init; }
}