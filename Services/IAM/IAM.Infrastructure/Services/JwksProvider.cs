using System.Security.Cryptography;
using IAM.Application.Common.Interfaces;
using IAM.Application.Common.Models;
using IAM.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IAM.Infrastructure.Services
{
    public class JwksProvider : IJwksProvider
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwksProvider> _logger;

        public JwksProvider(IOptions<JwtSettings> jwtSettings, ILogger<JwksProvider> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public JwksDocument? GetJwksDocument()
        {
            try
            {
                var publicKeyPem = _jwtSettings.PublicKeyPem;
                if (string.IsNullOrWhiteSpace(publicKeyPem))
                {
                    return null;
                }

                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem);
                var parameters = rsa.ExportParameters(false);

                var n = Base64UrlEncode(parameters.Modulus!);
                var e = Base64UrlEncode(parameters.Exponent!);

                var jwk = new JsonWebKey
                {
                    Kty = "RSA",
                    Use = "sig",
                    Alg = "RS256",
                    Kid = _jwtSettings.KeyId,
                    N = n,
                    E = e
                };

                return new JwksDocument { Keys = new[] { jwk } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build JWKS document");
                return null;
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
