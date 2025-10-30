using BuildingBlocks.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using IAM.Application.Common.Interfaces;

namespace IAM.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService, IJwtContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                                   ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
                                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
                                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
                                ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public string? Jti => _httpContextAccessor.HttpContext?.User?.FindFirst("jti")?.Value;

        public DateTime? TokenExpiry
        {
            get
            {
                var expClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("exp")?.Value;
                if (expClaim != null && long.TryParse(expClaim, out var expUnix))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expUnix).DateTime;
                }
                return null;
            }
        }
    }
}
