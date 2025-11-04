
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.Aggregates.Entities;

namespace ShopManagement.Infrastructure.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("Locations");

            builder.HasKey(l => l.Id);
            
            builder.Property(l => l.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
            builder.Property(l => l.IsActive).IsRequired();

            builder.OwnsOne(l => l.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("Street").IsRequired().HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("City").IsRequired().HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("State").IsRequired().HasMaxLength(100);
            });

            builder.OwnsOne(l => l.GpsCoordinate, gps =>
            {
                gps.Property(g => g.Latitude).HasColumnName("Latitude").IsRequired();
                gps.Property(g => g.Longitude).HasColumnName("Longitude").IsRequired();
            });

            builder.OwnsOne(l => l.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(15).IsRequired();
            });


            builder.HasOne(l => l.Shop)
                .WithMany(s => s.Locations)
                .HasForeignKey(l => l.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}