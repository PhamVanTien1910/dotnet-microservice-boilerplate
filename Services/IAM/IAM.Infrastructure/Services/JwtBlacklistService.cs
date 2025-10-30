using IAM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IAM.Infrastructure.Services
{
    public class JwtBlacklistService : IJwtBlacklistService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<JwtBlacklistService> _logger;

        // Simple Redis key pattern for blacklisted tokens
        private const string BlacklistKeyPrefix = "jwt_blacklist:";

        public JwtBlacklistService(
            IConnectionMultiplexer redis,
            ILogger<JwtBlacklistService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = _redis.GetDatabase();
        }

        public async Task BlacklistTokenAsync(string jti, TimeSpan expiry)
        {
            if (string.IsNullOrWhiteSpace(jti) || expiry <= TimeSpan.Zero)
            {
                _logger.LogDebug("Skipping token blacklisting: invalid JTI or expiry");
                return;
            }

            try
            {
                var key = $"{BlacklistKeyPrefix}{jti}";
                // Store minimal data - just mark as blacklisted with TTL
                await _database.StringSetAsync(key, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), expiry);

                _logger.LogInformation("Token {Jti} blacklisted for {Expiry}", jti, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to blacklist token {Jti}. System remains operational.", jti);
                // Don't throw - blacklisting failure shouldn't break the system
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti))
                return false;

            try
            {
                var key = $"{BlacklistKeyPrefix}{jti}";
                var result = await _database.StringGetAsync(key);

                var isBlacklisted = result.HasValue;
                if (isBlacklisted)
                {
                    _logger.LogDebug("Token {Jti} found in blacklist", jti);
                }

                return isBlacklisted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check blacklist for token {Jti}. Allowing request to continue.", jti);
                // Fail-safe: Return false on Redis errors to avoid blocking valid tokens
                return false;
            }
        }
    }
}
