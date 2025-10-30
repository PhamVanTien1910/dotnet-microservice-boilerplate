using System.Security.Cryptography;
using IAM.Infrastructure.Configurations;
using Microsoft.IdentityModel.Tokens;

namespace IAM.Api.Utils;

public static class RsaKeyHelper
{
    public static RsaSecurityKey LoadPublicKeyFromSettings(JwtSettings jwtSettings)
    {
        string publicKeyPem;

        // Try to get PEM content directly from settings first
        if (!string.IsNullOrWhiteSpace(jwtSettings.PublicKeyPem))
        {
            publicKeyPem = jwtSettings.PublicKeyPem;
        }
        // If PEM content is not available, try to load from file path
        else if (!string.IsNullOrWhiteSpace(jwtSettings.PublicKeyPath))
        {
            if (!File.Exists(jwtSettings.PublicKeyPath))
            {
                throw new InvalidOperationException($"JWT public key file not found at path: {jwtSettings.PublicKeyPath}");
            }

            try
            {
                publicKeyPem = File.ReadAllText(jwtSettings.PublicKeyPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read JWT public key from file: {jwtSettings.PublicKeyPath}", ex);
            }
        }
        else
        {
            throw new InvalidOperationException("JWT public key is not configured. Please provide either PublicKeyPem content or PublicKeyPath.");
        }

        if (string.IsNullOrWhiteSpace(publicKeyPem))
        {
            throw new InvalidOperationException("JWT public key PEM content is empty.");
        }

        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            
            return new RsaSecurityKey(rsa)
            {
                KeyId = jwtSettings.KeyId
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load JWT public key from PEM content.", ex);
        }
    }

    public static RsaSecurityKey LoadPrivateKeyFromSettings(JwtSettings jwtSettings)
    {
        string privateKeyPem;

        // Try to get PEM content directly from settings first
        if (!string.IsNullOrWhiteSpace(jwtSettings.PrivateKeyPem))
        {
            privateKeyPem = jwtSettings.PrivateKeyPem;
        }
        // If PEM content is not available, try to load from file path
        else if (!string.IsNullOrWhiteSpace(jwtSettings.PrivateKeyPath))
        {
            if (!File.Exists(jwtSettings.PrivateKeyPath))
            {
                throw new InvalidOperationException($"JWT private key file not found at path: {jwtSettings.PrivateKeyPath}");
            }

            try
            {
                privateKeyPem = File.ReadAllText(jwtSettings.PrivateKeyPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read JWT private key from file: {jwtSettings.PrivateKeyPath}", ex);
            }
        }
        else
        {
            throw new InvalidOperationException("JWT private key is not configured. Please provide either PrivateKeyPem content or PrivateKeyPath.");
        }

        if (string.IsNullOrWhiteSpace(privateKeyPem))
        {
            throw new InvalidOperationException("JWT private key PEM content is empty.");
        }

        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);
            
            return new RsaSecurityKey(rsa)
            {
                KeyId = jwtSettings.KeyId
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load JWT private key from PEM content.", ex);
        }
    }
}
