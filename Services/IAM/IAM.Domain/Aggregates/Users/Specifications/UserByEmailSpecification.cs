using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Domain.Aggregates.Users.Specifications
{
    public class UserByEmailSpecification : Specification<User>
    {
        private readonly string _email;

        public UserByEmailSpecification(string email)
        {
            _email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => user.Email.Value.ToLower() == _email;
        }
    }
}
