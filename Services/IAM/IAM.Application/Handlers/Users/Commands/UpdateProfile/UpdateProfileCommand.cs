using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.Handlers.Users.Queries.GetUserById;

namespace IAM.Application.Handlers.Users.Commands.UpdateProfile
{
    public record UpdateProfileCommand(
        Guid Id,
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        string? AvatarUrl
    ) : ICommand<UserResponse>;
}