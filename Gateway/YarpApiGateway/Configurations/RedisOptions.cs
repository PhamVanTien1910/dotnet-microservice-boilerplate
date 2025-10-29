using System.ComponentModel.DataAnnotations;

namespace YarpApiGateway.Configurations;

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 10;

    public bool Enabled { get; set; } = true;
}
