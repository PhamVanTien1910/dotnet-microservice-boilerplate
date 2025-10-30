using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using IAM.Application.Common.Interfaces;
using IAM.Domain.Aggregates.Users.Entities;
using IAM.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IAM.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenClaimService _tokenClaimService;
        private readonly ILogger<JwtService> _logger;
        private readonly Lazy<RSAParameters> _rsaParameters;

        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger, ITokenClaimService tokenClaimService)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _tokenClaimService = tokenClaimService;

            _rsaParameters = new Lazy<RSAParameters>(() =>
            {
                using var rsa = RSA.Create();
                try
                {
                    if (string.IsNullOrWhiteSpace(_jwtSettings.PrivateKeyPem))
                    {
                        throw new InvalidOperationException("JWT private key is not loaded. Ensure Jwt:PrivateKeyPem or Jwt:PrivateKeyPath is configured.");
                    }
                    rsa.ImportFromPem(_jwtSettings.PrivateKeyPem);
                    return rsa.ExportParameters(true);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    _logger.LogError(ex, "Failed to load RSA private key. {Message}", ex.Message);
                    throw;
                }
            });
        }

        public async Task<(string accessToken, string refreshToken, long accessTokenExpiresAt)> GenerateTokensAsync(User user)
        {
            var expirationTime = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            var accessToken = await GenerateAccessTokenAsync(user, expirationTime);
            var refreshToken = GenerateRefreshToken();

            var accessTokenExpiresAt = ((DateTimeOffset)expirationTime).ToUnixTimeSeconds();

            return (accessToken, refreshToken, accessTokenExpiresAt);
        }

        public TimeSpan GetRefreshTokenLifetime(bool rememberMe)
        {
            return rememberMe
                ? TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDaysRememberMe)
                : TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays);
        }

        private async Task<string> GenerateAccessTokenAsync(User user, DateTime expirationTime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var rsa = RSA.Create();
            rsa.ImportParameters(_rsaParameters.Value);
            var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = _jwtSettings.KeyId };
            var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

            var claims = await _tokenClaimService.GetClaimsAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationTime,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = signingCredentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            return accessToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
