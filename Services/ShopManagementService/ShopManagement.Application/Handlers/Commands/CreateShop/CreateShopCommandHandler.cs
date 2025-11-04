using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using Microsoft.AspNetCore.Http;
using ShopManagement.Application.Interfaces;
using ShopManagement.Domain.Aggregates.Entities;
using ShopManagement.Domain.Aggregates.Repositories;
using ShopManagement.Domain.Aggregates.Specifications;

namespace ShopManagement.Application.Handlers.Commands.CreateShop
{
    public class CreateShopCommandHandler : ICommandHandler<CreateShopCommand, Guid>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IUploadService _uploadService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateShopCommandHandler(IShopRepository shopRepository, IUploadService uploadService, IUnitOfWork unitOfWork)
        {
            _shopRepository = shopRepository;
            _uploadService = uploadService;
            _unitOfWork = unitOfWork;
        }
        public async Task<Guid> Handle(CreateShopCommand request, CancellationToken cancellationToken)
        {
            var spec = new ShopByNameSpecification(request.Name);
            var shopExists = await _shopRepository.ExistsAsync(spec, cancellationToken);
            if (shopExists)
            {
                throw new BadRequestException($"Shop with name {request.Name} already exists.");
            }

            var logoUrl = await UploadShopImageAsync(request.LogoUrl);
            var newShop = Shop.Create(request.Name, request.Description, logoUrl);

            await _shopRepository.AddAsync(newShop, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return newShop.Id;
        }

        private async Task<string> UploadShopImageAsync(IFormFile? image)
        {
            if (image == null)
                return string.Empty;

            return await _uploadService.UploadImageAsync(image, $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}",
                image.ContentType, image.Length, "shop-images");
        }
    }
}