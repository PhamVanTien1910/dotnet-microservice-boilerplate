using BuildingBlocks.MediatR.Abstractions.Command;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Application.Services;
using BuildingBlocks.Domain.Exceptions;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Application.Handlers.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Retrieve and validate authenticated user identifier
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString))
            throw new UnauthorizedException("User is not authenticated.");

        var userId = Guid.TryParse(userIdString, out var userIdResult) ? userIdResult : Guid.Empty;

        // Retrieve user from repository
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new UnauthorizedException("Invalid email or password.");
        
        if (user.IsDeleted)
            throw new ForbiddenException("This account has been deactivated.");

        // Authenticate current password
        VerifyCurrentPassword(user, request.CurrentPassword);

        // Validate password policy compliance (password reuse)
        CheckPasswordReuse(user, request.NewPassword);

        // Update user with new password hash
        var hashedNewPassword = _passwordHasher.HashPassword(request.NewPassword);
        var newPasswordHash = PasswordHash.Create(hashedNewPassword);
        user.ChangePassword(newPasswordHash);

        // Persist changes to repository
        _userRepository.Update(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSuccessResponse();
    }

    private void VerifyCurrentPassword(User user, string currentPassword)
    {
        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash.Value))
            throw new UnauthorizedException("Current password is incorrect.");
    }

    private void CheckPasswordReuse(User user, string newPassword)
    {
        if (_passwordHasher.VerifyPassword(newPassword, user.PasswordHash.Value))
            throw new BadRequestException("New password must be different from current password");
    }

    private static ChangePasswordResponse CreateSuccessResponse()
    {
        var response = new ChangePasswordResponse
        {
            Message = "Password changed successfully."
        };
        return response;
    }
}