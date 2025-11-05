using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using ShopManagement.Domain.Aggregates.Entities;

namespace ShopManagement.Domain.Aggregates.Specifications
{
    public class ShopByIdSpecification : Specification<Shop>
    {
        private readonly Guid _shopId;

        public ShopByIdSpecification(Guid shopId)
        {
            _shopId = shopId;
        }

        public override Expression<Func<Shop, bool>> ToExpression()
        {
            return shop => shop.Id == _shopId;
        }
    }
}