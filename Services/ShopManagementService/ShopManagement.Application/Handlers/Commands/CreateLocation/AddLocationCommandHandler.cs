using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using Microsoft.IdentityModel.Tokens;
using ShopManagement.Application.Interfaces;
using ShopManagement.Domain.Aggregates.Repositories;
using ShopManagement.Domain.Aggregates.Specifications;

namespace ShopManagement.Application.Handlers.Commands.AddLocation
{
    public class AddLocationCommandHandler : ICommandHandler<AddLocationCommand, object>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGpsService _gpsService;

        public AddLocationCommandHandler(IShopRepository shopRepository, IUnitOfWork unitOfWork, IGpsService gpsService)
        {
            _shopRepository = shopRepository;
            _unitOfWork = unitOfWork;
            _gpsService = gpsService;
        }

        public async Task<object> Handle(AddLocationCommand request, CancellationToken cancellationToken)
        {
            var spec = new ShopByIdSpecification(request.ShopId);
            var shop = await _shopRepository.GetBySpecAsync(spec, cancellationToken);
            if (shop == null)
            {
                throw new SecurityTokenException($"Shop with id {request.ShopId} not found.");
            }

            var gpsResult = await _gpsService.GetGpsCoordinatesAsync(request.Street, request.City, request.State);
            if (gpsResult == null)
            {
                throw new SecurityTokenException($"Location not found.");
            }

            shop.AddLocation(request.name, request.phoneNumber, request.Street, request.City, request.State, gpsResult.Latitude, gpsResult.Longitude);
            
            _shopRepository.Update(shop);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new { shop.Id, Location = gpsResult };
        }
    }
}