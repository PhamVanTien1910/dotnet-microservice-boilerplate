using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Token,
        string NewPassword,
        string ConfirmPassword
    ) : ICommand<ResetPasswordResponse>;
}
