using IAM.Domain.Aggregates.Users.Enums;

namespace IAM.Infrastructure.Configurations
{
    public class SeedUsersSettings
    {
        public const string SectionName = "SeedUsers";
        public List<SeedUserData> Users { get; set; } = [];
    }

    public class SeedUserData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}