using BuildingBlocks.MediatR.Abstractions.Query;
using BuildingBlocks.Application.Services;
using BuildingBlocks.Domain.Exceptions;
using IAM.Application.Handlers.Users.Queries.GetUserById;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using Mapster;

namespace IAM.Application.Handlers.Users.Queries.GetMyProfile;

internal class GetMyProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService) : IQueryHandler<GetMyProfileQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
           throw new UnauthorizedException("Invalid email or password.");

        var specification = new UserByIdSpecification(userId);
        var user = await userRepository.GetBySpecAsync(specification, cancellationToken);

        if (user is null)
            throw new NotFoundException("User not found");

        var response = user.Adapt<UserResponse>();
        return response;
    }
}
