using BuildingBlocks.MediatR.Abstractions.Command;
using BuildingBlocks.Domain.Repositories;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;
using IAM.Domain.Aggregates.Users.ValueObjects;

namespace IAM.Application.Handlers.Users.Commands.Logout;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, LogoutResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtContext _jwtContext;
    private readonly IJwtBlacklistService _jwtBlacklistService;

    public LogoutCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtContext jwtContext,
        IJwtBlacklistService jwtBlacklistService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtContext = jwtContext;
        _jwtBlacklistService = jwtBlacklistService;
    }

    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Blacklist current access token and revoke refresh token
        await BlacklistCurrentAccessTokenAsync();

        // Invalidate refresh token if provided
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            await RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        var response = new LogoutResponse { Message = "Logout successful." };
        return response;
    }

    private async Task BlacklistCurrentAccessTokenAsync()
    {
        var jti = _jwtContext.Jti;
        var tokenExpiry = _jwtContext.TokenExpiry;

        if (string.IsNullOrWhiteSpace(jti) || tokenExpiry == null || tokenExpiry <= DateTime.UtcNow)
        {
            return;
        }

        var remainingTime = tokenExpiry.Value - DateTime.UtcNow;
        await _jwtBlacklistService.BlacklistTokenAsync(jti, remainingTime);
    }

    private async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetBySpecAsync(new UserByRefreshTokenSpecification(refreshToken), cancellationToken);

        if (user != null)
        {
            var refreshTokenHash = TokenHash.FromPlainToken(refreshToken);
            var session = user.Sessions.FirstOrDefault(s => s.RefreshTokenHash.Value == refreshTokenHash.Value && s.IsActive());

            if (session != null)
            {
                user.RevokeSession(session.Id);
                _userRepository.Update(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
