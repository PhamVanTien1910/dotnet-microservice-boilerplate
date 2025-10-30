using System.Reflection;
using IAM.Domain.Aggregates.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.Data
{
    public class IAMDbContext : DbContext
    {
        public IAMDbContext(DbContextOptions<IAMDbContext> options) : base(options)
        {
        }

        // Aggregate root DbSets
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
