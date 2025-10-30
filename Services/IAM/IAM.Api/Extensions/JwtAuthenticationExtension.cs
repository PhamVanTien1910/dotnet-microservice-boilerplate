using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using IAM.Api.Utils;
using IAM.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace IAM.Api.Extensions;

public static class JwtAuthenticationExtensionExtension
{
    public static IServiceCollection AddJwtAuthenticationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        // Preserve original JWT claim types (sub, role, etc.)
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // Load JWT settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        var rsaKey = RsaKeyHelper.LoadPublicKeyFromSettings(jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = false;
            options.IncludeErrorDetails = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = rsaKey,

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

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = HandleAuthenticationFailed,
                OnTokenValidated = HandleTokenValidated,
                OnChallenge = HandleChallenge
            };
        });

        return services;
    }

    private static Task HandleAuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<JwtBearerEvents>>();

        logger.LogWarning(context.Exception,
            "JWT Authentication failed for request {Path}. Exception: {ExceptionType} - {ExceptionMessage}",
            context.Request.Path,
            context.Exception.GetType().Name,
            context.Exception.Message);

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

    private static Task HandleChallenge(JwtBearerChallengeContext context)
    {
        // Return ProblemDetails format like GlobalExceptionHandler
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1", // Unauthorized
            Title = "Authentication failed",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "The request requires valid authentication credentials.",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";
        
        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.WriteAsync(jsonResponse);
        context.HandleResponse(); // Suppress default challenge response

        return Task.CompletedTask;
    }
}
