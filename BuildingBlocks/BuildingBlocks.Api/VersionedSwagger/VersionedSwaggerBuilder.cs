using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;

namespace BuildingBlocks.Api.VersionedSwagger;

public static class VersionedSwaggerBuilder
{
    public static IServiceCollection AddVersionedSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        // base registration
        services.AddSwaggerGen();

        // configure per-version docs and security
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();

        return services;
    }

    private sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly IOptions<VersionedSwaggerOptions> _swagger;

        public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider, IOptions<VersionedSwaggerOptions> swagger)
        {
            _provider = provider;
            _swagger = swagger;
        }

        public void Configure(SwaggerGenOptions c)
        {
            var title = string.IsNullOrWhiteSpace(_swagger.Value.Title) ? "API" : _swagger.Value.Title!;
            var description = _swagger.Value.Description;

            foreach (var descriptionGroup in _provider.ApiVersionDescriptions)
            {
                c.SwaggerDoc(descriptionGroup.GroupName, new OpenApiInfo
                {
                    Title = $"{title} {descriptionGroup.ApiVersion}",
                    Version = descriptionGroup.ApiVersion.ToString(),
                    Description = description
                });
            }

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });

            var entry = Assembly.GetEntryAssembly();
            if (entry is not null)
            {
                var xml = Path.Combine(AppContext.BaseDirectory, $"{entry.GetName().Name}.xml");
                if (File.Exists(xml))
                    c.IncludeXmlComments(xml, includeControllerXmlComments: true);
            }
        }
    }
}
