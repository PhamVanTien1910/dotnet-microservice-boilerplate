namespace ShopManagement.Infrastructure.Configurations
{
    public class MinioSettings
    {
        public const string SectionName = "Minio";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
    }
}