using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.EmailConfirm
{
    public record EmailConfirmCommand(
        string Token
    ) : ICommand<EmailConfirmResponse>;
}
