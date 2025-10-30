using IAM.Domain.Aggregates.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .HasColumnName("ExpiresAt")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(s => s.RevokedAt)
            .HasColumnName("RevokedAt");

        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_Sessions_UserId");

        // Map TokenHash as an owned value object
        builder.OwnsOne(s => s.RefreshTokenHash, th =>
        {
            th.Property(p => p.Value)
                .HasColumnName("RefreshTokenHash")
                .HasMaxLength(512)
                .IsRequired();
        });

        builder.HasOne(session => session.User)
            .WithMany(user => user.Sessions)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


