using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.MediatR.Abstractions.Command;
using BuildingBlocks.Domain.Repositories;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Application.Handlers.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user with matching active session
        var user = await FindUserWithActiveSessionAsync(request.RefreshToken, cancellationToken);

        // Validate user account status
        if (user.IsDeleted)
            throw new ForbiddenException("This account has been deactivated.");

        // Generate new tokens and refresh the session
        var (accessToken, newRefreshToken, accessTokenExpiresAt) = await _jwtService.GenerateTokensAsync(user);
        var sessionLifetime = _jwtService.GetRefreshTokenLifetime(false);

        // Find and refresh the session
        var refreshTokenHash = TokenHash.FromPlainToken(request.RefreshToken);
        var session = user.Sessions.FirstOrDefault(s => s.RefreshTokenHash.Value == refreshTokenHash.Value && s.IsActive());

        if (session == null)
            throw new UnauthorizedException("The provided token is invalid or expired.");

        user.RefreshSession(session.Id, newRefreshToken, sessionLifetime);
        user.RecordLogin();

        _userRepository.Update(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt
        };
    }

    private async Task<User> FindUserWithActiveSessionAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetBySpecAsync(new UserByRefreshTokenSpecification(refreshToken), cancellationToken);

        if (user == null)
            throw new UnauthorizedException("The provided token is invalid or expired.");

        return user;
    }
}