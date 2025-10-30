using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.MediatR.Abstractions.Query;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using Mapster;

namespace IAM.Application.Handlers.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var specification = new UserByIdSpecification(request.UserId);
            var user = await _userRepository.GetBySpecAsync(specification, cancellationToken);

            if (user is null)
                throw new NotFoundException("User not found");

            var response = user.Adapt<UserResponse>();

            return response;
        }
    }
}