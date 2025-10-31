namespace IAM.Api.Extensions
{
    public static class CustomAuthorizationExtension
    {
        public static void AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy("AdminPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
        }
    }
}