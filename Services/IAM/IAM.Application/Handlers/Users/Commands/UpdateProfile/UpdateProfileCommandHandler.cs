using BuildingBlocks.Application.Services;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Repositories;
using IAM.Application.Handlers.Users.Queries.GetUserById;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using IAM.Domain.Aggregates.Users.ValueObjects;
using Mapster;
using MediatR;

namespace IAM.Application.Handlers.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<UserResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedException("Invalid email or password.");

            var spec = new UserByIdSpecification(userId);
            var user = await _userRepository.GetBySpecAsync(spec, cancellationToken);
            if (user == null) throw new NotFoundException("User not found");

            var name = PersonName.Create(request.FirstName!, request.LastName!);
            var phoneNumber = PhoneNumber.Create(request.PhoneNumber!);

            user.UpdateProfile(name, phoneNumber, request.AvatarUrl);
            _userRepository.Update(user);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            var response = user.Adapt<UserResponse>();
            return response;
        }
    }
}