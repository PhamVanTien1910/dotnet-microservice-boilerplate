using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IAM.Application.Common.Interfaces;
using IAM.Domain.Aggregates.Users.Entities;

namespace IAM.Infrastructure.Services
{
    public class TokenClaimService : ITokenClaimService
    {
        public Task<IEnumerable<Claim>> GetClaimsAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, user.Email.Value),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name.FirstName),
                new(ClaimTypes.Surname, user.Name.LastName),
                
                // Add the system-level role
                new(ClaimTypes.Role, user.Role.ToString()),
            };

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }
    }
}
