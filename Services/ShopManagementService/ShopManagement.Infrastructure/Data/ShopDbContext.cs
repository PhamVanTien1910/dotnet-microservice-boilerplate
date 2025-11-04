using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.Aggregates.Entities;

namespace ShopManagement.Infrastructure.Data
{
    public class ShopDbContext : DbContext
    {
        public DbSet<Shop> Shops { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;

        public ShopDbContext(DbContextOptions<ShopDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDbContext).Assembly);
        }
    }
}