using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;

namespace IAM.Application.Handlers.Users.Commands.DeleteProfile
{
    public class DeleteProfileCommandHandler : ICommandHandler<DeleteProfileCommand, object>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<object> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
        {
            var spec = new UserByIdSpecification(request.UserId);
            var user = await _userRepository.GetBySpecAsync(spec, cancellationToken);
            if (user == null) throw new NotFoundException("User not found");
            
            user.Inactivate();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new
            {
                message = "User profile deleted successfully."
            };
        }
    }
}