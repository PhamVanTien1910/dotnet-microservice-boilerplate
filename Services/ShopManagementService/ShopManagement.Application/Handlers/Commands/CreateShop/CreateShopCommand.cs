using BuildingBlocks.MediatR.Abstractions.Command;
using Microsoft.AspNetCore.Http;

namespace ShopManagement.Application.Handlers.Commands.CreateShop
{
    public record CreateShopCommand(
        string Name,
        string Description,
        IFormFile LogoUrl
    ) : ICommand<Guid>;
}