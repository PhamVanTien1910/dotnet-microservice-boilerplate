using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Domain.Aggregates.Users.Specifications;

public class UserByIdSpecification : Specification<User>
{
    private readonly Guid _userId;

    public UserByIdSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Id == _userId;
    }
}
