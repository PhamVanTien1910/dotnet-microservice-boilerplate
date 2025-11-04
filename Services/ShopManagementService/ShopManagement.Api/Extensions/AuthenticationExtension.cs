using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShopManagement.Application.Utils;
using ShopManagement.Application.Common.Constants;

namespace ShopManagement.Api.Extensions
{
    public static class AuthenticationExtension
    {
        public static void AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var rsaKey = RsaKeyHelper.LoadPublicKey(configuration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = rsaKey,
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                
            });
        }
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserOnly", policy => policy.RequireRole(UserRole.User));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin));
                options.AddPolicy("EmailConfirmed", policy => policy.RequireClaim("EmailConfirmed", "True"));

            });
        }
        public static void AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddJwtBearerAuthentication(configuration);
            services.AddAuthorizationPolicies();
        }
    }
}