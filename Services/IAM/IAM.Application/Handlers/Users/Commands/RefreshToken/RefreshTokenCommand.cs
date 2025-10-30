using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.RefreshToken
{
    public record RefreshTokenCommand(
        string RefreshToken
    ) : ICommand<RefreshTokenResponse>;
}