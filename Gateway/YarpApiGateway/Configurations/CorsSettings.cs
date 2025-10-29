namespace YarpApiGateway.Configurations;

public class CorsSettings
{
    public const string SectionName = "CorsSettings";
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowAnyOriginForDocker { get; set; } = false;
    public int PreflightMaxAgeSeconds { get; set; } = 86400;
}