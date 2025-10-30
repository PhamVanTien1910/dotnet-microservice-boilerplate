using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.BlacklistToken
{
    public record BlacklistTokenCommand(string Jti, DateTime? TokenExpiry = null) : ICommand<BlacklistTokenResponse>;
}
