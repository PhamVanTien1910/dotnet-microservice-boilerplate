using BuildingBlocks.MediatR.Abstractions.Query;
using IAM.Application.Handlers.Users.Queries.GetUserById;

namespace IAM.Application.Handlers.Users.Queries.GetMyProfile;

public record GetMyProfileQuery() : IQuery<UserResponse>;
