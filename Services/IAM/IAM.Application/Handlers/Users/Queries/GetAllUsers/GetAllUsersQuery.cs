using BuildingBlocks.MediatR.Abstractions.Query;
using IAM.Application.Handlers.Users.Queries.GetUserById;

namespace IAM.Application.Handlers.Users.Queries.GetAllUsers;

public record GetAllUsersQuery() : IQuery<IEnumerable<UserResponse>>;
