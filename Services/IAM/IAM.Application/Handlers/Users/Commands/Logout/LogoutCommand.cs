using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.Logout
{
    public record LogoutCommand(
        string? RefreshToken = null
    ) : ICommand<LogoutResponse>;
}
