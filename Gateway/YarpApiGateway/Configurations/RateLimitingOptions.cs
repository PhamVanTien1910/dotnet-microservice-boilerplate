namespace YarpApiGateway.Configurations;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;

    public FixedWindowOptions FixedWindow { get; set; } = new();

    public class FixedWindowOptions
    {
        [Range(1, 3600)]
        public int WindowSeconds { get; set; } = 10;

        [Range(1, 10000)]
        public int PermitLimit { get; set; } = 5;

        [Required]
        public string PolicyName { get; set; } = "fixed";
    }
}
