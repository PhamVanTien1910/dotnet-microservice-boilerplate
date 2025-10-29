using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Api.Services.Interfaces
{
    public interface IJwksKeyResolver
    {
        Task<IEnumerable<SecurityKey>> ResolveSigningKeysAsync(
        string token,
        SecurityToken securityToken,
        string kid,
        TokenValidationParameters parameters);
    }
}