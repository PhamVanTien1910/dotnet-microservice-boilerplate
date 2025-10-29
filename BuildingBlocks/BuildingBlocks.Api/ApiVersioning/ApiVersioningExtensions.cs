using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Api.ApiVersioning
{
    public static class ApiVersioningExtensions
    {
        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            services
                .AddApiVersioning(o =>
                {
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ReportApiVersions = true;
                })
                .AddApiExplorer(o =>
                {
                    o.GroupNameFormat = "'v'VVV"; // v1, v1.1, v2
                    o.SubstituteApiVersionInUrl = true;
                });

            return services;
        }
    }
}