using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Application.Handlers.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user with valid password reset token
        var user = await _userRepository.GetBySpecAsync(new UserByPasswordResetTokenSpecification(request.Token), cancellationToken);
        if (user == null || user.PasswordResetToken == null)
            throw new UnauthorizedException("The provided token is invalid or expired.");

        // Validate the token
        if (!user.PasswordResetToken.IsValid(request.Token, _tokenHasher.HashToken))
            throw new UnauthorizedException("The provided token is invalid or expired.");

        // Generate secure hash for the provided password
        var hashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        var newPasswordHash = PasswordHash.Create(hashedPassword);

        // Complete password reset
        user.CompletePasswordReset(newPasswordHash);

        // Persist the updated user state
        _userRepository.Update(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ResetPasswordResponse
        {
            Message = "Your password has been reset successfully."
        };

        return response;
    }
}
