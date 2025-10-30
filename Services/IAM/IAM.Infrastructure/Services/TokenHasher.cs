using System.Security.Cryptography;
using System.Text;
using IAM.Application.Common.Interfaces;

namespace IAM.Infrastructure.Services
{
    public class TokenHasher : ITokenHasher
    {
        public string HashToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}


