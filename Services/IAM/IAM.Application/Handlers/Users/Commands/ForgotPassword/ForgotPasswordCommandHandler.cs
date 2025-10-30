using BuildingBlocks.MediatR.Abstractions.Command;
using BuildingBlocks.Domain.Repositories;
using Shared.IntegrationEvents;
using BuildingBlocks.Application.Messaging.Abstractions;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Enums;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Application.Handlers.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Retrieve user account associated with provided email address
        var userSpec = new UserByEmailSpecification(request.Email);
        var user = await _userRepository.GetBySpecAsync(userSpec, cancellationToken);
        if (user == null)
            return CreateSuccessResponse();

        if (user.IsDeleted)
            return CreateSuccessResponse();

        // Generate password reset token
        var passwordResetToken = SecurityToken.Generate(
            TokenType.PasswordReset,
            _tokenHasher.HashToken);

        user.SetPasswordResetToken(passwordResetToken);

        _userRepository.Update(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch password reset notification event
        await _eventBus.PublishAsync(new PasswordResetRequestedIntegrationEvent(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Role.ToString(),
            passwordResetToken.PlainValue,
            passwordResetToken.ExpiresAt), cancellationToken);

        return CreateSuccessResponse();
    }

    private static ForgotPasswordResponse CreateSuccessResponse()
    {
        var response = new ForgotPasswordResponse
        {
            Message = "If an account with that email exists, a password reset link has been sent."
        };
        return response;
    }
}