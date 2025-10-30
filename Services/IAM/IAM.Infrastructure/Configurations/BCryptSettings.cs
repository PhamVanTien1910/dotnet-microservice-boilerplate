using System.ComponentModel.DataAnnotations;

namespace IAM.Infrastructure.Configurations
{
    public class BCryptSettings
    {
        public const string SectionName = "BCrypt";
        [Required]
        [Range(4, 31, ErrorMessage = "Salt rounds must be between 4 and 31.")]
        public int SaltRounds { get; set; } = 10;
    }
}