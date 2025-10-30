using BuildingBlocks.EfCore;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Infrastructure.Data;

namespace IAM.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IAMDbContext context) : base(context)
        {
        }
    }
}
