using BuildingBlocks.Domain.Repositories;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Domain.Aggregates.Users.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
    }
}
