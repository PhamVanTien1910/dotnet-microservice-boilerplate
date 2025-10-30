using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.ChangePassword
{
    public record ChangePasswordCommand(
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword
    ) : ICommand<ChangePasswordResponse>;
}
