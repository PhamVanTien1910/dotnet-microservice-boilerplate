namespace IAM.Application.DTOs;

public record RegisterDefaultParentResponse(
    Guid UserId,
    string? AvatarUrl,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber);