using BuildingBlocks.MediatR.Abstractions.Query;

namespace IAM.Application.Handlers.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(
        Guid UserId
    ) : IQuery<UserResponse>;
}