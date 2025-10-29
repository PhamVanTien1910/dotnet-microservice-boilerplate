using BuildingBlocks.Api.Configurations;
using BuildingBlocks.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace BuildingBlocks.Api.Services
{
    public class JwksKeyResolver : IJwksKeyResolver
    {
        private readonly IDatabase? _redisDatabase;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JwksSettings _jwksSettings;
        private readonly ILogger<JwksKeyResolver> _logger;
        private readonly bool _redisAvailable;

        public JwksKeyResolver(
            IHttpClientFactory httpClientFactory,
            IOptions<JwksSettings> jwksOptions,
            ILogger<JwksKeyResolver> logger,
            IConnectionMultiplexer? redis = null)
        {
            _httpClientFactory = httpClientFactory;
            _jwksSettings = jwksOptions.Value;
            _logger = logger;

            // Redis is optional - fallback to no caching if not available
            _redisAvailable = redis?.IsConnected == true;
            _redisDatabase = redis?.GetDatabase();

            if (!_redisAvailable)
            {
                _logger.LogWarning("Redis is not available for JWKS caching. JWKS will be fetched on every request.");
            }
        }

        public async Task<IEnumerable<SecurityKey>> ResolveSigningKeysAsync(
            string token,
            SecurityToken securityToken,
            string kid,
            TokenValidationParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(_jwksSettings.JwksUri))
            {
                throw new InvalidOperationException("JWKS URI is not configured.");
            }

            var cacheKey = $"jwks:{_jwksSettings.JwksUri}";
            IList<SecurityKey>? keys = null;

            // Try to get from Redis cache first
            if (_redisAvailable && _redisDatabase != null)
            {
                try
                {
                    var cachedJwks = await _redisDatabase.StringGetAsync(cacheKey);
                    if (cachedJwks.HasValue)
                    {
                        _logger.LogDebug("JWKS Redis cache hit for {JwksUri}", _jwksSettings.JwksUri);
                        var jwks = new JsonWebKeySet(cachedJwks!);
                        keys = jwks.GetSigningKeys();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve JWKS from Redis cache, will fetch from source");
                }
            }

            // If not in cache or Redis failed, fetch from JWKS endpoint
            if (keys is null || keys.Count == 0)
            {
                _logger.LogDebug("JWKS cache miss, fetching from {JwksUri}", _jwksSettings.JwksUri);

                try
                {
                    var client = _httpClientFactory.CreateClient("jwks-client");
                    string jwksJson = null!;
                    
                    // Try fetch, with one retry on transient failures
                    for (int attempt = 1; attempt <= 2; attempt++)
                    {
                        try
                        {
                            jwksJson = await client.GetStringAsync(_jwksSettings.JwksUri);
                            break;
                        }
                        catch (Exception fetchEx) when (attempt == 1)
                        {
                            _logger.LogWarning(fetchEx, "First attempt to fetch JWKS failed (attempt {Attempt}), retrying...", attempt);
                            await Task.Delay(500);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(jwksJson))
                    {
                        throw new InvalidOperationException($"Failed to fetch JWKS from {_jwksSettings.JwksUri} after retries.");
                    }

                    var jwks = new JsonWebKeySet(jwksJson);
                    keys = jwks.GetSigningKeys();

                    // Cache in Redis for 5 minutes
                    if (_redisAvailable && _redisDatabase != null)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(jwksJson))
                            {
                                await _redisDatabase.StringSetAsync(cacheKey, jwksJson, TimeSpan.FromMinutes(5));
                            }
                            _logger.LogDebug("Successfully cached {KeyCount} JWKS keys in Redis", keys.Count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to cache JWKS in Redis");
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Successfully fetched {KeyCount} JWKS keys (no caching)", keys.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch JWKS from {JwksUri}", _jwksSettings.JwksUri);
                    throw new InvalidOperationException($"Unable to obtain JWKS from {_jwksSettings.JwksUri}", ex);
                }
            }

            // If we have a specific key ID, try to find a matching key
            if (keys is { Count: > 0 } && !string.IsNullOrWhiteSpace(kid))
            {
                var match = keys.FirstOrDefault(k => k.KeyId == kid);
                if (match != null)
                {
                    _logger.LogDebug("Found matching key for kid: {Kid}", kid);
                    return new[] { match };
                }

                _logger.LogWarning("No matching key found for kid: {Kid}", kid);
            }

            return keys ?? Enumerable.Empty<SecurityKey>();
        }
    }
}