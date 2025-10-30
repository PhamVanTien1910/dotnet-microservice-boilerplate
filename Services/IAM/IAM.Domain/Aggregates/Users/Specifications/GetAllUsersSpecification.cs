using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Domain.Aggregates.Users.Specifications;

public class GetAllUsersSpecification : Specification<User>
{
    public GetAllUsersSpecification()
    {
        AsNoTracking();
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => !user.IsDeleted;
    }
}
