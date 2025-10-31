using BuildingBlocks.MediatR.Abstractions.Command;

namespace IAM.Application.Handlers.Users.Commands.DeleteProfile
{
    public record DeleteProfileCommand(Guid UserId) : ICommand<object>;
}