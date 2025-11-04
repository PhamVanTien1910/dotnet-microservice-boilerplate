using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace ShopManagement.Application.Utils;

public static class RsaKeyHelper
{
    public static RsaSecurityKey LoadPublicKey(IConfiguration configuration)
    {

        string? keyContent = configuration["JWT_PUBLIC_KEY"];
        if (string.IsNullOrWhiteSpace(keyContent))
        {
            throw new InvalidOperationException("Public key is not set in configuration");
        }
        var rsa = RSA.Create();
        rsa.ImportFromPem(keyContent);
        return new RsaSecurityKey(rsa);
    }
}
