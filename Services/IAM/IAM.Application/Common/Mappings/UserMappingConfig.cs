using IAM.Application.Handlers.Users.Queries.GetUserById;
using IAM.Domain.Aggregates.Users.Entities;
using Mapster;

namespace IAM.Application.Common.Mappings
{
    public class UserMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserResponse>()
                .MapWith(src => new UserResponse
                {
                    Id = src.Id,
                    Email = src.Email.Value,
                    FirstName = src.Name.FirstName,
                    LastName = src.Name.LastName,
                    Role = src.Role.ToString(),
                    PhoneNumber = src.PhoneNumber!,
                    AvatarUrl = src.AvatarUrl,
                    LastLoginAt = src.LastLoginAt,
                    CreatedAt = src.CreatedAt,
                    IsDeleted = src.IsDeleted
                });
        }
    }
}