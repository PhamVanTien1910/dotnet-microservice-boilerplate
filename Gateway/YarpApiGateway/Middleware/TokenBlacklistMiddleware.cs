using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using StackExchange.Redis;

namespace YarpApiGateway.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDatabase _redisDb;
    private readonly ILogger<TokenBlacklistMiddleware> _logger;
    
    private const string BlacklistKeyPrefix = "jwt_blacklist:";
    
    // Endpoints that should skip blacklist validation
    private static readonly HashSet<string> SkipBlacklistEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/iam/v1/auth/refresh-token",
        "/api/iam/v1.0/auth/refresh-token"
    };

    public TokenBlacklistMiddleware(RequestDelegate next, IDatabase redisDb, ILogger<TokenBlacklistMiddleware> logger)
    {
        _next = next;
        _redisDb = redisDb;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // Skip blacklist check for specific endpoints
            var requestPath = context.Request.Path.Value?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(requestPath) && SkipBlacklistEndpoints.Any(endpoint => 
                requestPath.Equals(endpoint.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogDebug("Skipping token blacklist check for endpoint: {Path}", requestPath);
                await _next(context);
                return;
            }

            try
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // Parse JWT without signature validation (performance optimization)
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwt = handler.ReadJwtToken(token);
                    var jti = jwt.Id; // JWT ID

                    // Simple blacklist check by JTI
                    if (!string.IsNullOrWhiteSpace(jti) && await IsTokenBlacklistedAsync(jti))
                    {
                        _logger.LogInformation("Blocked blacklisted token {Jti}", jti);
                        await WriteUnauthorizedResponseAsync(context, "token_blacklisted", "Token has been revoked");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fail-safe: Log error but continue processing
                // Don't block requests due to blacklist check failures
                _logger.LogError(ex, "Error checking token blacklist. Allowing request to continue.");
            }
        }

        await _next(context);
    }

    private async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        try
        {
            var key = $"{BlacklistKeyPrefix}{jti}";
            var result = await _redisDb.StringGetAsync(key);
            return result.HasValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error checking blacklist for token {Jti}. Assuming not blacklisted.", jti);
            return false; // Fail-safe: assume not blacklisted on Redis errors
        }
    }

    private static async Task WriteUnauthorizedResponseAsync(HttpContext context, string error, string description)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            type = "https://tools.ietf.org/html/rfc7235#section-3.1", // Unauthorized
            title = "Token Authentication Failed",
            status = StatusCodes.Status401Unauthorized,
            detail = description,
            instance = context.Request.Path.Value
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
