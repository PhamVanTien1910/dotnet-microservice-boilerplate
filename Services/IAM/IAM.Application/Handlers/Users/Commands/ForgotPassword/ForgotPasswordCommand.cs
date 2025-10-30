using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(
        string Email
    ) : ICommand<ForgotPasswordResponse>;
}
