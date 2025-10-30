using System.ComponentModel.DataAnnotations;

namespace IAM.Infrastructure.Configurations
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        [Range(1, int.MaxValue)]
        public int RefreshTokenExpirationDays { get; set; } = 7;
        [Range(1, int.MaxValue)]
        public int RefreshTokenExpirationDaysRememberMe { get; set; } = 30;

        [Required]
        public string KeyId { get; set; } = string.Empty;

        public string PrivateKeyPem { get; set; } = string.Empty;

        public string PublicKeyPem { get; set; } = string.Empty;

        public string? PrivateKeyPath { get; set; }

        public string? PublicKeyPath { get; set; }
    }
}