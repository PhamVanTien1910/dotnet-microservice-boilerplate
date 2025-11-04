using BuildingBlocks.EfCore;
using ShopManagement.Domain.Aggregates.Entities;
using ShopManagement.Domain.Aggregates.Repositories;
using ShopManagement.Infrastructure.Data;

namespace ShopManagement.Infrastructure.Repositories
{
    public class ShopRepository : Repository<Shop>, IShopRepository
    {
        public ShopRepository(ShopDbContext context) : base(context)
        {
        }
    }
}