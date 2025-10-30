using IAM.Infrastructure.Data;

namespace IAM.Api.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task<WebApplication> MigrateAndSeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.MigrateAndSeedAsync();
            return app;
        }
    }
}

