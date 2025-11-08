namespace PaymentService.API.Extensions;

public static class AuthorizationExtension
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            })
            .AddPolicy("UserOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("User");
            })
            .AddPolicy("UserOrAdminPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("User", "Admin");
            })
            .AddPolicy("EmailConfirmed", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("EmailConfirmed", "True");
            });

        return services;
    }
}