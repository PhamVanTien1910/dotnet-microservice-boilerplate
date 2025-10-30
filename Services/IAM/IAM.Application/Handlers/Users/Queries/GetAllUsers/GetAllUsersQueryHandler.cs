using BuildingBlocks.MediatR.Abstractions.Query;
using IAM.Application.Handlers.Users.Queries.GetUserById;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using Mapster;

namespace IAM.Application.Handlers.Users.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IEnumerable<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponse>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new GetAllUsersSpecification();
        var users = await _userRepository.ListAsync(specification, cancellationToken);

        var response = users.Select(user => user.Adapt<UserResponse>());

        return response;
    }
}
