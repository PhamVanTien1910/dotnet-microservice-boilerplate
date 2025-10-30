using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IAM.Infrastructure.Data
{
    public class IAMDbContextFactory : IDesignTimeDbContextFactory<IAMDbContext>
    {
        public IAMDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IAMDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=BoilerplateIAMDb;Username=postgres;Password=postgres");

            return new IAMDbContext(optionsBuilder.Options);
        }
    }
}