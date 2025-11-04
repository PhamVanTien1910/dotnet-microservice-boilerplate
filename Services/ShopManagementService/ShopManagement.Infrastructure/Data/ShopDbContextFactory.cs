using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopManagement.Infrastructure.Data
{
    public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
    {
        public ShopDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            
            optionsBuilder.UseNpgsql("Host=localhost;Database=shop_db;Username=postgres;Password=postgres");

            return new ShopDbContext(optionsBuilder.Options);
        }
    }
}