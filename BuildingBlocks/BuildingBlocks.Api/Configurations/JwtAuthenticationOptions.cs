namespace BuildingBlocks.Api.Configurations
{
    public class JwtAuthenticationOptions
    {
        public string UserAgent { get; set; } = "Boilerplate-Service/1.0";

        public bool RequireHttpsMetadata { get; set; } = true;

        public bool IncludeErrorDetails { get; set; } = false;

        public bool SaveToken { get; set; } = false;

        public int HttpClientTimeoutSeconds { get; set; } = 10;

        public bool ClearInboundClaimTypeMap { get; set; } = true;

        public bool DisableInboundClaimMapping { get; set; } = false;
    }
}