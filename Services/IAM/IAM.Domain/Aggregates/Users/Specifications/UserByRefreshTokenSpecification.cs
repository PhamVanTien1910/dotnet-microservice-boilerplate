using System.Linq.Expressions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Specifications;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Domain.Aggregates.Users.Specifications
{
    public class UserByRefreshTokenSpecification : Specification<User>
    {
        private readonly string _refreshTokenHash;

        public UserByRefreshTokenSpecification(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new NotFoundException(nameof(refreshToken));

            var tokenHash = TokenHash.FromPlainToken(refreshToken);
            _refreshTokenHash = tokenHash.Value;

            AddIncludeExpressions(new Expression<Func<User, object>>[]
            {
                u => u.Sessions
            });
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            var now = DateTime.UtcNow;
            return user => !user.IsDeleted &&
                          user.Sessions.Any(s => s.RefreshTokenHash.Value == _refreshTokenHash && s.RevokedAt == null && s.ExpiresAt > now);
        }
    }
}
