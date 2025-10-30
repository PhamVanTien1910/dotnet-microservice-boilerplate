namespace IAM.Application.DTOs;

public record RegisterResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsEmailConfirmed,
    DateTime CreatedAt);