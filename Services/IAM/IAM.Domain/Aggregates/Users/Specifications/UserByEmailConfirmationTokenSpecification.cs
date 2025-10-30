using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Domain.Aggregates.Users.Specifications
{
    public class UserByEmailConfirmationTokenSpecification : Specification<User>
    {
        private readonly string _tokenHash;

        public UserByEmailConfirmationTokenSpecification(string plainToken)
        {
            if (string.IsNullOrWhiteSpace(plainToken))
                throw new ArgumentNullException(nameof(plainToken));

            var tokenHash = TokenHash.FromPlainToken(plainToken);
            _tokenHash = tokenHash.Value;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            var now = DateTime.UtcNow;
            return user => !user.IsDeleted &&
                          user.EmailConfirmationToken != null &&
                          user.EmailConfirmationToken.HashedValue == _tokenHash &&
                          user.EmailConfirmationToken.ExpiresAt > now;
        }
    }
}
