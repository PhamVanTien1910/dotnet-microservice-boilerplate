using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.MediatR.Abstractions.Command;
using IAM.Application.Common.Interfaces;
using IAM.Application.DTOs;

namespace IAM.Application.Handlers.Users.Commands.BlacklistToken
{
    public class BlacklistTokenCommandHandler : ICommandHandler<BlacklistTokenCommand, BlacklistTokenResponse>
    {
        private readonly IJwtBlacklistService _jwtBlacklistService;

        public BlacklistTokenCommandHandler(
            IJwtBlacklistService jwtBlacklistService)
        {
            _jwtBlacklistService = jwtBlacklistService;
        }

        public async Task<BlacklistTokenResponse> Handle(BlacklistTokenCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Jti))
                throw new BadRequestException("Invalid JTI - cannot blacklist token");
            
            var tokenExpiry = request.TokenExpiry ?? DateTime.UtcNow.AddHours(24);
            
            if (tokenExpiry <= DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired token cannot be blacklisted");
                
            var remainingTime = tokenExpiry - DateTime.UtcNow;
            await _jwtBlacklistService.BlacklistTokenAsync(request.Jti, remainingTime);

            return new BlacklistTokenResponse
            {
                Success = true,
                Message = "Token successfully blacklisted",
                Jti = request.Jti,
            };
        }
    }
}