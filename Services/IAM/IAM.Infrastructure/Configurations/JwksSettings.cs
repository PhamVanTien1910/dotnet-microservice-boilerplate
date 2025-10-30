namespace IAM.Infrastructure.Configurations
{
    public class JwksSettings
    {
        public const string SectionName = "JwksSettings";
        public string JwksUri { get; set; } = string.Empty;
    }
}