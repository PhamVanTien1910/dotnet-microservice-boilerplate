using BuildingBlocks.MediatR.Abstractions.Command;

namespace ShopManagement.Application.Handlers.Commands.AddLocation
{
    public record AddLocationCommand(
        Guid ShopId,
        string name,
        string phoneNumber,
        string Street,
        string City,
        string State
    ) : ICommand<object>;
}