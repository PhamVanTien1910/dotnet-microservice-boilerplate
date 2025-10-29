using System.IdentityModel.Tokens.Jwt;
using BuildingBlocks.Api.Configurations;
using BuildingBlocks.Api.Services;
using BuildingBlocks.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwksAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtAuthenticationOptions>? configureOptions = null)
    {
        // Configure options
        var options = new JwtAuthenticationOptions();
        configureOptions?.Invoke(options);

        // Preserve original JWT claim types if requested
        if (options.ClearInboundClaimTypeMap)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        if (options.DisableInboundClaimMapping)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }

        // Configure HTTP client for JWKS
        services.AddHttpClient("jwks-client", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(options.HttpClientTimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        });

        // Configure settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<JwksSettings>(configuration.GetSection(JwksSettings.SectionName));

        // Register JWKS key resolver
        services.AddSingleton<IJwksKeyResolver, JwksKeyResolver>();

        // Get and validate JWT settings
        var jwtSettings = GetAndValidateJwtSettings(configuration);

        // Configure authentication
        services.AddAuthentication(authOptions =>
        {
            authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            authOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(jwtOptions =>
        {
            jwtOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
            jwtOptions.SaveToken = options.SaveToken;
            jwtOptions.IncludeErrorDetails = options.IncludeErrorDetails;

            jwtOptions.TokenValidationParameters = CreateTokenValidationParameters(jwtSettings);
            jwtOptions.Events = CreateJwtBearerEvents();

            // Configure token handlers if inbound claim mapping should be disabled
            if (options.DisableInboundClaimMapping)
            {
                jwtOptions.TokenHandlers.Clear();
                jwtOptions.TokenHandlers.Add(new JwtSecurityTokenHandler()
                {
                    MapInboundClaims = false
                });
            }
        });

        return services;
    }

    public static void ConfigureJwtBearerWithJwks(this IServiceProvider serviceProvider)
    {
        var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var keyResolver = serviceProvider.GetRequiredService<IJwksKeyResolver>();

        void ConfigureResolver(JwtBearerOptions options)
        {
            options.TokenValidationParameters.IssuerSigningKeyResolver =
                (token, securityToken, kid, parameters) =>
                {
                    return keyResolver.ResolveSigningKeysAsync(token, securityToken, kid, parameters)
                        .GetAwaiter().GetResult();
                };
        }

        // Configure current options
        var currentOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        ConfigureResolver(currentOptions);

        // Re-configure when options change at runtime
        jwtBearerOptions.OnChange((options, name) =>
        {
            if (string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                ConfigureResolver(options);
            }
        });
    }

    private static void ValidateJwtSettings(JwtSettings jwtSettings)
    {
        if (string.IsNullOrEmpty(jwtSettings.Issuer))
            throw new InvalidOperationException("JWT Issuer is not configured.");

        if (string.IsNullOrEmpty(jwtSettings.Audience))
            throw new InvalidOperationException("JWT Audience is not configured.");

        if (jwtSettings.AccessTokenExpirationMinutes <= 0)
            throw new InvalidOperationException("JWT Expiration must be greater than 0.");
    }

    private static JwtSettings GetAndValidateJwtSettings(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        ValidateJwtSettings(jwtSettings);
        return jwtSettings;
    }

    private static TokenValidationParameters CreateTokenValidationParameters(JwtSettings jwtSettings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            RequireSignedTokens = true,
            ValidateActor = false,
            ValidateTokenReplay = false
        };
    }

    private static JwtBearerEvents CreateJwtBearerEvents()
    {
        return new JwtBearerEvents
        {
            OnAuthenticationFailed = HandleAuthenticationFailed,
            OnTokenValidated = HandleTokenValidated,
        };
    }

    private static Task HandleAuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<JwtBearerEvents>>();

        logger.LogWarning(
            context.Exception,
            "JWT Authentication failed for request {Path}. Exception: {ExceptionType} - {ExceptionMessage}",
            context.Request.Path,
            context.Exception.GetType().Name,
            context.Exception.Message
        );

        context.Fail("Authentication failed");
        return Task.CompletedTask;
    }

    private static Task HandleTokenValidated(TokenValidatedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<JwtBearerEvents>>();
        logger.LogInformation("Token validated for user {UserId} at path {Path}",
            context.Principal?.Identity?.Name, context.Request.Path);
        return Task.CompletedTask;
    }
}
