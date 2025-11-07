using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;
using PaymentService.Domain.Aggregates.PaymentAggregate.ValueObjects;

namespace PaymentService.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BookingId)
            .IsRequired(false);

        builder.Property(p => p.ShopId)
            .IsRequired(false);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ProcessedAt)
            .IsRequired(false);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.Currency)
            .HasConversion(
                currency => currency.Code,
                code => Currency.Create(code))
            .HasColumnName("CurrencyCode")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.ModifiedAt)
            .IsRequired(false);

        builder.HasIndex(p => p.BookingId)
            .IsUnique();

        builder.HasIndex(p => p.UserId);

        builder.HasIndex(p => p.Status);
    }
}