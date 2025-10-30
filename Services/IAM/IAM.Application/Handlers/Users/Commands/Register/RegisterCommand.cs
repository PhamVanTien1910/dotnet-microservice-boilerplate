using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.Register
{
    public record RegisterCommand (
        string Email,
        string Password,
        string ConfirmPassword,
        string FirstName,
        string LastName,
        string Role,
        string? PhoneNumber) : ICommand<RegisterResponse>;
}