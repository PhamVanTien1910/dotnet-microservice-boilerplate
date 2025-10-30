using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.MediatR.Abstractions.Command;
using BuildingBlocks.Domain.Repositories;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;

namespace IAM.Application.Handlers.Users.Commands.EmailConfirm;

public class EmailConfirmCommandHandler : ICommandHandler<EmailConfirmCommand, EmailConfirmResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;

    public EmailConfirmCommandHandler(
        IUserRepository userRepository,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailConfirmResponse> Handle(EmailConfirmCommand request, CancellationToken cancellationToken)
    {
        // Find user with valid email confirmation token
        var user = await _userRepository.GetBySpecAsync(new UserByEmailConfirmationTokenSpecification(request.Token),
            cancellationToken);

        if (user == null || user.EmailConfirmationToken == null)
            throw new UnauthorizedException("The provided token is invalid or expired.");

        // Validate the token
        if (!user.EmailConfirmationToken.IsValid(request.Token, _tokenHasher.HashToken))
            throw new UnauthorizedException("The provided token is invalid or expired.");

        // Process email confirmation in domain model
        user.ConfirmEmail();

        // Persist confirmation state
        _userRepository.Update(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSuccessResponse();
    }

    private static EmailConfirmResponse CreateSuccessResponse()
    {
        var response = new EmailConfirmResponse
        {
            Message = "Email verification completed successfully. Your account has been activated."
        };
        return response;
    }
}