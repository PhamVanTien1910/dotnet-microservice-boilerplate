using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.Data;

namespace ShopManagement.Api.Extensions;
public static class MigrationDatabaseExtension
{
    public static async Task<IServiceProvider> ApplyMigrationAsync(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await dbContext.Database.MigrateAsync();
        }
        return serviceProvider;
    }
}
