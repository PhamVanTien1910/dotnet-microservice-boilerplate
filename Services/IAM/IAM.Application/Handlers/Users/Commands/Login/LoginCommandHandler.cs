using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;
using IAM.Domain.Aggregates.Users.Repositories;
using IAM.Domain.Aggregates.Users.Specifications;

namespace IAM.Application.Handlers.Users.Commands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetBySpecAsync(new UserByEmailSpecification(request.Email), ct);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Value))
            throw new UnauthorizedException("Invalid email or password.");

        if (user.IsDeleted)
            throw new ForbiddenException("This account has been deactivated.");

        // Build claims and generate tokens
        var (accessToken, refreshToken, accessTokenExpiresAt) = await _jwtService.GenerateTokensAsync(user);
        var sessionLifetime = _jwtService.GetRefreshTokenLifetime(request.RememberMe);
        user.CreateSession(refreshToken, sessionLifetime);
        user.RecordLogin();

        _userRepository.Update(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt
        };
    }
}