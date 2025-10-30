using IAM.Application.DTOs;
using IAM.Application.Handlers.Users.Commands.Register;
using Mapster;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Application.Common.Mappings;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, RegisterResponse>()
            .MapWith(src => new RegisterResponse(
                src.Id,
                src.Email.Value,
                src.Name.FirstName,
                src.Name.LastName,
                src.Role.ToString(),
                src.IsEmailConfirmed,
                src.CreatedAt
            ));

        config.NewConfig<User, RegisterDefaultParentResponse>()
            .MapWith(src => new RegisterDefaultParentResponse(
                src.Id,
                src.AvatarUrl,
                src.Email.Value,
                src.Name.FirstName,
                src.Name.LastName,
                src.PhoneNumber!
            ));
    }
}