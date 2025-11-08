using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Data;

namespace PaymentService.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task<IHost> MigrateDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PaymentDbContext>>();
        
        try
        {
            logger.LogInformation("Starting database migration...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed: {Message}", ex.Message);
            throw; 
        }
        
        return host;
    }
}