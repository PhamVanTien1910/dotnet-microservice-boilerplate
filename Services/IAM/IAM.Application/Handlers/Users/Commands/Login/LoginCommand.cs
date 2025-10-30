using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password,
        bool RememberMe = false
    ) : ICommand<LoginResponse>;
}