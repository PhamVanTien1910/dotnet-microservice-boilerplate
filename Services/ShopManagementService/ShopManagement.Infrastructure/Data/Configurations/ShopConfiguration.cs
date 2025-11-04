using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.Aggregates.Entities;

namespace ShopManagement.Infrastructure.Data.Configurations
{
    public class ShopConfiguration : IEntityTypeConfiguration<Shop>
    {
        public void Configure(EntityTypeBuilder<Shop> builder)
        {
            builder.ToTable("Shops");

            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).HasMaxLength(1000);
            builder.Property(s => s.LogoUrl).HasMaxLength(500);

            builder.HasMany(p => p.Locations)
                        .WithOne(l => l.Shop)
                        .HasForeignKey(l => l.ShopId)
                        .OnDelete(DeleteBehavior.Cascade);

        }
    }
}