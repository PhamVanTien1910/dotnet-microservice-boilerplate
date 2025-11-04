using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using ShopManagement.Domain.Aggregates.Entities;

namespace ShopManagement.Domain.Aggregates.Specifications
{
    public class ShopByNameSpecification : Specification<Shop>
    {
        private readonly string _name;
        public ShopByNameSpecification(string name)
        {
            _name = name;
        }
        
        public override Expression<Func<Shop, bool>> ToExpression()
        {
            return shop => shop.Name == _name;
        }
    }
}