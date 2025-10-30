using BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.Options;
using IAM.Infrastructure.Configurations;
using IAM.Application.Common.Interfaces;

namespace IAM.Infrastructure.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private readonly BCryptSettings _options;

        public BCryptPasswordHasher(IOptions<BCryptSettings> options)
        {
            _options = options.Value;
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new BadRequestException("Password cannot be null or empty");

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(_options.SaltRounds));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new BadRequestException("Password cannot be null or empty");

            if (string.IsNullOrWhiteSpace(hashedPassword))
                throw new BadRequestException("Hashed password cannot be null or empty");

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // If verification fails due to invalid hash format, return false
                return false;
            }
        }
    }
}
