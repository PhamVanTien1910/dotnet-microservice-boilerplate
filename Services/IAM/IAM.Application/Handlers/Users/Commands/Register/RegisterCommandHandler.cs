using BuildingBlocks.Application.Messaging.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.MediatR.Abstractions.Command;
using BuildingBlocks.Domain.Repositories;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.Enums;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using IAM.Domain.Aggregates.Users.ValueObjects;
using Shared.IntegrationEvents;
using MapsterMapper;

namespace IAM.Application.Handlers.Users.Commands.Register
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenHasher _tokenHasher;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenHasher tokenHasher,
            IMapper mapper,
            IEventBus eventBus)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenHasher = tokenHasher;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Validate user uniqueness
            var existingUser =
                await _userRepository.GetBySpecAsync(new UserByEmailSpecification(request.Email), cancellationToken);
            if (existingUser != null)
                throw new ConflictException("A user with this email already exists.");

            // Create user identity
            var user = await CreateUserAsync(request, cancellationToken);

            // Persist changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new UserRegisteredIntegrationEvent(
                user.Id,
                user.Email.Value,
                user.Name.FirstName,
                user.Name.LastName,
                user.EmailConfirmationToken?.PlainValue ?? string.Empty
            ), cancellationToken);

            return _mapper.Map<RegisterResponse>(user);
        }

        private async Task<User> CreateUserAsync(RegisterCommand request, CancellationToken cancellationToken)
        {
            var hashedPasswordString = _passwordHasher.HashPassword(request.Password);
            var hashedPassword = PasswordHash.Create(hashedPasswordString);

            var name = PersonName.Create(request.FirstName, request.LastName);
            var email = Email.Create(request.Email);
            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : PhoneNumber.Create(request.PhoneNumber);
            var userRole = Enum.Parse<UserRole>(request.Role, true);

            var user = User.Create(
                name,
                email,
                phoneNumber,
                hashedPassword,
                userRole);

            // Generate and set email confirmation token
            var emailConfirmationToken = SecurityToken.Generate(
                TokenType.EmailConfirmation,
                _tokenHasher.HashToken);
            user.SetEmailConfirmationToken(emailConfirmationToken);

            await _userRepository.AddAsync(user, cancellationToken);
            return user;
        }
    }
}